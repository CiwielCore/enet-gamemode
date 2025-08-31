using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using GTANetworkAPI;
using eNetwork.Framework;
using Colorful;
using Console = Colorful.Console;

namespace eNetwork.ServerEvents
{
    public class ResourceStart : Script
    {
        public static Logger Logger = new Logger("serverEvents-resourceStart");

        public static void Initialize()
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    NAPI.World.SetTime(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    NAPI.Server.SetDefaultSpawnLocation(new Vector3(-654.8698, 4383.7466, 34.416706));
                    //NAPI.Server.SetDefaultSpawnLocation(new Vector3(-1490.6287, 144.23244, 34.416706));
                    NAPI.Server.SetGlobalServerChat(false);
                    NAPI.Server.SetAutoRespawnAfterDeath(false);
                });

                RAGE.Entities.Players.CreateEntity = (NetHandle netHandle) => new ENetPlayer(netHandle);
                RAGE.Entities.Colshapes.CreateEntity = (NetHandle netHandle) => new ENetColShape(netHandle);
                RAGE.Entities.Blips.CreateEntity = (NetHandle netHandle) => new ENetBlip(netHandle);
                RAGE.Entities.Vehicles.CreateEntity = (NetHandle netHandle) => new ENetVehicle(netHandle);
            }
            catch(Exception e) { Logger.WriteError("Initialize", e); }
        }
    }
}
