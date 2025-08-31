namespace eNetwork.Services.CarRental
{
    internal class CarRentalModel
    {
        public string ModelName { get; set; }
        public uint CostPerHour { get; set; }
        public string Type { get; set; }
        public string Img { get; set; }

        public CarRentalModel(
            string modelName,
            uint costPerHour,
            string type,
            string img)
        {
            ModelName = modelName;
            CostPerHour = costPerHour;
            Type = type;
            Img = img;
        }
    }
}
