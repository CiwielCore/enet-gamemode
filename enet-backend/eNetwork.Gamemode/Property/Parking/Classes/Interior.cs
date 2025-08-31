using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Property.Parking
{
    public class Interior
    {
        public static readonly Dictionary<InteriorType, Interior> Interiors = new Dictionary<InteriorType, Interior>()
        {
            { InteriorType.Garage20Places, new Interior(new Position(1295.1766, 264.45874, -48.88, -178), new List<Position>()
                {
                    new Position(1309.2826, 260.01514, -48.96, 90),     // 1
                    new Position(1309.2826, 256.8371, -48.96, 90),      // 2
                    new Position(1309.2826, 251.81628, -48.96, 90),     // 3
                    new Position(1309.2826, 247.40395, -48.96, 90),     // 4
                    new Position(1309.2826, 243.26126, -48.96, 90),     // 5
                    new Position(1309.2826, 239.4592, -48.96, 90),      // 6
                    new Position(1309.2826, 234.38127, -48.96, 90),     // 7
                    new Position(1309.2826, 229.53207, -48.96, 90),     // 8

                    new Position(1280.8823, 259.90802, -48.96, 90),     // 9
                    new Position(1280.8823, 256.4617, -48.96, -90),     // 10
                    new Position(1280.8823, 251.7557, -48.96, -90),     // 11
                    new Position(1280.8823, 247.95763, -48.96, -90),    // 12
                    new Position(1280.8823, 243.11328, -48.96, -90),    // 13
                    new Position(1280.8823, 239.82475, -48.96, -90),    // 14

                    new Position(1295.4562, 251.93298, -48.96, 90),     // 15
                    new Position(1295.4562, 248.04172, -48.96, 90),     // 16
                    new Position(1295.4562, 243.55049, -48.96, 90),     // 17
                    new Position(1295.4562, 239.73387, -48.96, 90),     // 18
                    new Position(1295.4562, 233.78453, -48.96, 90),     // 19
                    new Position(1295.4562, 229.4851, -48.96, 90),      // 20
                })
            }
        };

        public Position Enterpoint { get; set; }
        public List<Position> Positions { get; set; } = new List<Position>();

        public Interior(Position enterpoint, List<Position> positions)
        {
            Enterpoint = enterpoint;
            Positions = positions;
        }

        public static Interior GetInteriorData(InteriorType type)
        {
            Interiors.TryGetValue(type, out var interior);
            return interior;
        }
    }

    public enum InteriorType
    {
        Garage20Places
    }
}
