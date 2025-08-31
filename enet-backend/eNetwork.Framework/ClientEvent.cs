using GTANetworkAPI;
using System;

namespace eNetwork.Framework
{
    public class ClientEvent : Script
    {
        private const string _eventName = "ee8f3f8479b79";

        public static void Event(Player player, string eventName, params object[] args)
        {
            NAPI.Task.Run(() =>
            {
                if (player != null) NAPI.ClientEvent.TriggerClientEvent(player, _eventName, _getParameters(eventName, args));
            });
        }

        public static void EventInRange(Vector3 pos, float range, string eventName, params object[] args)
        {
            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventInRange(pos, range, _eventName, _getParameters(eventName, args));
            });
        }

        public static void EventForAll(string eventName, params object[] args)
        {
            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventForAll(_eventName, _getParameters(eventName, args));
            });
        }

        public static void EventToPlayers(Player[] players, string eventName, params object[] args)
        {
            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, _eventName, _getParameters(eventName, args));
            });
        }

        public static void EventInDimension(uint dim, string eventName, params object[] args)
        {
            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventInDimension(dim, _eventName, _getParameters(eventName, args));
            });
        }

        private static object[] _getParameters(string eventName, params object[] args)
        {
            try
            {
                object[] result = new object[args.Length + 2];

                result[0] = eventName; 
                result[1] = ENet.CustomEvent.GetSecretCode();

                for (int i = 0; i < args.Length; i++)
                    result[i + 2] = args[i];

                return result;
            }
            catch (Exception e) { Console.WriteLine("Get Parameters ClientEvent: " + e.ToString()); return null; }
        }
    }
}
