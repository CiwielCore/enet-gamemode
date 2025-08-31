using eNetwork.Factions.Classes;
using eNetwork.Framework.API.Interaction;
using eNetwork.Framework.Classes.Faction;
using eNetwork.GameUI;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace eNetwork.Factions.Interaction
{
    public class PedInteraction
    {
        public static void Interaction(ENetPlayer player, ColShape colShape)
        {
            FactionsManager factionsManager = new FactionsManager();
            FactionType factionType = factionsManager.GetFactionTypeByID(player.CharacterData.FactionId);

            if (colShape.GetData<int>("FACTION_ID") != player.CharacterData.FactionId)
            {
                if (factionType == FactionType.State)
                {
                    InteractionStateFromPlayerWithoutFaction(player, colShape);
                }
                else
                {
                    InteractionCriminalFromPlayerWithoutFaction(player, colShape);
                }
            }
            else
            {
                if (factionType == FactionType.State)
                {
                    InteractionStateWithPlayerFromFaction(player, colShape);
                }
                else
                {
                    InteractionCriminalWithPlayerFromFaction(player, colShape);
                }
            }
        }

        private static void InteractionStateWithPlayerFromFaction(ENetPlayer player, ColShape colShape)
        {
            List<DialogAnswer> answers = new List<DialogAnswer>();

            Dialog dialog = new Dialog()
            {
                Name = colShape.GetData<string>("PED_NAME"),
                Description = "Местный сотрудник"
            };

            dialog.Text = "Чем могу помочь вам?";
            answers.Add(new DialogAnswer("Я пойду", (player, o) => Dialogs.Close(player), "stop"));
            dialog.Answers = answers;

            dialog.Open(player, colShape.GetData<Ped>("PED"));
        }

        private static void InteractionCriminalWithPlayerFromFaction(ENetPlayer player, ColShape colShape)
        {
            List<DialogAnswer> answers = new List<DialogAnswer>();

            Dialog dialog = new Dialog()
            {
                Name = colShape.GetData<string>("PED_NAME"),
                Description = "Местный гангстер"
            };

            dialog.Text = "Йо, чё-то хотел спросить?";
            answers.Add(new DialogAnswer("Я пойду", (player, o) => Dialogs.Close(player), "stop"));
            dialog.Answers = answers;

            dialog.Open(player, colShape.GetData<Ped>("PED"));
        }

        private static void InteractionStateFromPlayerWithoutFaction(ENetPlayer player, ColShape colShape)
        {
            List<DialogAnswer> answers = new List<DialogAnswer>();

            Dialog dialog = new Dialog()
            {
                Name = colShape.GetData<string>("PED_NAME"),
                Description = "Местный сотрудник"
            };

            dialog.Text = "Я обычно координирую сотрудников, но может чем-то могу вам помочь?";
            answers.Add(new DialogAnswer("Я пойду", (player, o) => Dialogs.Close(player), "stop"));
            dialog.Answers = answers;

            dialog.Open(player, colShape.GetData<Ped>("PED"));
        }

        private static void InteractionCriminalFromPlayerWithoutFaction(ENetPlayer player, ColShape colShape)
        {
            List<DialogAnswer> answers = new List<DialogAnswer>();

            Dialog dialog = new Dialog()
            {
                Name = colShape.GetData<string>("PED_NAME"),
                Description = "Местный гангстер"
            };

            dialog.Text = "Чё то ранее я тебе не видел, чё тебе надо?!";
            answers.Add(new DialogAnswer("Я пойду", (player, o) => Dialogs.Close(player), "stop"));
            dialog.Answers = answers;

            dialog.Open(player, colShape.GetData<Ped>("PED"));
        }
    }
}