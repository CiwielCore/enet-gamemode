using eNetwork.Framework;
using eNetwork.GameUI;
using eNetwork.Property.Parking;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Houses.Garage
{
    public class GaragePositionPlace
    {
        private static readonly Logger Logger = new Logger("garage-place");
        public Position Position { get; set; }

        public GaragePositionPlace(Position position)
        {
            Position = position;
        }

        public GarageData Garage { get; set; }
        public void Init(GarageData garageData)
        {
            Garage = garageData;
        }

        [JsonIgnore]
        public VehicleData VehicleData { get; set; } = null;

        [JsonIgnore]
        public ENetVehicle Vehicle { get; set; } = null;

        public bool SpawnVehicle(VehicleData vehicleData)
        {
            try
            {
                if (VehicleData != null || vehicleData is null) return false;

                VehicleData = vehicleData;
                if (Vehicle is null)
                {
                    Vehicle = ENet.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(vehicleData.Model), Position.GetVector3(), (float)Position.Heading, 0, 0, vehicleData.NumberPlate, 255, false, true, Garage.GetDimension());
                    Vehicle.SetVehicleData(vehicleData);
                    Vehicle.SetType(VehicleType.Personal);
                    Vehicle.ApplyCustomization();

                    Vehicle.VehicleData.Position = Position;

                    Vehicle.SetSharedData("model.name", vehicleData.Model);
                    Vehicle.SetSharedData("owner", vehicleData.Owner);
                    Vehicle.SetData("in.garage", this);

                    Vehicle.LockStatus(true);
                    Position.Set(Vehicle);
                }

                return true;
            }
            catch(Exception ex) { Logger.WriteError("SpawnVehicle", ex); return false; }
        }

        public async void TakeVehicle(ENetPlayer player)
        {
            try
            {
                if (Vehicle is null) return;

                Transition.Open(player, "Выезжаем из гаража");

                var vehicle = Vehicle;
                Vehicle = null;
                VehicleData = null;

                await Task.Delay(500);

                Garage.GetExteriorPosition().Set(vehicle);
                vehicle.SetDimension(0);

                if (player != null)
                {
                    Garage.GetExteriorPosition().Set(player);
                    player.SetDimension(0);
                }

                vehicle.ResetData("in.garage");

                await Task.Delay(100);

                if (player != null)
                    NAPI.Task.Run(() => player.SetIntoVehicle(vehicle, (int)VehicleSeat.Driver));

                vehicle.EngineState(true);

                if (player != null)
                {
                    player.CharacterData.ExteriosPosition = null;
                    player.SessionData.EnteredHouse = -1;
                }

                await Task.Delay(200);
                Transition.Close(player);
            }
            catch(Exception ex) { Logger.WriteError("TakeVehicle", ex); }
        }
    }
}

