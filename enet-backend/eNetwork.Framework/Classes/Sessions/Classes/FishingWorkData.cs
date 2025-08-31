using eNetwork.Inv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.Classes.Sessions.Classes
{
    public class FishingWorkData
    {
        public bool CanDo { get; set; } = false;
        public bool MinigameStarted { get; set; } = false;
        public Item CurrentRod { get; set; } = null;
    }
}
