using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Jobs.Trucker.Classes
{
    public class TruckerCarData
    {
        public int CharacterId { get; set; }
        public ENetVehicle RentalCar { get; set; }
        public string CarModelName { get; set; }
    }
}
