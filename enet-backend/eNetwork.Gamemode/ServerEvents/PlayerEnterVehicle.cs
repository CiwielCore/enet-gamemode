using eNetwork.Framework;
using GTANetworkAPI;
using System;

namespace eNetwork.ServerEvents
{
    public class PlayerEnterVehicle : Script
    {
        private static readonly Logger Logger = new Logger("player-enter-vehicle");

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void On(Player player, Vehicle vehicle, sbyte seatId)
        {
            try
            {
                if (player is ENetPlayer enetPlayer && vehicle is ENetVehicle enetVehicle)
                {
                    if (enetPlayer == null || !enetPlayer.GetCharacter(out var characterData) || !enetPlayer.GetSessionData(out var sessionData)) return;

                    if (sessionData.WorkData.VehicleTimer != null)
                    {
                        Timers.Stop(sessionData.WorkData.VehicleTimer);
                        sessionData.WorkData.VehicleTimer = null;
                    }

                    //Factions.Tasks.CarTheft.CarTheftManager.OnEnterVehicle(enetPlayer, enetVehicle, seatId);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError("On", ex);
            }
        }
    }
}