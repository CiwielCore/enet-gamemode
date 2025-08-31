using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using eNetwork.Framework;
using eNetwork.Framework.Enums;

namespace eNetwork.ServerEvents
{
    class PlayerExitVehicle : Script
    {
        private static readonly Logger Logger = new Logger("player-death");

        [ServerEvent(Event.PlayerExitVehicle)]
        private static void OnEvent(Player player, Vehicle vehicle)
        {
            try
            {
                if (player is ENetPlayer enetPlayer && vehicle is ENetVehicle enetVehicle)
                {
                    if (enetPlayer == null || !enetPlayer.GetCharacter(out var characterData) || !enetPlayer.GetSessionData(out var sessionData)) return;

                    if (sessionData.WorkData.VehicleTimer != null)
                    {
                        sessionData.WorkData.VehicleTimer = Timers.StartOnce(600 * 1000, () => deleteVehicle(enetPlayer));
                        enetPlayer.SendInfo($"У вас есть 10 минут чтобы вернуться в свое ТС");
                        return;
                    }
                    Businesses.Showroom.EndTestdrive(enetPlayer);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError("On", ex);
            }
        }

        private static void deleteVehicle(ENetPlayer player)
        {
            if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

            if (sessionData.WorkData.VehicleTimer != null)
            {
                Timers.Stop(sessionData.WorkData.VehicleTimer);
                sessionData.WorkData.VehicleTimer = null;
            }

            if (sessionData.WorkData.Vehicle != null)
            {
                sessionData.WorkData.Vehicle.Delete();
                sessionData.WorkData.Vehicle = null;
            }
            characterData.JobId = JobId.None;
            player.SendInfo($"Работа закончена!");
            //TODO: Remove blips
        }
    }
}
