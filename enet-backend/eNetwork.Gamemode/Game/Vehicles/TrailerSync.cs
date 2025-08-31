using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using eNetwork.Framework;

namespace eNetwork.Game.Vehicles
{
    public class TrailerSync
    {
        public const string SyncData = "vehicle.trailer";
        private static Logger Logger = new Logger("trailer-sync");
        public static TrailerData CreateTrailer(string model)
        {
            try
            {
                return new TrailerData(model) { Number = "TRAILER", }; 
            }
            catch(Exception e) { Logger.WriteError("CreateTrailer", e); return null; }
        }
    }
    public class TrailerData 
    {
        public string Hash;
        public int[] Extra = new int[4] { 0, 0, 0, 0 };
        public int[] GolovaExtra = new int[4] { 0, 0, 0, 0 };
        public int Livery = 0;
        public string Number = "Trailer";
        public TrailerData(string hash)
        {
            Hash = hash;
        }
    }
}
