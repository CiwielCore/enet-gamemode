using System;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace eNetwork
{
    public class Position
    {
        public double X;
        public double Y;
        public double Z;
        public double Heading;

        [JsonConstructor]
        public Position(float x, float y, float z, float heading)
        {
            X = x; Y = y; Z = z; Heading = heading;
        }
        public Position(double x, double y, double z, double heading)
        {
            X = x; Y = y; Z = z; Heading = heading;
        }
        public Position(Vector3 vector, float heading)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
            Heading = heading;
        }
        public float GetHeading()
        {
            return (float)Heading;
        }
        public Vector3 GetVector3()
        {
            return new Vector3(X, Y, Z);
        }
        public void Set(NetHandle entity)
        {
            NAPI.Task.Run(() =>
            {
                NAPI.Entity.SetEntityPosition(entity, new Vector3(X, Y, Z));
                NAPI.Entity.SetEntityRotation(entity, new Vector3(0, 0, Heading));
            });
        }
    }
}
