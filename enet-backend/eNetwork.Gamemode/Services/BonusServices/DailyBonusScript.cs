using eNetwork.Framework;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace eNetwork.Services.BonusServices
{
    internal class DailyBonusScript
    {
        private readonly Logger _logger = new Logger("daily-bonus-script");

        [ChatCommand("dailybonus", "Открывает меню ежедневного бонуса", Access = PlayerRank.Player)]
        public void OpenDailyMenuCommandHandler(ENetPlayer player)
        {
            try
            {
                if (!player.IsTimeouted("DAILYOPEN", 1))
                    return;

                if (player.GetCharacter() is null)
                    return;

                if (DailyBonus.Instance.GetPlayerBonusData(player.GetUUID()) is null)
                    DailyBonus.Instance.LoadPromonionts(player.GetUUID());

                PlayerBonus bonusData = DailyBonus.Instance.GetPlayerBonusData(player.GetUUID());

                var bonus = DailyBonus.Instance.GetBonusPromotions(BonusType.BonusDays).FirstOrDefault(b => b.ID == 0);

                ClientEvent.Event(player, "client.daily.open",
                    JsonConvert.SerializeObject(bonus.Prize),
                    JsonConvert.SerializeObject(bonusData.BonusDays[0]),
                    JsonConvert.SerializeObject(bonusData.Storage));
            }
            catch (Exception ex)
            {
                _logger.WriteError("RemoteEvent_OpenDailyMenu: \n" + ex.ToString());
            }
        }

        [CustomEvent("server.daily.storage.take")]
        public void RemoteEvent_TakeStorage(ENetPlayer player, int index)
        {
            try
            {
                if (!player.IsTimeouted("DAILYTAKE", 1))
                    return;

                if (player.GetCharacter() is null)
                    return;


                if (DailyBonus.Instance.GetPlayerBonusData(player.GetUUID()) is null)
                    DailyBonus.Instance.LoadPromonionts(player.GetUUID());

                PlayerBonus bonusData = DailyBonus.Instance.GetPlayerBonusData(player.GetUUID());
                BonusItem item = bonusData.Storage[index];
                if (!item.Get(player))
                    return;

                bonusData.Storage.Remove(item);
                player.SendInfo($"Вы забрали {item.Name}");
                ClientEvent.Event(player, "client.dailybonus.updateStorage", JsonConvert.SerializeObject(bonusData.Storage));
            }
            catch (Exception ex)
            {
                _logger.WriteError("RemoteEvent_TakeStorage: \n" + ex.ToString());
            }
        }
    }
}
