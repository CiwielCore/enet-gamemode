using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Game.Vehicles;
using eNetwork.Houses.Interior;
using eNetwork.Property.Parking;
using GTANetworkAPI;
using GTANetworkInternals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Houses.Garage
{
    public class GarageData
    {
        private static readonly Logger Logger = new Logger("garage-data");

        public Position Position { get; set; }
        public List<GaragePositionPlace> Places { get; set; } = new List<GaragePositionPlace>();

        private House House { get; set; }

        private ENetColShape _colShape { get; set; }
        private Marker _marker { get; set; }

        public void GTAElements(House house)
        {
            House = house;

            _colShape = ENet.ColShape.CreateCylinderColShape(Position.GetVector3(), 1f, 2, House.GetDimension(), ColShapeType.GarageInterior);
            _colShape.OnEntityEnterColShape += (s, e) => e.SetData("interior.garage", this);
            _colShape.OnEntityExitColShape += (s, e) => e.ResetData("interior.garage");
            _colShape.SetInteractionText("Выход из гаража");

            _marker = NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, Position.GetVector3() - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), .7f, Helper.GTAColor, false, House.GetDimension());

            Places.ForEach(place => place.Init(this));
            SpawnCars();
        }

        public void SpawnCars()
        {
            try
            {
                if (House.Owner == -1) return;

                var vehicles = VehicleManager.GetPlayerVehicles(House.Owner).ToList();

                foreach (var vehicle in vehicles.ToList())
                {
                    if (ParkingManager.GetParkingByPlace(vehicle.ID) != null)
                    {
                        vehicles.Remove(vehicle);
                        continue;
                    }
                }

                if (vehicles.Count == 0) return;

                NAPI.Task.Run(() =>
                {
                    foreach (var place in Places)
                    {
                        if (!vehicles.Any()) return;

                        var veh = vehicles.First();
                        if (place.SpawnVehicle(veh))
                        {
                            vehicles.Remove(veh);
                        }
                    }
                });
            }
            catch (Exception ex) { Logger.WriteError("SpawnCars", ex); }
        }

        public void SendPlayer(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

                if (!House.Players.Contains(player))
                    House.Players.Add(player);

                Position.Set(player);
                player.SetDimension(House.GetDimension());

                sessionData.EnteredHouse = House.Id;
                characterData.ExteriosPosition = GetExteriorPosition().GetVector3();
            }
            catch (Exception ex) { Logger.WriteError("SendPlayer", ex); }
        }

        public void RemovePlayer(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;

                if (House.Players.Contains(player))
                    House.Players.Remove(player);

                House.GaragePosition.Set(player);
                player.SetDimension(0);

                sessionData.EnteredHouse = -1;
                characterData.ExteriosPosition = null;
            }
            catch (Exception ex) { Logger.WriteError("RemovePlayer", ex); }
        }

        public void Interaction(ENetPlayer player)
        {
            try
            {
                if (!House.CanAccess(player.GetUUID())) return;

                if (player.IsInVehicle && player.VehicleSeat == 0 && player.Vehicle != null)
                {
                    ENetVehicle vehicle = player.Vehicle as ENetVehicle;
                    if (vehicle.VehicleData is null || vehicle.VehicleData.Owner.OwnerUUID != player.GetUUID())
                    {
                        player.SendError("Это не ваша машина!");
                        return;
                    }

                    if (vehicle.VehicleData.Owner.OwnerVehicleType == OwnerVehicleEnum.Faction && (int)player.CharacterData.FactionId != vehicle.VehicleData.Owner.OwnerUUID)
                    {
                        player.SendError("Транспорт принадлежит фракции!");
                        return;
                    }

                    var freePlaces = Places.Where(x => x.Vehicle is null).ToList();
                    if (freePlaces.Count == 0)
                    {
                        player.SendError("В гараже нет мест!");
                        return;
                    }

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (freePlaces.First().SpawnVehicle(vehicle.VehicleData))
                            {
                                vehicle.Delete();
                                SendPlayer(player);

                                var parking = ParkingManager.GetPlace(vehicle.VehicleData.ID);
                                if (parking != null)
                                {
                                    parking.VehicleId = -1;
                                    parking.Save();
                                }
                            }
                            else
                            {
                                player.SendError("Не удалось отправить машину в гараж!");
                            }
                        }
                        catch (Exception ex) { Logger.WriteError("Interaction.Task", ex); }
                    });
                }
                else
                {
                    SendPlayer(player);
                }
            }
            catch (Exception ex) { Logger.WriteError("Interaction", ex); }
        }

        public uint GetDimension()
        { return House.GetDimension(); }

        public Position GetExteriorPosition()
        { return House.GaragePosition; }

        [InteractionDeprecated(ColShapeType.GarageInterior)]
        public static void OnInteraction(ENetPlayer player)
        {
            try
            {
                if (!player.GetSessionData(out var sesionData) || !player.GetData<GarageData>("interior.garage", out var shapeGarage) || sesionData.EnteredHouse == -1) return;

                var garage = HousesManager.GetHouse(sesionData.EnteredHouse).Garage;
                if (garage != shapeGarage) return;

                garage.RemovePlayer(player);
            }
            catch (Exception ex) { Logger.WriteError("OnInteraction", ex); }
        }
    }
}