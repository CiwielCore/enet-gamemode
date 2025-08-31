using eNetwork.Framework;
using eNetwork.Game;
using eNetwork.Inv;
using eNetwork.Jobs.Fishing.Classes;
using eNetwork.Jobs.Fishing.Data;
using GTANetworkAPI;
using NeptuneEvo.Jobs.Fishing.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Jobs.Fishing
{
    public class FishingManager
    {
        private static readonly Logger Logger = new Logger(nameof(FishingManager));

        #region Main methods

        public static void Initialize()
        {
            try
            {
                Config.Initialize();
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public static void OnPayday()
            => Config.STORES.ForEach(buyer => buyer.Update());

        #endregion Main methods

        #region Fishing functions
        public static void UseRod(ENetPlayer player, Item item)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData) || !Config.RODS_DATA.TryGetValue(item.Type, out var rodData) || !Inventory.GetInventory(player, out var items)) return;

                if (!player.HasData("fish.spot"))
                {
                    player.SendError($"Вы не в зоне рыболовства!", 3000);
                    return;
                }

                if (sessionData.TimersData.Fishing != null)
                {
                    player.SendError("Вы уже рыбачите!", 3000);
                    return;
                }

                if (!sessionData.WorkData.FishingWorkData.CanDo)
                {
                    player.SendError($"Чтобы рыбачить нужно смотреть в сторону воды!", 3000);
                    return;
                }

                int countOfBait = Inventory.GetCount(items, ItemId.Bait);
                if (countOfBait < 1)
                {
                    player.SendError("У вас нет наживки!", 3000);
                    return;
                }

                Inventory.RemoveItem(player, ItemId.Bait, 1);

                item.IsActive = true;
                sessionData.WorkData.FishingWorkData.CurrentRod = item;

                int waitingTime = ENet.Random.Next(rodData.MinTime, rodData.MaxTime);
                sessionData.TimersData.Fishing = Timers.StartOnce(waitingTime * 1000, () => StartMinigame(player));

                player.PlayAnimation("amb@world_human_stand_fishing@base", "base", 1);
                player.AddAttachment("rod");
            }
            catch (Exception ex) { Logger.WriteError("UseRod: " + ex.ToString()); }
        }

        public static void Reset(ENetPlayer player, bool isLoose = false)
        {
            try
            {
                if (player.SessionData.TimersData.Fishing == null)
                    return;

                if (player is null || !player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)
                    || (isLoose && sessionData.TimersData.Fishing is null && !sessionData.WorkData.FishingWorkData.MinigameStarted)) return;

                if (sessionData.TimersData.Fishing != null)
                    Timers.Stop(sessionData.TimersData.Fishing);

                sessionData.TimersData.Fishing = null;

                var activeItem = sessionData.WorkData.FishingWorkData.CurrentRod;
                if (activeItem != null)
                    activeItem.IsActive = false;

                sessionData.WorkData.FishingWorkData.CurrentRod = null;
                sessionData.WorkData.FishingWorkData.MinigameStarted = false;

                player.StopAnimation();
                ClientEvent.Event(player, "client.fishing.minigame.close");

                player.AddAttachment("rod", true);

                if (isLoose)
                    player.SendInfo("Наживка слетела");
            }
            catch (Exception ex) { Logger.WriteError("OnDead: " + ex.ToString()); }
        }

        #endregion Fishing functions

        #region Minigame

        private static void StartMinigame(ENetPlayer player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player is null || !player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData) || sessionData.TimersData.Fishing == null) return;

                    Timers.Stop(sessionData.TimersData.Fishing);
                    sessionData.TimersData.Fishing = null;

                    player.PlayAnimation("amb@world_human_stand_fishing@idle_a", "idle_c", 1);
                    ClientEvent.Event(player, "client.fishing.minigame.open");

                    player.SendInfo($"Что-то клюнуло...", 3000);
                    sessionData.WorkData.FishingWorkData.MinigameStarted = true;
                }
                catch (Exception ex) { Logger.WriteError("StartMinigame: " + ex.ToString()); }
            });
        }

        public static void EndMinigame(ENetPlayer player, bool state)
        {
            try
            {
                if (player is null || !player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)
                    || !player.GetData("fish.spot", out FishSpot fishSpot) || !sessionData.WorkData.FishingWorkData.MinigameStarted) return;

                var rodItem = sessionData.WorkData.FishingWorkData.CurrentRod;

                if (!state)
                {
                    Reset(player, true);
                    return;
                }

                if (rodItem is null || !RodData.Get(rodItem.Type, out var rodData) || !rodItem.IsActive)
                {
                    player.SendError("У вас в руках должна быть удочка", 3000);
                    return;
                }

                // TODO: Work skills
                int lvl = 6;

                FishItemData fishItem = GetRandomFish(lvl, fishSpot.Fish, rodItem.Type);
                if (fishItem is null)
                {
                    player.SendError("Вам не удалось поймать рыбу!", 3000);
                    return;
                }

                if (string.IsNullOrEmpty(rodItem.Data))
                    rodItem.Data = "100";

                rodItem.Data = Convert.ToString(Convert.ToDouble(rodItem.Data) - rodData.Wear);

                if (Convert.ToDouble(rodItem.Data) < 1)
                {
                    Inventory.RemoveItem(player, rodItem);
                    player.SendInfo("Удочка сломалась!", 3000);
                }

                if (!Inventory.CheckCanAddItem(player, new Item(fishItem.ItemId, 1)))
                {
                    player.SendError(Language.GetText(TextType.NoInventoryPlaces));
                    return;
                }

                // TODO: Work skills
                // add progress

                player.SendDone($"Вы поймали {InvItems.Get(fishItem.ItemId)?.Name}!", 3000);
                Reset(player, false);
            }
            catch (Exception ex) { Logger.WriteError("EndMinigame: " + ex.ToString()); }
        }

        #endregion Minigame

        #region Computing functions

        public static FishItemData GetRandomFish(int currentLvl, List<ItemId> fish, ItemId rodId)
        {
            try
            {
                int pool = 0;

                var fishItems = Config.FISH_ITEMS_DATA.Where(x => fish.Contains(x.ItemId) && x.MinLvl <= currentLvl && x.Rods.Contains(rodId)).ToList();
                fishItems.ForEach(item => pool += item.Chance);

                int random = ENet.Random.Next(0, pool);
                int accumulatedProbability = 0;

                foreach (var item in fishItems)
                {
                    accumulatedProbability += item.Chance;
                    if (random <= accumulatedProbability)
                        return item;
                }

                return fishItems[0];
            }
            catch (Exception ex) { Logger.WriteError("GetRandomFish: " + ex.ToString()); return null; }
        }

        #endregion Computing functions
    }
}