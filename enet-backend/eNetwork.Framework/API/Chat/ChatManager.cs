using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using eNetwork.Framework;
using Newtonsoft.Json;
using GTANetworkMethods;

namespace eNetwork.Framework.API.ChatCommand
{
    public class ChatManager
    {
        private static readonly Logger _logger = new Logger("Chat");
        private const string _pushEvnetName = "client.chat.push";
        
        /// <summary>
        /// Отправка сообщения в игроковй чат 
        /// </summary>
        public void SendMessage(ENetPlayer player, string message, ChatAddition type = null)
        {
            try
            {
                ClientEvent.Event(player, _pushEvnetName, message, JsonConvert.SerializeObject(type));
            }
            catch (Exception ex) { _logger.WriteError("SendMessage", ex); }
        }

        public void SendMessageForAll(ENetPlayer player, string message, ChatAddition type = null)
        {
            try
            {
                List<ENetPlayer> target = ENet.Pools.GetAllRegisteredPlayers();

                foreach (ENetPlayer targetPool in target)
                {
                    ClientEvent.Event(targetPool, _pushEvnetName, message, JsonConvert.SerializeObject(type));
                }
            }
            catch (Exception ex) { _logger.WriteError("SendMessage", ex); }
        }
    }
}
