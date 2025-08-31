using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using eNetwork.Framework;
using eNetwork.Modules;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using eNetwork.Property.Parking;
using eNetwork.Services.CarRental;
using eNetwork.Houses.Garage;

namespace eNetwork.Game.Vehicles
{
    public class VehicleSync
    {
        private static readonly Logger Logger = new Logger("vehicle-sync");
        private const string DataSync = "vehicle.sync";
        private static Dictionary<string, VehicleConfig> VehicleConfigs = new Dictionary<string, VehicleConfig>();

        public static void Initialize()
        {
            try
            {
                VehicleConfigs = ConfigReader.ReadAsync("vehicleConfig", new Dictionary<string, VehicleConfig>());

                Timers.StartTask(1000, () => VehicleControl());
            }
            catch (Exception e) { Logger.WriteError("Init", e); }
        }

        public static void VehicleControl()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    ENetVehicle currentVehicle;
                    var vehicles = ENet.Pools.GetAllVehicles();
                    foreach (ENetVehicle veh in vehicles)
                    {
                        currentVehicle = veh;
                        if (!veh.HasSharedData("mile") || !veh.HasSharedData("petrol") || !veh.EngineStatus) continue;

                        DateTime lastTime = veh.GetData<DateTime>("last.update");

                        // Milliage
                        Vector3 velocity = NAPI.Entity.GetEntityVelocity(veh.Handle);
                        double speeds = Math.Sqrt((velocity.X * velocity.X) + (velocity.Y * velocity.Y) + (velocity.Z * velocity.Z)) * 3.6;

                        float trip = (float)((float)speeds * ((DateTime.Now - lastTime).TotalSeconds / 1000) * 100 / 100);

                        float distance = trip / 3.6f;
                        float currentMiles = veh.GetMile();
                        veh.SetSharedData("mile", (float)(currentMiles + distance));

                        // Petrol control
                        float currentPetrol = veh.GetPetrol();

                        float rate = veh.GetSharedData<float>("petrol.rate") / 100;
                        float consumed = rate * distance;

                        float newFuel = currentPetrol - consumed;
                        if (newFuel < 0)
                        {
                            veh.EngineState(false);
                        }
                        else
                        {
                            if (distance == 0)
                            {
                                newFuel = currentPetrol - 0.0005f;
                            }
                            veh.SetSharedData("petrol", newFuel);
                        }

                        veh.SetData("last.update", DateTime.Now);
                        //Console.WriteLine("Mile: " + veh.GetSharedData<float>("mile") + "; Fuel: " + veh.GetSharedData<float>("petrol"));
                    }
                }
                catch (Exception e) { Logger.WriteError("VehicleControl", e); }
            });
        }

        public static bool GetVehicleConfig(string model, out VehicleConfig vehicleConfig)
        {
            return VehicleConfigs.TryGetValue(model, out vehicleConfig);
        }

        public static ENetVehicle GetNearestVehicle(ENetPlayer player, int radius)
        {
            try
            {
                List<ENetVehicle> all_vehicles = ENet.Pools.GetAllVehicles();
                ENetVehicle nearest_vehicle = null;
                foreach (ENetVehicle v in all_vehicles)
                {
                    if (v.Dimension != player.Dimension) continue;
                    if (nearest_vehicle == null && player.Position.DistanceTo(v.Position) < radius)
                    {
                        nearest_vehicle = v;
                        continue;
                    }
                    else if (nearest_vehicle != null)
                    {
                        if (player.Position.DistanceTo(v.Position) < player.Position.DistanceTo(nearest_vehicle.Position))
                        {
                            nearest_vehicle = v;
                            continue;
                        }
                    }
                }
                return nearest_vehicle;
            }
            catch (Exception e) { Logger.WriteError("GetNearestVehicle", e); return null; }
        }

        public static VehicleConfig GetVehicleConfig(Vehicle vehicle)
        {
            return VehicleConfigs.FirstOrDefault(x => NAPI.Util.GetHashKey(x.Key) == vehicle.Model).Value;
        }

        #region Events

        [CustomEvent("server.vehicleSync.engine")]
        public static void Engine(ENetPlayer player)
        {
            try
            {
                if (!NAPI.Player.IsPlayerInAnyVehicle(player) || player.VehicleSeat != 0 || player.Vehicle.Class == 13 || !player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData)) return;
                ENetVehicle vehicle = (ENetVehicle)player.Vehicle;

                VehicleData vehicleData = vehicle.GetVehicleData();
                switch (vehicle.VehicleType)
                {
                    case VehicleType.Server:
                        if (characterData.Status < PlayerRank.Admin) return;

                        break;

                    case VehicleType.Work:
                        if (vehicle.Driver != player.GetUUID() || sessionData.WorkData.Vehicle != vehicle)
                        {
                            player.SendError("Это не ваша машина!");
                            return;
                        }
                        break;

                    case VehicleType.Personal:
                        if (vehicleData == null) return;
                        if (vehicleData.Owner.OwnerUUID != characterData.UUID)
                        {
                            player.SendError("У вас нет ключей от этой машины");
                            return;
                        }
                        break;

                    case VehicleType.Fraction:
                        if (vehicleData == null) return;
                        if (vehicleData.Owner.OwnerUUID != characterData.FactionId)
                        {
                            player.SendError("У вас нет ключей от этой машины");
                            return;
                        }
                        break;

                    case VehicleType.Rental:
                        if (!CarRentalService.Instance.IsPlayerOwnerVehicle(player, vehicle))
                        {
                            player.SendError("У вас нет ключей от этой машины");
                            return;
                        }
                        break;
                }

                if (vehicle.HasData("in.parking"))
                {
                    ParkingPlace parkingPlace = vehicle.GetData<ParkingPlace>("in.parking");
                    parkingPlace.TakeVehicle(player);
                    return;
                }

                if (vehicle.HasData("in.garage"))
                {
                    GaragePositionPlace garagePlace = vehicle.GetData<GaragePositionPlace>("in.garage");
                    garagePlace.TakeVehicle(player);
                    return;
                }

                bool engine = vehicle.GetEngine();
                vehicle.EngineState(!engine);
                //ChatHandler.SendMessage(player, !engine ? "Вы завели двигатель авто" : "Вы заглушили двигатель авто");
            }
            catch (Exception e) { Logger.WriteError("Engine", e); }
        }

        [CustomEvent("server.vehicleSync.lights")]
        public void Ligts(ENetPlayer player, ENetVehicle vehicle, bool left, bool right)
        {
            try
            {
                var data = vehicle.GetSyncData();

                if (data.Engine)
                {
                    vehicle.LightState(Lights.Left, left);
                    vehicle.LightState(Lights.Right, right);
                }
            }
            catch (Exception e) { Logger.WriteError("Ligts", e); }
        }

        [CustomEvent("server.vehicleSync.lock")]
        public void Locked(ENetPlayer player)
        {
            try
            {
                ENetVehicle vehicle = null;
                if (NAPI.Player.IsPlayerInAnyVehicle(player))
                    vehicle = (ENetVehicle)player.Vehicle;
                else
                {
                    vehicle = GetNearestVehicle(player, 10);
                    if (vehicle is null) return;
                }

                LockVehicle(player, vehicle);
            }
            catch (Exception e) { Logger.WriteError("Locked", e); }
        }

        public static void LockVehicle(ENetPlayer player, ENetVehicle vehicle)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetSessionData(out var sessionData) || !vehicle.GetVehicleData(out VehicleData vehicleData) && vehicle.VehicleType != VehicleType.Server) return;

                if (vehicle.Class == 13) return;

                switch (vehicle.VehicleType)
                {
                    case VehicleType.Server:
                        if (characterData.Status < PlayerRank.Admin) return;

                        break;

                    case VehicleType.Work:
                        if (vehicle.Driver != player.GetUUID() || sessionData.WorkData.Vehicle != vehicle)
                        {
                            player.SendError("Это не ваша машина!");
                            return;
                        }
                        break;

                    case VehicleType.Personal:
                        if (vehicleData == null) return;
                        if (vehicleData.Owner.OwnerUUID != characterData.UUID)
                        {
                            player.SendError("У вас нет ключей от этой машины");
                            return;
                        }
                        break;

                    case VehicleType.Fraction:
                        if (vehicleData == null) return;
                        if (vehicleData.Owner.OwnerUUID != characterData.FactionId)
                        {
                            player.SendError("У вас нет ключей от этой машины");
                            return;
                        }
                        break;

                    case VehicleType.Rental:
                        if (!CarRentalService.Instance.IsPlayerOwnerVehicle(player, vehicle))
                        {
                            player.SendError("У вас нет ключей от этой машины");
                            return;
                        }
                        break;
                }

                bool locked = vehicle.GetLocked();
                vehicle.LockStatus(!locked);
                //ChatHandler.SendMessage(player, locked ? "Вы открыли двери авто" : "Вы закрыли двери авто");

                player.PlayScenario(ScenarioType.VehicleKey);
                player.AddAttachment("carkey");
            }
            catch (Exception e) { Logger.WriteError("LockVehicle", e); }
        }

        [CustomEvent("server.vehicleSync.fix")]
        public void FixStream(ENetPlayer player, ENetVehicle vehicle)
        {
            try
            {
                if (vehicle is null || !NAPI.Entity.DoesEntityExist(vehicle)) return;
                // Устанавливаем дату
                vehicle.GetSyncData();
            }
            catch (Exception e) { Logger.WriteError("FixStream", e); }
        }

        [CustomEvent("server.vehicleSync.nitro")]
        public void NitroSync(ENetPlayer player, bool isBoosting, bool isPurging, bool isLastVehicle)
        {
            try
            {
                ClientEvent.EventInRange(player.Position, 200, "client.vehicleSync.nitro", player.Value, isBoosting, isPurging, isLastVehicle);
            }
            catch (Exception ex) { Logger.WriteError("NitroSync", ex); }
        }

        #endregion Events
    }
}