using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.Pools.List
{
    public class PlayersPool : Script
    {
        private static List<ENetPlayer> _pool = new List<ENetPlayer>();

        public static List<ENetPlayer> GetPool() { return _pool; }

        [ServerEvent(Event.PlayerConnected)]
        public static void OnConnect(Player rPlayer)
        {
            ENetPlayer player = rPlayer as ENetPlayer;
            if (player == null)
                return;

            if (!_pool.Contains(player))
                _pool.Add(player);
        }

        public static void OnDisconnect(ENetPlayer player)
        {
            if (_pool.Contains(player))
                _pool.Remove(player);
        }
    }
}
