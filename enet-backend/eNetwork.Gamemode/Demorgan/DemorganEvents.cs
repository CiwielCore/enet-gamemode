using GTANetworkAPI;

namespace eNetwork.Demorgan
{
    class DemorganEvents : Script
    {
        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeathEventHandler(Player player, Player killer, uint reason)
        {
            ENetPlayer ePlayer = player as ENetPlayer;
            if (ePlayer == null)
                return;

            if (ePlayer?.CharacterData == null || !DemorganRepository.Instance.IsCharacterInDemorgan(ePlayer.CharacterData.UUID))
                return;

            NAPI.Player.SpawnPlayer(player, player.Position);
            DemorganManager.Instance.RestoreInDemorgan(ePlayer);
            System.Console.WriteLine("Player leave, restore to demorgan");
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public void OnPlayerExitColShapeEventHandler(ColShape colShape, Player player)
        {
            ENetPlayer ePlayer = player as ENetPlayer;
            if (ePlayer == null)
                return;

            if (ePlayer?.CharacterData == null)
                return;

            if (!DemorganRepository.Instance.IsCharacterInDemorgan(ePlayer.CharacterData.UUID) ||
                !DemorganManager.Instance.IsZoneColShape(colShape))
            {
                return;
            }

            DemorganManager.Instance.RestoreInDemorgan(ePlayer);
        }

        [CustomEvent("server.demorgan.leave_zone")]
        public void OnPlayerLeaveZoneEventHandler(ENetPlayer player)
        {
            if (player?.CharacterData == null || !DemorganRepository.Instance.IsCharacterInDemorgan(player.CharacterData.UUID))
                return;

            DemorganManager.Instance.RestoreInDemorgan(player);
        }
    }
}
