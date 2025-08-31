using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace eNetwork.GameUI.Pad
{
    public class PadManager
    {
        private static readonly Logger Logger = new Logger(nameof(PadManager));

        [CustomEvent("server.ipad.open")]
        public static void Open(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;

                ClientEvent.Event(player, "client.ipad.open");
                if (!player.IsInVehicle)
                {
                    player.PlayScenario(ScenarioType.TakeIpad);
                    player.AddAttachment("ipad");
                }
            }
            catch(Exception ex) { Logger.WriteError("Open", ex); }
        }

        [CustomEvent("server.ipad.close")]
        public static void Close(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;

                ClientEvent.Event(player, "client.ipad.close");

                if (!player.IsInVehicle)
                {
                    player.StopScenario();
                    player.AddAttachment("ipad", true);
                }
            }
            catch(Exception ex) { Logger.WriteError("Close", ex); }
        }

        [CustomEvent("server.ipad.openApp")]
        public static void TryOpenApp(ENetPlayer player, string appName)
        {
            try
            {
                switch(appName)
                {
                    case "business":
                        Apps.Business.BusinessApp.On(player);
                        return;
                }
            }
            catch(Exception ex) { Logger.WriteError("OpenApp", ex); }
        }

        public static void OpenApp(ENetPlayer player, string appName, string jsonData)
            => ClientEvent.Event(player, "client.ipad.openApp", appName, jsonData);
    }
}
