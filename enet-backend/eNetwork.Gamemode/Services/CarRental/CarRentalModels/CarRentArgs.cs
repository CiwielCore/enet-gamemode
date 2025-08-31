using System;

namespace eNetwork.Services.CarRental.CarRentalModels
{
    class CarRentArgs : EventArgs
    {
        public int RentHours { get; }
        public int RentPrice { get; }
        public string CarModel { get; }

        public CarRentArgs(int rentHours, int rentPrice, string carModel)
        {
            RentHours = rentHours;
            RentPrice = rentPrice;
            CarModel = carModel;
        }
    }
}
