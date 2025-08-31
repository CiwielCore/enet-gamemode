using eNetwork.Admin.Reports;
using eNetwork.Framework;
using eNetwork.Framework.Enums;
using eNetwork.Game.Banks.Player;
using eNetwork.Game.Casino;
using eNetwork.Game.Characters.Customization;
using eNetwork.Houses;
using eNetwork.Inv;
using eNetwork.Inv.Items;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace eNetwork.Game.Characters.Methods
{
    public static class SpawnCharacter
    {
        private static readonly Logger Logger = new Logger("spawn-character");

        /// <summary>
        ///     Спавним игрового персонажа после авторизации и навешиваем на него различные данные
        /// </summary>
        /// <param name="data">Данные о персонаже</param>
        /// <param name="player">Экземпляр игрового персонажа</param>
        /// <param name="type">Место спавна</param>
        public static async Task Spawn(this CharacterData data, ENetPlayer player, string type)
        {
            //NAPI.Task.Run(() =>
            //{
            try
            {
                if (!data.CustomizationData.Created)
                {
                    player.SendToCreator(String.IsNullOrEmpty(player.Name));
                    return;
                }


                var storage = await Inventory.LoadInventory(data.UUID);

                //// Создаем инвентарь
                //if (Inventory.CreateInventory(player.GetUUID()))
                //{
                //    if (data.CustomizationData.Clothes.Top.Variation != Configs.CustomizationConfig.EmptySlots[data.CustomizationData.Gender][ItemId.Top][0])
                //    {
                //        var itemData = JsonConvert.SerializeObject(new InvClothesData(data.CustomizationData.Gender, data.CustomizationData.Clothes.Top.Variation, data.CustomizationData.Clothes.Top.Texture));
                //        Inventory.TryAdd(player, new Item(ItemId.Top, 1, true, data: itemData), -(int)ItemId.Top);
                //    }
                //    if (data.CustomizationData.Clothes.Leg.Variation != Configs.CustomizationConfig.EmptySlots[data.CustomizationData.Gender][ItemId.Pants][0])
                //    {
                //        var itemData = JsonConvert.SerializeObject(new InvClothesData(data.CustomizationData.Gender, data.CustomizationData.Clothes.Leg.Variation, data.CustomizationData.Clothes.Leg.Texture));
                //        Inventory.TryAdd(player, new Item(ItemId.Pants, 1, true, data: itemData), -(int)ItemId.Pants);
                //    }
                //    if (data.CustomizationData.Clothes.Feet.Variation != Configs.CustomizationConfig.EmptySlots[data.CustomizationData.Gender][ItemId.Shoes][0])
                //    {
                //        var itemData = JsonConvert.SerializeObject(new InvClothesData(data.CustomizationData.Gender, data.CustomizationData.Clothes.Feet.Variation, data.CustomizationData.Clothes.Feet.Texture));
                //        Inventory.TryAdd(player, new Item(ItemId.Shoes, 1, true, data: itemData), -(int)ItemId.Shoes);
                //    }

                //    Inventory.TryAdd(player, new Item(ItemId.IdCard, 1));
                //}

                //if (Inventory.GetInventory(player.GetUUID(), out var storage))
                //{

                //переделать на создание персонажа
                if(type == "CreateCharacter")
                {
                    if (storage.Items.Exists(i => i.Type == ItemId.IdCard) is false)
                    {
                        var item = TypedItems.Get(new Item(ItemId.IdCard, 1));
                        item.OwnerType = ItemOwnerType.Player;
                        item.OwnerId = data.UUID;
                        await Inventory.CreateNewItem(player, item);
                    }
                }
                

                //хз зачем это
                foreach (var item in storage.Items)
                {
                    if (!item.IsActive) continue;
                    ItemType itemType = InvItems.GetType(item.Type);
                    if (itemType == ItemType.Tools || itemType == ItemType.Weapon || itemType == ItemType.Melee || itemType == ItemType.Consumable)
                        item.IsActive = false;
                }
                //}
                var backpack = (BackpackItem)storage.Items.FirstOrDefault(el=>el is BackpackItem && el.Slot == -((int)el.Type));
                if(backpack != null)
                {
                    await InvBackpack.LoadItems(backpack);
                }
                InvInterface.SendItems(player);

                // Заранее проверяем является ли игрок лидером

                IsLeader(player);

                // Ставим команды в чат
                ChatHandler.LoadChat(player);
                ReportsManager.LoadPlayer(player);

                player.ApplyCustomization();

                World.WeatherHandler.LoadWeather(player);
                Player.Panel.Init.FirstLoad(player);
                NAPI.Task.Run(() =>
                {
                    // Ставим дату статика и динамика
                    player.SetSharedData("DYNAMIC_ID", player.Value);
                    player.SetSharedData("STATIC_ID", data.UUID);
                    player.SetSharedData("player.status", (int)data.Status);

                    player.SetSharedData("IS_FREEZED", false);
                    player.SetSharedData("IS_INVISIBILITY", false);
                    player.SetSharedData("IS_GODMODE", false);
                    player.SetSharedData("TEMPORARY_NICKNAME", false);

                    player.SetSharedData("player.fraction_id", (int)data.FactionId);
                    player.SetSharedData("player.fraction_rank", data.FactionRank);
                    player.SetSharedData("player.family_id", 0);

                    player.LoadCasinoData();

                    player.SetSessionData(new SessionData());

                    if (Demorgan.DemorganRepository.Instance.IsCharacterInDemorgan(data.UUID))
                        type = "demorgan";

                    switch (type)
                    {
                        default:
                            Vector3 spawnRandom = CharacterManager.GetRandomSpawn();
                            NAPI.Entity.SetEntityPosition(player, spawnRandom);
                            NAPI.Entity.SetEntityDimension(player, 0);
                            break;

                        case "Faction":
                            if (data.FactionId < 0) return;
                            //var factionData = Factions.Factionnager.GetFaction(data.Faction);
                            //if (factionData is null) return;

                            //factionData.Spawn.Set(player);
                            NAPI.Entity.SetEntityDimension(player, 0);
                            break;

                        case "House":
                            var house = HousesManager.GetPlayerHouse(player.GetUUID());
                            if (house is null)
                            {
                                NAPI.Entity.SetEntityPosition(player, data.LastVector);
                                NAPI.Entity.SetEntityDimension(player, 0);
                            }
                            else
                            {
                                house.Position.Set(player);
                            }
                            break;

                        case "Last":
                            NAPI.Entity.SetEntityPosition(player, data.LastVector);
                            NAPI.Entity.SetEntityDimension(player, 0);
                            break;

                        case "demorgan":
                            Demorgan.DemorganManager.Instance.PutPlayerInDemorgan(player,
                                Demorgan.DemorganRepository.Instance.GetDemorganInfo(data.UUID));
                            break;
                    }

                    ClientEvent.Event(player, "client.spawnSelector.close");

                    ClientEvent.Event(player, "client.ready");
                    ClientEvent.Event(player, "server.world.ready");

                    HousesManager.Load(player);

                    Inventory.LoadFastSlot(player);

                    if (player.GetBankAccount(out var bankAccount))
                        player.SetSharedData("player.bank", bankAccount.Balance);

                    if (type == "default")
                        ENet.Chat.SendMessage(player, $"Добро пожаловать в штат {Engine.Config.ServerName}, {player.GetName()}!", new ChatAddition(ChatType.System));
                    else
                        ENet.Chat.SendMessage(player, $"С возвращением в штат {Engine.Config.ServerName}, {player.GetName()}!", new ChatAddition(ChatType.System));

                    Task.Run(async() =>
                    {
                        await Quests.QuestTasksHandler.Instance.InitPlayer(player);
                        await Services.VipServices.VipService.Instance.InitPlayer(player);
                        await Services.DrivingLicensing.DrivingLicenseService.Instance.InitPlayer(player);
                    });

                    data.IsSpawned = true;
                });
                
            }
            catch (Exception e) { Logger.WriteError("Spawn", e); }
            //});
        }

        /// <summary>
        ///     Иницилизируем CharacterData.IsLeader при спавне
        /// </summary>
        /// <param name="player">Объект игрока</param>
        private static void IsLeader(ENetPlayer player)
        {
            if (player.CharacterData.FactionId <= 0) return;

            DataTable resultFactionMaximumRank = ENet.Database.ExecuteRead($"SELECT MAX(`lvl`) FROM `factions_ranks` WHERE factionid=1");

            if (resultFactionMaximumRank is null) return;

            foreach (DataRow factionRankRow in resultFactionMaximumRank.Rows)
            {
                player.CharacterData.isLeader = player.CharacterData.FactionRank == Convert.ToInt32(factionRankRow.ItemArray[0]);
            }
        }
    }
}