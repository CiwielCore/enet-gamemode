using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Mute.Commands
{
    public class MuteCommands
    {
        [ChatCommand("mute", Description = "Выдать блокировку игровых чатов", Access = PlayerRank.Helper, Arguments = "[Статический ID] [Время в минутах] [Причина]")]
        public void Command_Mute(ENetPlayer player, int uuid, uint minutes, string reason)
        {
            ENetPlayer target = ENet.Pools.GetPlayerByUUID(uuid);

            if (target == null)
            {
                GiveMuteThePlayer(player, uuid, minutes, reason);
                return;
            }

            GiveMuteThePlayer(player, target, minutes, reason);
            ENet.Chat.SendMessageForAll(player, $"Администратор {player.Name}[{player.Id}] выдал мут {target.Name} {(target is null ? $"[{target.Id}]" : "")} мин. по причине: {reason}");
        }

        [ChatCommand("unmute", Description = "Снять блокировку игровых чатов", Access = PlayerRank.Helper, Arguments = "[Статический ID]")]
        public void Command_UnMute(ENetPlayer player, int uuid)
        {
            ENetPlayer target = ENet.Pools.GetPlayerByUUID(uuid);

            if (target is null)
            {
                RemoveMuteThePlayer(uuid);
                return;
            }

            RemoveMuteThePlayer(target);
        }

        private static void GiveMuteThePlayer(ENetPlayer admin, ENetPlayer target, uint minutes, string reason)
        {
            ENet.Chat.SendMessage(target, $"Администратор {admin.Name} выдал Вам мут на \"{minutes}\" мин. по причине: {reason}");
            MuteRepository.Instance.AddMuteInfo(target.CharacterData.UUID, admin.CharacterData.UUID, minutes, reason);
        }

        private static void GiveMuteThePlayer(ENetPlayer admin, int targetUUID, uint minutes, string reason)
        {
            MuteRepository.Instance.AddMuteInfo(targetUUID, admin.CharacterData.UUID, minutes, reason);
        }

        public void RemoveMuteThePlayer(ENetPlayer target)
        {
            ENet.Chat.SendMessage(target, "Ваш чат был разблокирован, больше не нарушайте правила!");
            MuteRepository.Instance.RemoveMuteInfo(target.CharacterData.UUID);
        }
            
        private void RemoveMuteThePlayer(int targetUUID)
        {
            MuteRepository.Instance.RemoveMuteInfo(targetUUID);
        }
    }
}
