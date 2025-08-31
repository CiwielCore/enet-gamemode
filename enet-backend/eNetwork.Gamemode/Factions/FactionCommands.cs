using eNetwork.Factions.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Factions
{
    internal class FactionCommands
    {
        [ChatCommand("f", "Сказать что-то в РП чат фракции", Arguments = "[Текст]", GreedyArg = true)]
        private void FactionChat_Command(ENetPlayer player, string text)
        {
            if (player.CharacterData.FactionId < 0)
            {
                player.SendError("Вы не состоите во фракции.");
                return;
            }

            List<ENetPlayer> MembersInFaction = FactionsManager.GetCurrentMembersListInFaction(player.CharacterData.FactionId);

            foreach (ENetPlayer member in MembersInFaction)
            {
                ENet.Chat.SendMessage(member, $"[F] {player.CharacterData} {text}");
            }
        }
    }
}