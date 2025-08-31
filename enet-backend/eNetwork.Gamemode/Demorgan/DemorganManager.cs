using eNetwork.Demorgan.Models;
using eNetwork.Framework;
using eNetwork.Framework.Singleton;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace eNetwork.Demorgan
{
    class DemorganManager : Singleton<DemorganManager>
    {
        private const string ZoneConfigPath = "demorgan/zone.json";
        private readonly DemorganZone _zone;

        private DemorganManager()
        {
            _zone = ConfigReader.Read<DemorganZone>(ZoneConfigPath);
        }

        public void UpdateEveryMinute(ENetPlayer player)
        {
            if (player.CharacterData is null)
                return;

            if (!DemorganRepository.Instance.IsCharacterInDemorgan(player.CharacterData.UUID))
                return;

            DemorganInfo info = DemorganRepository.Instance.GetDemorganInfo(player.CharacterData.UUID);
            if (--info.MinutesLeft <= 0)
            {
                ReleasePlayerInDemorgan(player);
            }
        }

        public void PutPlayerInDemorgan(ENetPlayer player, DemorganInfo info)
        {
            player.SendWarning("Вас посадили в деморган");
            // TODO: send to global chat
            _zone.PlacePlayerInZone(player);

            object data = new
            {
                info.Reason,
                info.MinutesLeft,
                Admin = Game.Characters.CharacterManager.GetName(info.AdminCharacterId),
            };
            ClientEvent.Event(player, "client.demorgan.set", _zone.Position, _zone.Range, _zone.Height, JsonConvert.SerializeObject(data));
        }

        public void ReleasePlayerInDemorgan(ENetPlayer player)
        {
            player.SendInfo("Вы выпущены из деморгана");
            DemorganRepository.Instance.RemoveDemorganInfo(player.CharacterData.UUID);
            _zone.ReleasePlayerFromZone(player);
            ClientEvent.Event(player, "client.demorgan.clear");
        }

        public void RestoreInDemorgan(ENetPlayer player)
        {
            _zone.PlacePlayerInZone(player);
            ClientEvent.Event(player, "client.demorgan.start_zone_check", _zone.Position, _zone.Range, _zone.Height);
        }

        public bool IsZoneColShape(ColShape colShape)
        {
            return _zone.Colshape == colShape;
        }
    }
}
