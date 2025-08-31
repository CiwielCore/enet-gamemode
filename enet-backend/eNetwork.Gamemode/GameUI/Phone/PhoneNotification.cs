using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.GameUI.Phone
{
    public class PhoneNotification
    {
        private static readonly Logger Logger = new Logger("phone-notification");
        private static IReadOnlyDictionary<PhoneNotificationType, string> Headers = new Dictionary<PhoneNotificationType, string>()
        {
            { PhoneNotificationType.Mazebank, "Mazebank" },
            { PhoneNotificationType.Fleeca, "Fleeca" },
            { PhoneNotificationType.Sms, "Сообщения" },
            { PhoneNotificationType.MyRide, "My Ride" },
        };

        public static void Send(ENetPlayer player, PhoneNotificationType type, string message, int time = 3000)
        {
            try
            {
                if (!Headers.TryGetValue(type, out string header))
                    Logger.WriteError($"Не получилось получить хеадер уведомления с типом: PhoneNotificationType.{type.ToString()}");

                ClientEvent.Event(player, "client.phone.notification", type.ToString().ToLower(), header, message, time);
            }
            catch(Exception ex) { Logger.WriteError("Send", ex); }
        }
    }
}
