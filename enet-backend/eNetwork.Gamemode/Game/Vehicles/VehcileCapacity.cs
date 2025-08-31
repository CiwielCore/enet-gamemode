using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Vehicles
{
    public class VehcileCapacity
    {
        private static readonly Dictionary<uint, int> List = new Dictionary<uint, int>()
        {
            { NAPI.Util.GetHashKey("pts21"), 8 },
        };

        public static int Get(string model)
        {
            return List[NAPI.Util.GetHashKey(model)] | 0;
        }

        public static int Get(uint model)
        {
            return List[model] | 0;
        }
    }
}
