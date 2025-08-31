using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.Classes.Sessions.Classes
{
    public class BusWorkData
    {
        public bool IsStopped { get; set; }
        public string BusStationId { get; set; } = string.Empty;
        public int CheckpointId { get; set; } = -1;
    }
}
