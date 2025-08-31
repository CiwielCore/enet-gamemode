using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Jobs.BusDriver.Classes;
using eNetwork.Jobs.BusDriver.Data;
using System;

namespace eNetwork.Jobs.BusDriver
{
    public class BusManager
    {
        private static readonly Logger Logger = new Logger("bus-manager");

        public static void Initialize()
        {
            try
            {
                Config.PARKINGS.ForEach(park => park.GTAElements());
            }
            catch(Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public static BusParking GetBusParking(string id)
        {
            return Config.PARKINGS.Find(x => x.Id == id);
        }

        public static void OnDeath(ENetPlayer player, ENetPlayer killer, uint reason)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

                if (!sessionData.WorkData.IsWorking || sessionData.WorkData.BusWorkData.BusStationId == string.Empty) return;

                var parking = GetBusParking(sessionData.WorkData.BusWorkData.BusStationId);
                if (parking is null) return;

                parking.Stop(player);
            }
            catch(Exception ex) { Logger.WriteError("OnDeath", ex); }
        }

        [InteractionDeprecated(ColShapeType.Bus)]
        public static void OnInteractionParking(ENetPlayer player)
        {
            try
            {
                if (!player.GetData<BusParking>("busParking.data", out var parking)) return;
                parking.InteractionNpc(player);
            }
            catch(Exception ex) { Logger.WriteError("OnInteractionParking", ex); }
        }

        [InteractionDeprecated(ColShapeType.BusCheckpoint)]
        public static void OnInteractionCheckpoint(ENetPlayer player)
        {
            try
            {
                if (!player.GetData<BusCheckpoint>("busCheckpoint.data", out var busCheckpoint)) return;
                busCheckpoint.Interaction(player);
            }
            catch(Exception ex) { Logger.WriteError("OnInteractionCheckpoint", ex); }
        }
    }
}
