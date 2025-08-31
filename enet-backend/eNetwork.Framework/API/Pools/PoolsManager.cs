using eNetwork.Framework.API.Pools.List;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.Pools
{
    public class PoolsManager
    {
        public PoolsManager() { }

        public List<ENetPlayer> GetAllPlayers()
        {
            return PlayersPool.GetPool();
        }

        public List<ENetPlayer> GetAllRegisteredPlayers()
        {
            return PlayersPool.GetPool()
                .Where(p => p.CharacterData != null).ToList();
        }

        public ENetPlayer GetPlayerByDynamic(int dynamicID)
        {
            return GetAllRegisteredPlayers()
                .FirstOrDefault(x => x.Value == dynamicID);
        }

        public ENetPlayer GetPlayerByUUID(int staticID)
        {
            return GetAllRegisteredPlayers()
                .FirstOrDefault(x => x.GetUUID() == staticID);
        }

        public IEnumerable<ENetPlayer> GetPlayersInRadius(Vector3 position, float radius, uint dim)
        {
            return GetAllRegisteredPlayers()
                .Where(x => x.Position.DistanceTo(position) < radius && x.Dimension == dim);
        }

        public List<ENetVehicle> GetAllVehicles()
        {
            return VehiclePool.GetPool();
        }

        public ENetVehicle GetVehicleById(int id)
        {
            return GetAllVehicles()
                .FirstOrDefault(x => x.GetVehicleData()?.ID == id);
        }

        public void OnDisconnect(ENetPlayer player)
        {
            PlayersPool.OnDisconnect(player);
        }
    }
}
