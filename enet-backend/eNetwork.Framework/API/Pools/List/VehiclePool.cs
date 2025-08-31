using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.Pools.List
{
    public class VehiclePool : Script
    {
        private static List<ENetVehicle> _pool = new List<ENetVehicle>();

        public static List<ENetVehicle> GetPool() { return _pool; }

        [ServerEvent(Event.EntityCreated)]
        public static void OnCreate(Entity entity)
        {
            if (entity.Type != EntityType.Vehicle) return;
            if (!_pool.Contains((ENetVehicle)entity))
                _pool.Add((ENetVehicle)entity);
        }

        [ServerEvent(Event.EntityDeleted)]
        public static void OnDeleted(Entity entity)
        {
            if (entity.Type != EntityType.Vehicle) return;
            if (_pool.Contains((ENetVehicle)entity))
                _pool.Remove((ENetVehicle)entity);
        }
    }
}
