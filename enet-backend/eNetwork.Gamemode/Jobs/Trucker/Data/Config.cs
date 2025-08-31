using System;
using System.Collections.Generic;
using System.Text;
using eNetwork.External;
using eNetwork.Framework;

namespace eNetwork.Jobs.Trucker
{
    internal class Config
    {
        public static List<Position> VehSpawns = new List<Position>(){
            new Position(-120.87803, -2534.643, 6.732869, -125.0),
            new Position(-119.01775, -2531.8928, 6.734426, -125.0),
            new Position(-117.2897, -2529.2651, 6.7332377, -125.0),
            new Position(-115.41039, -2526.7095, 6.733633, -125.0),
            new Position(-113.61369, -2524.0881, 6.734153, -125.0),
            new Position(-111.80647, -2521.3184, 6.7345934, -125.0),
            new Position(-109.82693, -2518.8057, 6.7341557, -125.0)
        };

        public static List<Position> LoadPos = new List<Position>(){
            new Position(-1057.5896, -2011.8907, 12.7, 137.0)
        };

        public static List<Position> UnLoadPos = new List<Position>(){
            new Position(-1193.7662, -1480.6217, 4.1141224, -54)
        };

        public static Position NPCPos = new Position(-113.22457, -2514.4058, 6, -133.9571f);
    }
}
