using eNetwork.Framework;
using eNetwork.Framework.Classes;
using eNetwork.Framework.Singleton;
using eNetwork.Game.Vehicles;
using eNetwork.GameUI;
using eNetwork.Services.CarRental.CarRentalModels;
using eNetwork.Services.DrivingLicensing;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eNetwork.Services.CarRental
{
    internal class CarRentalService : Singleton<CarRentalService>
    {
        public static event EventHandler<CarRentArgs> PlayerRentCarEvent;

        public CarRentalConfig Config => _config;
        private CarRentalConfig _config;

        private readonly List<CarRentalData> _rentsData;

        private CarRentalService()
        {
            _config = new CarRentalConfig();
            _rentsData = new List<CarRentalData>();
        }

        public bool IsPlayerOwnerVehicle(ENetPlayer player, ENetVehicle vehicle)
        {
            CarRentalData rentalData = _rentsData.FirstOrDefault(d => d.RentalCar == vehicle);
            if (rentalData == null)
                return false;

            return rentalData.CharacterId == player.CharacterData.UUID;
        }

        public void ShowCarRentalMenu(ENetPlayer player, CarRentalPoint rentalPoint)
        {
            var vehicles = CarRentalRepository.Instance.CarRentalModels.ToList();
            var colors = _config.ServiceConfig.Colors.ToList();
            var timeOptions = _config.ServiceConfig.RentalHoursOptions.ToList();

            Dialogs.Close(player);
            ClientEvent.Event(player,
                "client.car_rental.open_menu",
                JsonConvert.SerializeObject(vehicles),
                JsonConvert.SerializeObject(colors),
                JsonConvert.SerializeObject(timeOptions)
                );
        }

        public void RentACar(ENetPlayer player, int rentHour, CarRentalModel rentCarData, Color color)
        {
            if (!player.GetData(nameof(CarRentalPoint), out CarRentalPoint rentalPoint))
                return;

            if (_rentsData.Any(d => d.CharacterId == player.CharacterData.UUID))
            {
                player.SendError("Вы уже арендовывали транспорт");
                return;
            }

            if (
                VehicleSync.GetVehicleConfig(rentCarData.ModelName, out VehicleConfig vehConfig) &&
                vehConfig != null &&
                vehConfig.LicenseClass.HasValue &&
                !DrivingLicenseService.Instance.GetDrivingLicense(player).HasCategory(vehConfig.LicenseClass.Value)
                )
            {
                player.SendWarning($"У вас нет необходимой категории водительских прав: {vehConfig.LicenseClass.Value.ToDesignation()}");
                return;
            }

            double price = Convert.ToUInt32(rentCarData.CostPerHour * rentHour);

            if (!player.ChangeWallet(-price))
            {
                player.SendError("Недостаточно средств");
                return;
            }

            Position spawn = rentalPoint.GetCarSpawnPosition();
            ENetVehicle vehicle = CreateVehicle(rentCarData.ModelName, color, spawn);
            CarRentalData rentalData = new CarRentalData()
            {
                CharacterId = player.CharacterData.UUID,
                CarModelName = rentCarData.ModelName,
                RentalCar = vehicle,
                RentTimeInHour = rentHour
            };
            int ms = rentHour * 60 * 60 * 1000;
            rentalData.StopRentTimerId = Timers.StartOnce(ms, () => RentTimeLeftHandler(rentalData));
            ClientEvent.Event(player, "client.car_rental.close");
            _rentsData.Add(rentalData);
            player.SendInfo($"Вы арендовали {rentCarData.ModelName} на {rentHour}ч.");
            player.SetIntoVehicle(vehicle, (int)VehicleSeat.Driver);

            PlayerRentCarEvent?.Invoke(player,
                new CarRentArgs(
                    rentHour,
                    (int)price,
                    rentCarData.ModelName
                    )
                );
        }

        private void RentTimeLeftHandler(CarRentalData rentalData)
        {
            if (rentalData is null)
                return;

            if (rentalData.RentalCar != null)
                NAPI.Task.Run(() => rentalData.RentalCar.Delete());

            ENetPlayer player = ENet.Pools.GetPlayerByUUID(rentalData.CharacterId);
            if (player != null)
                player.SendWarning($"Аренда авто {rentalData.CarModelName} на {rentalData.RentTimeInHour}ч. закончилась");

            _rentsData.Remove(rentalData);
        }

        private ENetVehicle CreateVehicle(string modelName, Color color, Position spawn)
        {
            uint modelHash = (uint)(VehicleHash)NAPI.Util.GetHashKey(modelName);
            ENetVehicle vehicle = ENet.Vehicle.CreateVehicle(
                modelHash,
                spawn.GetVector3(),
                (float)spawn.Heading,
                color.ToInt32(),
                color.ToInt32(),
                GenerateNumberPlateText(),
                255, true, false, 0
                );

            vehicle.SetType(VehicleType.Rental);
            vehicle.LockStatus(true);
            vehicle.EngineState(false);
            NAPI.Vehicle.SetVehicleCustomPrimaryColor(vehicle, color.Red, color.Green, color.Blue);
            return vehicle;
        }

        private string GenerateNumberPlateText()
        {
            return Guid.NewGuid().ToString()[..8];
        }
    }
}
