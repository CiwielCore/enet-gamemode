using System;
using GTANetworkAPI;
using eNetwork.Framework;
using eNetwork.Game;
using eNetwork.Services.VipServices;
using eNetwork.Services.VipServices.VipAddons;
using eNetwork.Jobs.Fishing;
using eNetwork.Game.Player;

namespace eNetwork.ServerEvents
{
    public class PlayerDeath : Script
    {
        private readonly Logger _logger = new Logger("player-death");

        private const int IdleTimeInSeconds = 3 * 60;
        private const int IdleTimeInSecondsIfPlayerHasVip = 1 * 60;
        private const int IdleTimeIfMedicsAreCalled = 10 * 60;

        private const string SetCauseOfDeathClientEvent = "client.player.set_cause_of_death";
        private const string SetDeathTimerClientEvent = "client.player.set_death_timer";
        private const string ShowDeathUIClientEvent = "client.player.show_death_ui";
        private const string HideDeathUIClientEvent = "client.player.hide_death_ui";

        private void HandlePlayerDeathEvent(ENetPlayer player, ENetPlayer killer, uint weapon)
        {
            try
            {
                Businesses.Showroom.EndTestdrive(player);
                WeaponController.OnDeath(player);
                Inventory.OnDeath(player);

                FishingManager.Reset(player);

                Animation.StopCustomAnimation(player);

                ClientEvent.Event(player, SetCauseOfDeathClientEvent, killer, weapon);
                ClientEvent.Event(player, ShowDeathUIClientEvent);
            }
            catch (Exception ex)
            {
                _logger.WriteError("HandlePlayerDeathEvent", ex);
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        private void OnPlayerDeathEvent(Player rPlayer, Player entityKiller, uint weapon)
        {
            ENetPlayer player = rPlayer as ENetPlayer;
            if (player == null)
                return;

            if (player is null || !player.GetCharacter(out CharacterData characterData))
                return;

            if (player.GetData<bool>("participantInEvent") == true)
            {
                player.SetData<bool>("participantInEvent", false);
            }

            HandlePlayerDeathEvent(player, entityKiller as ENetPlayer, weapon);
        }

        [ServerEvent(Event.PlayerSpawn)]
        private void OnPlayerSpawnEvent(Player player)
        {
            ClientEvent.Event(player as ENetPlayer, HideDeathUIClientEvent);
        }

        [CustomEvent("server.player_death.time_is_over")]
        public static void HandleDeathTimeIsOver(ENetPlayer player)
        {
            // TODO: Move in Hospital spawn method
            NAPI.Player.SpawnPlayer(player, player.Position, player.Heading);
            NAPI.Player.SetPlayerHealth(player, 10);

            Vip vip = VipService.Instance.GetVipOfPlayer(player);
            if (vip is IHandlerPlayerDeath vipDeathHandler)
            {
                vipDeathHandler.SetHp(player);
            }

            ClientEvent.Event(player, HideDeathUIClientEvent);
        }

        [CustomEvent("server.player_death.call_medic")]
        public static void HandleCallMedicEvent(ENetPlayer player)
        {
            // call medics
            ClientEvent.Event(player, SetDeathTimerClientEvent, IdleTimeIfMedicsAreCalled);
        }

        [CustomEvent("server.player_death.select_death")]
        public static void HandleSelectDeathEvent(ENetPlayer player)
        {
            Vip vip = VipService.Instance.GetVipOfPlayer(player);
            int idleTime = vip is null ? IdleTimeInSeconds : IdleTimeInSecondsIfPlayerHasVip;
            ClientEvent.Event(player, SetDeathTimerClientEvent, idleTime);
        }
    }
}
