using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using eNetwork.Framework;
using eNetwork.Modules;
using GTANetworkAPI;
using Newtonsoft.Json;
using eNetwork.Game;
using eNetwork.Framework.API.ChatCommand;
using eNetwork.External;
using eNetwork.Mute;

namespace eNetwork
{
    public class ChatHandler
    {
        private static Logger Logger = new Logger("ChatHandler");
        /// <summary>
        /// Отправка сообщения игроку
        /// </summary>
        /// <param name="player">Игрок</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="type">Дополнительный модификатор</param>
        public static void SendMessage(ENetPlayer player, string message, ChatAddition type = null)
        {
            try
            {
                ENet.Chat.SendMessage(player, message, type);
            }
            catch (Exception ex) { Logger.WriteError("SendMessage", ex); }
        }

        public static void SendRPMessage(ENetPlayer player, string message, ChatAddition type = null)
        {
            try
            {
                foreach (var target in ENet.Pools.GetPlayersInRadius(player.Position, 10, player.Dimension))
                    SendMessage(target, message, type);
            }
            catch(Exception ex) { Logger.WriteError("SendRPMessage", ex); }
        }

        /// <summary>
        /// Отправить сообщение всем зарегестрированным игрокам
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="type">Дополнительный модификатор</param>
        public static void SendMessageToAll(string message, ChatAddition type = null)
        {
            try
            {
                foreach (ENetPlayer player in ENet.Pools.GetAllRegisteredPlayers())
                {
                    ENet.Chat.SendMessage(player, message, type);
                }
            }
            catch (Exception ex) { Logger.WriteError("SendMessageToAll", ex); }
        }
        /// <summary>                             
        /// Загрузка всех команд в игровой чат клиенту
        /// </summary>
        /// <param name="player">Игрок</param>
        public static void LoadChat(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var data)) return;
                ClientEvent.Event(player, "client.chat.load", 
                    JsonConvert.SerializeObject(ENet.ChatCommands.GetCommandsForUser(data.Status)));
            }
            catch (Exception ex) { Logger.WriteError($"LoadChat: {player.Name}", ex); }
        }

        #region Custom Events
        [CustomEvent("server.chat.sendMessage")]
        private static void CustomEvent_SendMessage(ENetPlayer player, string text)
        {
            try
            {
                if (!player.IsTimeouted("MESSAGE", 2) || !player.IsExsist()) return;

                text = Helper.ParseHTML(text);
                if (String.IsNullOrEmpty(text)) return;
                //var reaction = ReactionTest.CheckReaction(player, text);
                //if (reaction) return;

                bool isMuted = MuteRepository.Instance.IsCharacterInMute(player.CharacterData.UUID);

                if (isMuted)
                {
                    ENet.Chat.SendMessage(player, "У вас заблокирован игровой чат за нарушение правил сервера!");
                    return;
                }


                foreach (var target in ENet.Pools.GetPlayersInRadius(player.Position, 10, player.Dimension))
                    SendMessage(target, $"{Helper.FormatName(player.Name)} сказал: {text}");
            }
            catch (Exception ex) { Logger.WriteError("CustomEvent_SendMessage", ex); }
        }
        [CustomEvent("server.chat.command")]
        private static void CustomEvent_UseCommand(ENetPlayer player, string message)
        {
            try
            {
                if (!player.IsTimeouted("COMMAND", 2) || !player.IsExsist() || !player.GetCharacter(out var characterData)) return;
                string[] split = message.Split(" ");

                string text = "";
                for (int i = 0; i < split.Length; i++) if (i != 0) text += split[i] + (split.Length - 1 == i ? "" : " ");

                var use = ENet.ChatCommands.UseCommand(player, characterData.Status, split[0], text);
                switch (use)
                {
                    case ChatError.UndefinedCommand:
                        SendMessage(player, "Команда не найдена!");
                        return;
                    default: return;
                }
            }
            catch (Exception ex) { Logger.WriteError("CustomEvent_UseCommand", ex); }
        }
        #endregion
    }
}
