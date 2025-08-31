using GTANetworkAPI;
using Newtonsoft.Json;

namespace eNetwork.Demorgan
{
    class DemorganZone
    {
        public Vector3 Position { get; set; }
        public Vector3 PlayerSpawnPosition { get; set; }
        public Vector3 ReleaseSpawnPosition { get; set; }
        public float Range { get; set; }
        public float Height { get; set; }
        public uint Dimension { get; set; }

        [JsonIgnore]
        public ColShape Colshape => _colShape;
        private ColShape _colShape;

        public DemorganZone()
        {
            ENet.Task.SetMainTask(() =>
            {
                _colShape = NAPI.ColShape.CreateCylinderColShape(Position, Range, Height, Dimension);
            });
        }

        public void PlacePlayerInZone(ENetPlayer player)
        {
            NAPI.Task.Run(() =>
            {
                NAPI.Entity.SetEntityPosition(player, PlayerSpawnPosition);
                NAPI.Entity.SetEntityDimension(player, Dimension);
            });
        }

        public void ReleasePlayerFromZone(ENetPlayer player)
        {
            NAPI.Task.Run(() =>
            {
                NAPI.Entity.SetEntityPosition(player, ReleaseSpawnPosition);
                NAPI.Entity.SetEntityDimension(player, 0);
            });
        }
    }
}
