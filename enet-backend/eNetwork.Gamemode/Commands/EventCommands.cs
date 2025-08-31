using eNetwork.Framework;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Commands
{
    public class EventCommands
    {
        private static bool EventStarted = false;
        private static int MaxPlayer;
        private static int CurrentValuePlayer;
        private static Vector3 EventPositionPoint;

        [ChatCommand("eventon", Description = "Начать мероприятие", Access = PlayerRank.Helper, Arguments = "[Максимальное кол-во участников] [Название] [Описание]", GreedyArg = true)]
        public static void Command_EventOn(ENetPlayer player, int maxPlayer, string eventName, string description)
        {
            try
            {
                if (EventStarted) return;  

                foreach (ENetPlayer playerSort in ENet.Pools.GetAllPlayers())
                {
                   ENet.Chat.SendMessage(playerSort, ($"Началось мероприятие: {eventName}, {description}. Чтобы принять участие напишите /mp"));
                }

                EventStarted = true;
                MaxPlayer = maxPlayer;
                EventPositionPoint = player.Position;

            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }


        [ChatCommand("mp", Description = "Телепортироваться на эвент", Access = PlayerRank.Helper)]
        public static void Command_Mp(ENetPlayer player)
        {
            try
            {
                if (!EventStarted)
                {
                    ENet.Chat.SendMessage(player, "Мероприятие еще не началось.");
                    return;
                }

                if (CurrentValuePlayer == MaxPlayer)
                {
                    ENet.Chat.SendMessage(player, "Достигнуто максимальное количетсво участников.");
                    return;
                }

                player.Position = EventPositionPoint;
                player.SetData<bool>("participantInEvent", true);
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("eventhp", Description = "Выдать ХП игрокам на мероприятии", Access = PlayerRank.Helper, Arguments = "[Value]")]
        public static void Command_EventHp(ENetPlayer player, int value)
        {
            try
            {
                foreach (ENetPlayer playerSort in ENet.Pools.GetAllPlayers())
                {
                    if (playerSort.GetData<bool>("participantInEvent") == true)
                    {
                        playerSort.Health = value;
                    }
                }

            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("eventoff", Description = "Закончить мероприятие", Access = PlayerRank.Helper)]
        public static void Command_EventOff(ENetPlayer player)
        {
            try
            {
                if (!EventStarted) return;

                EventPositionPoint = null;
                EventStarted = false;

            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }
    }
}
