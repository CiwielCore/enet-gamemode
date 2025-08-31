using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.Classes.Sessions.Classes
{
    public class WorkData
    {
        public bool IsWorking { get; set; }
        public ENetVehicle Vehicle { get; set; } = null;
        public BusWorkData BusWorkData { get; set; } = new BusWorkData();
        public TruckerWorkData TruckerWorkData { get; set; } = new TruckerWorkData();
        public string VehicleTimer { get; set; } = null;

        public FishingWorkData FishingWorkData { get; set; } = new FishingWorkData();
    }
}
