using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Framework.Classes.Sessions.Classes
{
    public class AdminData
    {
        public bool IsSpectating { get; set; } = false;
        public uint SpectateDimension { get; set; } = 0;
    }
}
