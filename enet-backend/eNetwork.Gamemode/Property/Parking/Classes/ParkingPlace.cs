using eNetwork.Framework;
using eNetwork.Game.Vehicles;
using eNetwork.GameUI;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Property.Parking
{
    public class ParkingPlace
    {
        private static readonly Logger Logger = new Logger("parking-place");

        public int Id { get; set; }
        public int Owner { get; set; }
        public uint Dimension { get; set; }
        public int VehicleId { get; set; }
        public int PositionIndex { get; set; }
        public DateTime PaidDate { get; set; }
        public int ParkingId { get; set; }
        public int Floor { get; set; }

        public ParkingPlace(int id, int owner, uint dimension, int vehicleId, int positionIndex, DateTime paidDate, int parkingId, int floor)
        {
            Id = id;
            Owner = owner;
            Dimension = dimension;
            VehicleId = vehicleId;
            PositionIndex = positionIndex;
            PaidDate = paidDate;
            ParkingId = parkingId;
            Floor = floor;

            SpawnVehicle();
            GTAElements();
        }

        [JsonIgnore]
        public ENetVehicle Vehicle { get; set; }

        [JsonIgnore]
        public TextLabel TextLabel { get; set; }

        public void GTAElements()
        {
            try
            {
                if (!ParkingManager.Parkings.TryGetValue(ParkingId, out Parking parking)) return;
                var interiorData = Interior.GetInteriorData(parking.InteriorType);
                if (interiorData is null)
                {
                    Logger.WriteError("Не удалось найти данные о интерьере паркинга: " + parking.InteriorType.ToString());
                    return;
                }
                Position pos = interiorData.Positions.ElementAt(PositionIndex);
                if (pos is null) return;

                TextLabel = NAPI.TextLabel.CreateTextLabel($"Парковочное место #{Id}", pos.GetVector3() + new Vector3(0, 0, .5), 10f, 1f, 0, new Color(255, 255, 255, 220), false, Dimension);
            }
            catch(Exception ex) { Logger.WriteError("GTAElements", ex); }
        }

        public void SpawnVehicle()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (VehicleId == -1 || !ParkingManager.Parkings.TryGetValue(ParkingId, out Parking parking) || !VehicleManager.Vehicles.TryGetValue(VehicleId, out VehicleData data)) return;
                    
                    var foundedCar = ENet.Pools.GetVehicleById(VehicleId);
                    if (foundedCar != null && !foundedCar.HasData("in.parking"))
                        foundedCar.Delete();

                    var interiorData = Interior.GetInteriorData(parking.InteriorType);
                    if (interiorData is null)
                    {
                        Logger.WriteError("Не удалось найти данные о интерьере паркинга: " + parking.InteriorType.ToString());
                        return;
                    }
                    Position currentPosition = interiorData.Positions.ElementAt(PositionIndex);

                    if (Vehicle is null)
                    {
                        Vehicle = ENet.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(data.Model), currentPosition.GetVector3(), (float)currentPosition.Heading, 0, 0, data.NumberPlate, 255, false, true, Dimension);
                        Vehicle.SetVehicleData(data);
                        Vehicle.SetType(VehicleType.Personal);
                        Vehicle.ApplyCustomization();

                        Vehicle.VehicleData.Position = parking.Position;

                        Vehicle.SetSharedData("model.name", data.Model);
                        Vehicle.SetSharedData("owner", Owner);
                        Vehicle.SetData("in.parking", this);

                        Vehicle.LockStatus(true);
                        currentPosition.Set(Vehicle);
                    }

                    data.ParkedPlace = $"parking_{Id}";
                }
                catch (Exception ex) { Logger.WriteError("SpawnVehicle", ex); }
            });
        }

        public async void TakeVehicle(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData)) return;
                var parking = ParkingManager.GetParking(ParkingId);
                if (parking is null) return;

                Transition.Open(player, "Выезжаем из паркинга");

                var vehicle = Vehicle;
                Vehicle = null;

                await Task.Delay(500);
                parking.EnterPosition.Set(vehicle);
                vehicle.SetDimension(0);

                parking.EnterPosition.Set(player);
                player.SetDimension(0);

                vehicle.ResetData("in.parking");

                await Task.Delay(100);

                NAPI.Task.Run(() => player.SetIntoVehicle(vehicle, (int)VehicleSeat.Driver));
                vehicle.EngineState(true);
                characterData.ExteriosPosition = null;

                await Task.Delay(200);
                Transition.Close(player);
            }
            catch (Exception ex) { Logger.WriteError("TakeVehicle", ex); }
        }

        public void Buy(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData)) return;

                var parking = ParkingManager.GetParking(ParkingId);
                if (parking is null) return;

                if (Owner != -1)
                {
                    player.SendError($"Парковочное место #{Id} уже куплено");
                    return;
                }

                if (!player.ChangeWallet(-parking.Cost)) return;

                Owner = characterData.UUID;
                VehicleId = -1;
                player.SendDone($"Поздравляем с приобритением Парковочного места #{Id}! Не забудьте оплатить налог в банке");

                SpawnVehicle();
                Save();
            }
            catch(Exception ex) { Logger.WriteError("Buy", ex); }
        }

        public void Save()
        {
            try
            {
                ENet.Database.Execute($"UPDATE `{ParkingManager.PlacesDB}` SET `owner`={Owner}, vehid={VehicleId}, paid='{Helper.ConvertTime(PaidDate)}' WHERE `id`={Id}");
            }
            catch(Exception ex) { Logger.WriteError("Save", ex); }
        }
    }
}
