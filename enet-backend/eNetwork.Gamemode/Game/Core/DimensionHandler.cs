using System;
using System.Collections.Generic;
using System.Text;
using eNetwork.Framework;
using GTANetworkAPI;

namespace eNetwork.Game
{
    class DimensionHandler
    {
        private static readonly Logger Logger = new Logger("DimensionHandler");

        private static Dictionary<int, Entity> DimensionsInUse = new Dictionary<int, Entity>();
        private static ICollection<int> Keys = DimensionsInUse.Keys;

        public static uint RequestPrivateDimension(ENetPlayer requester)
        {
            int firstUnusedDim = 10000;

            lock (DimensionsInUse)
            {
                while (DimensionsInUse.ContainsKey(--firstUnusedDim)) { }
                DimensionsInUse.Add(firstUnusedDim, requester);
            }
            return (uint)firstUnusedDim;
        }
        public static void DismissPrivateDimension(ENetPlayer requester)
        {
            try
            {
                foreach (KeyValuePair<int, Entity> dim in DimensionsInUse)
                {
                    if (dim.Value == requester.Handle)
                        DimensionsInUse.Remove(dim.Key);
                    break;
                }
            }
            catch (Exception e) { Logger.WriteError("DismissPrivateDimension", e); }
        }
        public static uint GetPlayerDimension(ENetPlayer player)
        {
            foreach (var key in Keys)
                if (DimensionsInUse[key] == player.Handle) return (uint)key;
            return 0;
        }
    }
}
