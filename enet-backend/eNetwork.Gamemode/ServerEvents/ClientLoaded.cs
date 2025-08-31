using eNetwork.Framework;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.ServerEvents
{
    public class ClientLoaded : Script
    {
        private static readonly Logger Logger = new Logger("onClient-loaded");

        [RemoteEvent("onClientStart")]
        public static void OnEvent(ENetPlayer player)
        {
            try
            {
                
            }
            catch(Exception ex) { Logger.WriteError("OnEvent", ex); }
        }
    }
}
