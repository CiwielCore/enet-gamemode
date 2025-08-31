using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using eNetwork.Framework;
using GTANetworkAPI;

namespace eNetwork.GameUI
{
    class Transition
    {
        private static Logger _loggger = new Logger("Transition");
        /// <summary>
        /// Запускает плавное затухание экрана
        /// </summary>
        /// <param name="player">Игрока</param>
        /// <param name="fadein">Скорость затухания</param>
        /// <param name="delay">Задержка между переходами</param>
        /// <param name="fadeout">Скорость появления</param>
        public static void Start(ENetPlayer player, int fadein, int delay, int fadeout, Action<Player> action = null)
        {
            try
            {
                ClientEvent.Event(player, "client.transition", fadein, delay, fadeout);
                Task.Delay(fadein + delay);

                if (!(action is  null))
                    action(player);
            }   
            catch(Exception e) { _loggger.WriteError("Start" + e.ToString()); }
        }

        public static void Open(ENetPlayer player, string text)
        {
            try
            {
                ClientEvent.Event(player, "client.customTransition", text);   
            }
            catch(Exception ex) { _loggger.WriteError("Open", ex); }
        }
        
        public static void Close(ENetPlayer player)
        {
            try
            {
                ClientEvent.Event(player, "client.customTransition.close");
            }
            catch (Exception ex) { _loggger.WriteError("Open", ex); }
        }
    }
}
