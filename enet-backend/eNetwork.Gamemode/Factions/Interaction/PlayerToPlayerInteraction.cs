using eNetwork.Framework;
using eNetwork.Game;
using eNetwork.Inv;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Factions.Interaction
{
    internal class PlayerToPlayerInteraction
    {
        public static void InivtePlayer(ENetPlayer player, ENetPlayer target)
        {
            // TODO: Не хватает решения, как у игрока будет появляться уведомление с выбором принимать или нет.
            bool resultFactionPermission = new FactionsManager().GetRanksPermission(player.CharacterData.FactionId, player.CharacterData.FactionRank, "Invite");

            if (!resultFactionPermission)
            {
                player.SendError("Ваш ранг не может выполнить это действие");
                return;
            }

            target.CharacterData.Faction = player.CharacterData.Faction;
            target.CharacterData.FactionRank = 1;
            target.SendInfo("Вы вступили во фракцию!");
        }

        public void UnInvitePlayer(ENetPlayer player, ENetPlayer target)
        {
            bool resultFactionPermission = new FactionsManager().GetRanksPermission(player.CharacterData.FactionId, player.CharacterData.FactionRank, "UnInvite");

            if (!resultFactionPermission && player.CharacterData.Faction != target.CharacterData.Faction && player.CharacterData.FactionRank >= target.CharacterData.FactionRank)
            {
                player.SendError("Ваш ранг не может выполнить это действие");
                return;
            }

            target.CharacterData.FactionId = 0;
            target.CharacterData.FactionRank = 0;
            target.SendInfo($"Игрок {player.CharacterData.Name} уволили вас из фракции!");
        }

        public void HandcuffOrUnHandcuffPlayer(ENetPlayer player, ENetPlayer target)
        {
            bool resultFactionPermission = new FactionsManager().GetRanksPermission(player.CharacterData.FactionId, player.CharacterData.FactionRank, "Cuff");

            if (!resultFactionPermission)
            {
                player.SendError("Ваш ранг не может выполнить это действие");
                return;
            }

            if (target.CharacterData.IsCuffed)
            {
                target.StopAnimation();
                player.ClearAttachments();
            }
            else
            {
                target.PlayAnimation("mp_arresting", "idle", 49);
                player.AddAttachment("cuffs");
            }

            foreach (ENetPlayer playersPool in ENet.Pools.GetPlayersInRadius(player.Position, 10.0f, player.Dimension))
            {
                if (target.CharacterData.IsCuffed)
                {
                    ENet.Chat.SendMessage(playersPool, $"{player.Nametag} снял наручники с {target.Nametag}");
                }
                else
                {
                    ENet.Chat.SendMessage(playersPool, $"{player.Nametag} надел наручники на {target.Nametag} ");
                }
            }
        }

        public void CrackTheHancuff(ENetPlayer player, ENetPlayer target)
        {
            if (!target.CharacterData.IsCuffed)
            {
                player.SendError("Игрок не в стяжках или не в наручниках");
                return;
            }

            target.StopAnimation();
            target.ClearAttachments();
            player.SendDone("Вы успешно срезали стяжки");
        }

        public virtual void WringingOrUnWringingPlayer(ENetPlayer player, ENetPlayer target)
        {
            if (!target.CharacterData.IsCuffed)
            {
                player.SendError("Игрок не в наручниках");
                return;
            }
            player.SetData("WRINGING_TARGET", target.CharacterData.UUID);
            ClientEvent.Event(player, "AttachPlayerToPlayer", target);
        }

        public void UnMaskTarget(ENetPlayer player, ENetPlayer target)
        {
            if (target.GetClothesDrawable(1) <= 0)
            {
                player.SendError("Игрок без маски");
            }

            Random rand = new Random();
            int result = rand.Next(1);

            if (result == 0)
            {
                player.SendError("Маска порвалась");
                return;
            }

            Inventory.AddItem(player, new Item(ItemId.Masks));
            player.SendDone("Вы успешно сняли маску");

            foreach (ENetPlayer playersPool in ENet.Pools.GetPlayersInRadius(player.Position, 10.0f, player.Dimension))
            {
                if (result == 0)
                {
                    ENet.Chat.SendMessage(playersPool, $"{player.Nametag} порвал(а) маску игрока{target.Nametag}");
                }
                else
                {
                    ENet.Chat.SendMessage(playersPool, $"{player.Nametag} снял маску игрока{target.Nametag}");
                }
            }
        }

        [CustomEvent("server.putPlayerInCar")]
        private void PutPlayerInCar(ENetPlayer player, ENetVehicle vehicle, int idFreeSlot)
        {
            if (player.GetData<int>("WIRING_TARGET") == null && player.GetData<int>("WIRING_TARGET") != -1)
            {
                player.SendError("Вы не заломали игрока");
                return;
            }

            ENetPlayer target = ENet.Pools.GetPlayerByUUID(player.GetData<int>("WIRING_TARGET"));

            if (target == null)
            {
                player.SetData("WIRING_TARGET", -1);
                return;
            }

            target.SetIntoVehicle(vehicle.Handle, idFreeSlot);
            player.SendDone("Вы усадили игрока в машину");

            foreach (ENetPlayer playersPool in ENet.Pools.GetPlayersInRadius(player.Position, 10.0f, player.Dimension))
            {
                ENet.Chat.SendMessage(playersPool, $"{player.Nametag} усадил игрок {target.Nametag} в автомобиль");
            }
        }
    }
}