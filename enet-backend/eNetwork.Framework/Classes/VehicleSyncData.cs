using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework
{
    public enum VehicleType 
    { 
        Server,
        Personal,
        Work,
        Fraction,
        Family,
        Testdrive,
        Rental,
        Theft
    }
    public class VehicleSyncData
    {
        public bool Locked { get; set; } = false;
        public bool Engine { get; set; } = false;
        public bool LeftIL { get; set; } = false;
        public bool RightIL { get; set; } = false;
        public float Dirt { get; set; } = 0.0f;
        public int Color1 { get; set; } = -1;
        public int Color2 { get; set; } = -1;
        public float BodyHealth { get; set; } = 1000.0f;
        public float EngineHealth { get; set; } = 1000.0f;

        //Doors 0-7 (0 = closed, 1 = open, 2 = broken) (This uses enums so don't worry about it)
        public int[] Door { get; set; } = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        //Windows (0 = up, 1 = down, 2 = smashed) (This uses enums so don't worry about it)
        public int[] Window { get; set; } = new int[4] { 0, 0, 0, 0 };

        //Wheels 0-7, 45/47 (0 = fixed, 1 = flat, 2 = missing) (This uses enums so don't worry about it)
        public int[] Wheel { get; set; } = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    }
}
