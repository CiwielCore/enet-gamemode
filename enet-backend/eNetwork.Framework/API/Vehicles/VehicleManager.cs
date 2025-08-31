using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using eNetwork.Framework;

namespace eNetwork.Framework.API.Vehicles
{
    public class VehicleManager
    {
        private static readonly Logger _logger = new Logger("vehicle-handler");
        public ENetVehicle CreateVehicle(uint model, Vector3 pos, float rot, int color1, int color2, string numberPlate = "", byte alpha = 255, bool locked = false, bool engine = true, uint dimension = 0)
        {
            try
            {
                var vehicle = NAPI.Vehicle.CreateVehicle(model, pos, rot, color1, color2, numberPlate, alpha, locked, engine, dimension);

                return (ENetVehicle)vehicle;
            }
            catch(Exception ex) { _logger.WriteError("CreateVehicle", ex); return null; }
        }
    }
}
