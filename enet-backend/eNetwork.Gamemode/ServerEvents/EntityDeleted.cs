using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using eNetwork.Framework;

namespace eNetwork.ServerEvents
{
    public class EntityDeleted : Script
    {
        private static readonly Logger Logger = new Logger("entity-created");

        [ServerEvent(Event.EntityDeleted)]
        public static void OnEvent(Entity entity)
        {
            try
            {
                
            }
            catch (Exception e) { Logger.WriteError("OnEvent", e); }
        }
    }
}
