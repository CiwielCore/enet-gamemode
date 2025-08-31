//using eNetwork.Factions.Tasks.CarTheft;
using eNetwork.Framework;
using eNetwork.Game.Vehicles;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.GameUI.InteractionMenu
{
    public class InteractionMenuController
    {
        private static readonly Logger Logger = new Logger("interaction-menu");

        [CustomEvent("server.interactionMenu.vehicle.callback")]
        public static void VehicleInteraction(ENetPlayer player, ENetVehicle vehicle, string callback)
        {
            try
            {
                if (!player.IsTimeouted("vehicle.interaction", 1) || vehicle is null) return;

                if (vehicle.Position.DistanceTo(player.Position) > 5)
                {
                    player.SendError("Транспорт слишком далеко от вас");
                    return;
                }

                switch (callback)
                {
                    case "lock":
                        VehicleSync.LockVehicle(player, vehicle);
                        break;

                    case "engine":
                        VehicleSync.Engine(player);
                        break;

                    case "boot":
                        if (vehicle.GetDoorState(DoorID.DoorHood) == DoorState.DoorClosed)
                        {
                            if (vehicle.Locked)
                            {
                                player.SendWarning("Двери транспорта закрыты");
                                return;
                            }
                            vehicle.SetDoorState(DoorID.DoorHood, DoorState.DoorOpen);
                        }
                        else
                            vehicle.SetDoorState(DoorID.DoorHood, DoorState.DoorClosed);
                        break;

                    case "trunk":
                        if (vehicle.GetDoorState(DoorID.DoorTrunk) == DoorState.DoorClosed)
                        {
                            if (vehicle.Locked)
                            {
                                player.SendWarning("Двери транспорта закрыты");
                                return;
                            }
                            vehicle.SetDoorState(DoorID.DoorTrunk, DoorState.DoorOpen);
                        }
                        else
                        {
                            vehicle.SetDoorState(DoorID.DoorTrunk, DoorState.DoorClosed);
                        }
                        break;

                    case "hijacking":
                        ClientEvent.Event(player, "client.interactionMenu.close");
                        //CarTheftManager.UseLockpick(player, vehicle);
                        return;
                }

                ClientEvent.Event(player, "client.interactionMenu.close");
            }
            catch (Exception ex) { Logger.WriteError("VehicleInteraction", ex); }
        }
    }
}