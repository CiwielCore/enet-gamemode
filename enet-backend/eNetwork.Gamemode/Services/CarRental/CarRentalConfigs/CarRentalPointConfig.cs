using GTANetworkAPI;

namespace eNetwork.Services.CarRental
{
    class CarRentalPointConfig
    {
        public uint PedModelHash { get; set; }

        public float ColShapeRange { get; set; }
        public float ColShapeHeight { get; set; }

        public bool BlipsEnable { get; set; }
        public string BlipName { get; set; }
        public uint BlipSprite { get; set; }
        public float BlipScale { get; set; }
        public byte BlipColor { get; set; }

        public bool MarkerEnable { get; set; }
        public uint MarkerType { get; set; }
        public float MarkerScale { get; set; }
        public Color MarkerColor { get; set; }
    }
}
