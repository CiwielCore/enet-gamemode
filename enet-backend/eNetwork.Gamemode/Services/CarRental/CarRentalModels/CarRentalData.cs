namespace eNetwork.Services.CarRental
{
    internal class CarRentalData
    {
        public int CharacterId { get; set; }
        public ENetVehicle RentalCar { get; set; }
        public string CarModelName { get; set; }
        public string StopRentTimerId { get; set; }
        public int RentTimeInHour { get; set; }
    }
}
