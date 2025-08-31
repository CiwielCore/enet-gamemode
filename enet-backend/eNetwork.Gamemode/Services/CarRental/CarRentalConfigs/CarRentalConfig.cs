using System;
using eNetwork.Framework;

namespace eNetwork.Services.CarRental
{
    internal class CarRentalConfig
    {
        public CarRentalPointConfig PointConfig { get; set; }
        public CarRentalDialogConfig DialogConfig { get; set; }
        public CarRentalServiceConfig ServiceConfig { get; set; }

        public CarRentalConfig()
        {
            PointConfig = ConfigReader.Read<CarRentalPointConfig>("car_rental/point_config");
            DialogConfig = ConfigReader.Read<CarRentalDialogConfig>("car_rental/dialog_config");
            ServiceConfig = ConfigReader.Read<CarRentalServiceConfig>("car_rental/service_config");
        }
    }
}
