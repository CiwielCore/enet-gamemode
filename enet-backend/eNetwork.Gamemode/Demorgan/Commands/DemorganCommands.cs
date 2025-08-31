using GTANetworkAPI;
using eNetwork.Framework;
using System;
using eNetwork.Demorgan.Models;
using eNetwork.Game.Characters;

namespace eNetwork.Demorgan
{
    class DemorganCommands : Script
    {
        [ChatCommand("jail", Description = "Посадить игрока в деморган", Access = PlayerRank.JuniorAdmin, Arguments = "[статик] [минуты] [причина]")]
        public static void PutPlayerInDemorganCommandHandler(ENetPlayer player, int characterId, int minutes, string reason)
        {

            ENetPlayer target = ENet.Pools.GetPlayerByUUID(characterId);

            if (DemorganRepository.Instance.IsCharacterInDemorgan(characterId))
            {
                player.SendError("Игрок уже находится в деморгане");
                return;
            }

            if (CharacterManager.GetCharacterData(characterId) is null)
            {
                player.SendError("Персонажа с таким UUID не существует");
                return;
            }

            DemorganInfo info = DemorganRepository.Instance.AddDemorganInfo(characterId, player.CharacterData.UUID, Convert.ToUInt32(minutes), reason);

            if (target != null)
            {
                DemorganManager.Instance.PutPlayerInDemorgan(target, info);
                ENet.Chat.SendMessageForAll(player, $"Администратор {player.Name}[{player.Id}] посадил {target.Name}[{target.Id}] по причине: {reason}");
                ENet.Chat.SendMessage(target, $"Администратор {player.Name}[{player.Id}] посадил Вас в деморган на \"{minutes}\" по причине: {reason}");
            } else
            {
                ENet.Chat.SendMessageForAll(player, $"Администратор {player.Name}[{player.Id}] посадил {target.Name} по причине: {reason}");
            }

        }

        [ChatCommand("sjail", Description = "Тихо посадить игрока в деморган", Access = PlayerRank.JuniorAdmin, Arguments = "[статик] [минуты] [причина]")]
        public static void SilentPutPlayerInDemorganCommandHandler(ENetPlayer player, int characterId, int minutes, string reason)
        {
            if (DemorganRepository.Instance.IsCharacterInDemorgan(characterId))
            {
                player.SendError("Игрок уже находится в деморгане");
                return;
            }

            if (CharacterManager.GetCharacterData(characterId) is null)
            {
                player.SendError("Персонажа с таким UUID не существует");
                return;
            }

            DemorganInfo info = DemorganRepository.Instance.AddDemorganInfo(characterId, player.CharacterData.UUID, Convert.ToUInt32(minutes), reason);

            if (ENet.Pools.GetPlayerByUUID(characterId) is ENetPlayer target)
                DemorganManager.Instance.PutPlayerInDemorgan(target, info);
        }

        [ChatCommand("unjail", Description = "Освободить персонажа игрока из деморгана", Access = PlayerRank.JuniorAdmin, Arguments = "[статик]")]
        public static void ReleaseCharacterFromDemorganCommandHandler(ENetPlayer player, int characterId)
        {
            if (!DemorganRepository.Instance.IsCharacterInDemorgan(characterId))
            {
                player.SendError("Игрок не в деморгане");
                return;
            }

            if (ENet.Pools.GetPlayerByUUID(characterId) is ENetPlayer target)
            {
                DemorganManager.Instance.ReleasePlayerInDemorgan(target);
                return;
            }

            DemorganRepository.Instance.RemoveDemorganInfo(characterId);
        }
    }
}
