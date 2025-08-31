using GTANetworkAPI;

namespace eNetwork.Services.VipServices.VipAddons
{
    public interface IHandlerPlayerDeath
    {
        public int AmountOfHealthDuringSpawn { get; }

        public void SetHp(ENetPlayer player)
        {
            NAPI.Player.SetPlayerHealth(player, AmountOfHealthDuringSpawn);
        }
    }
}
