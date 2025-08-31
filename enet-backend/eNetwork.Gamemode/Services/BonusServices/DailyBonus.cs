using eNetwork.Framework;
using eNetwork.Framework.Singleton;
using eNetwork.Services.Rewards;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace eNetwork.Services.BonusServices
{
    public class DailyBonus : Singleton<DailyBonus>
    {
        public const string DBName = "e_dailybonuses";
        
        private Logger _logger;
        private List<BonusPromotion> _bonusPromotions;
        private Dictionary<int, PlayerBonus> _playerBonuses;

        private DailyBonus()
        {
            _logger = new Logger("daily-bonus");
            _bonusPromotions = new List<BonusPromotion>();
            _playerBonuses = new Dictionary<int, PlayerBonus>();
        }

        public void OnResourceStart()
        {
            try
            {
                _bonusPromotions.Add(new BonusPromotion(0, BonusType.BonusDays, new List<BonusItem>() {
                    new BonusItem(RewardTypes.Money, "5 0000 000$", "5000000", "cash.png"),
                    new BonusItem(RewardTypes.Vehicle, "Hakuchou", "hakuchou", "hakuchou.png"),
                    new BonusItem(RewardTypes.Money, "10 0000 000$", "10000000", "cash.png"),
                    new BonusItem(RewardTypes.DonatePoints, "50 Coins", "50", "donate.png"),
                    new BonusItem(RewardTypes.Money, "5 0000 000$", "5000000", "cash.png"), //Шмотка должна быть
                    new BonusItem(RewardTypes.Exp, "5 EXP", "5", "exp.png"),
                    new BonusItem(RewardTypes.DonatePoints, "100 Coins", "100", "donate.png"),
                    new BonusItem(RewardTypes.Money, "20 0000 000$", "20000000", "cash.png"),
                    new BonusItem(RewardTypes.Exp, "5 EXP", "5", "exp.png"),//Вверх гуччи
                    new BonusItem(RewardTypes.Exp, "5 EXP", "5", "exp.png"),//Низ гуччи
                    new BonusItem(RewardTypes.Money, "25 0000 000$", "25000000", "cash.png"),
                    new BonusItem(RewardTypes.Exp, "5 EXP", "5", "exp.png"),//Катаны
                    new BonusItem(RewardTypes.DonatePoints, "150 Coins", "150", "donate.png"),
                    new BonusItem(RewardTypes.Exp, "10 EXP", "10", "exp.png"),
                    new BonusItem(RewardTypes.Money, "30 0000 000$", "30000000", "cash.png"),
                    new BonusItem(RewardTypes.Exp, "10 EXP", "10", "exp.png"),//номерки
                    new BonusItem(RewardTypes.Money, "35 0000 000$", "35000000", "cash.png"),
                    new BonusItem(RewardTypes.DonatePoints, "200 Coins", "200", "donate.png"),
                    new BonusItem(RewardTypes.Vehicle, "Snow Mobile", "newsnowmobile", "newsnowmobile.png"),
                }, 60, new DateTime(2025, 1, 16, 0, 0, 0)));

                DataTable result = ENet.Database.ExecuteRead($"SELECT * FROM {DBName}");
                if (result is null || result.Rows.Count < 1)
                {
                    _logger.WriteWarning($"DB `{DBName}` return null result");
                    return;
                }

                foreach (DataRow Row in result.Rows)
                {
                    PlayerBonus data = new PlayerBonus();
                    int uuid = Convert.ToInt32(Row["uuid"]);
                    data.DailyBonus = JsonConvert.DeserializeObject<bool[]>(Row["dailybonus"].ToString());
                    data.BonusDays = JsonConvert.DeserializeObject<List<bool[]>>(Row["bonusday"].ToString());
                    data.CarBonus = JsonConvert.DeserializeObject<bool[]>(Row["carbonus"].ToString());
                    data.Storage = JsonConvert.DeserializeObject<List<BonusItem>>(Row["storage"].ToString());
                    _playerBonuses.Add(uuid, data);
                }
            }
            catch (Exception ex)
            {
                _logger.WriteError("OnResourceStart: \n" + ex.ToString());
            }
        }

        public PlayerBonus GetPlayerBonusData(int uuid)
        {
            return _playerBonuses.GetValueOrDefault(uuid);
        }

        public List<BonusPromotion> GetBonusPromotions(BonusType bonusDays)
        {
            return _bonusPromotions.FindAll(b => b.Type == bonusDays);
        }

        public void TriggerEveryMinuteEvent(ENetPlayer player)
        {
            CheckEveryoneMinute(player);
            UpdateTime(player);
        }

        public async Task<bool> Save(int uuid)
        {
            try
            {
                if (!_playerBonuses.ContainsKey(uuid))
                    LoadPromonionts(uuid);

                var data = _playerBonuses[uuid];
                await ENet.Database.ExecuteAsync(@$"
                    UPDATE {DBName} 
                    SET 
                        dailybonus='{JsonConvert.SerializeObject(data.DailyBonus)}'
                      , bonusday='{JsonConvert.SerializeObject(data.BonusDays)}'
                      , carbonus='{JsonConvert.SerializeObject(data.CarBonus)}'
                      , storage='{JsonConvert.SerializeObject(data.Storage)}'
                    WHERE uuid={uuid}
                ");
                return true;
            }
            catch (Exception ex)
            {
                _logger.WriteError("Save: \n" + ex.ToString());
                return false;
            }
        }

        public void LoadPromonionts(int uuid)
        {
            try
            {
                if (!_playerBonuses.ContainsKey(uuid))
                {
                    var data = new PlayerBonus();
                    _playerBonuses.Add(uuid, data);
                    ENet.Database.Execute(@$"
                        INSERT INTO {DBName}
                        (uuid, dailybonus, bonusday, carbonus, storage)
                        VALUES (
                            {uuid}
                         , '{JsonConvert.SerializeObject(data.DailyBonus)}'
                         , '{JsonConvert.SerializeObject(data.BonusDays)}'
                         , '{JsonConvert.SerializeObject(data.CarBonus)}'
                         , '{JsonConvert.SerializeObject(data.Storage)}'
                        )
                    ");
                }
            }
            catch (Exception ex)
            {
                _logger.WriteError("LoadPromonionts: \n" + ex.ToString());
            }
        }

        private void CheckEveryoneMinute(ENetPlayer player)
        {
            try
            {
                if (player.GetCharacter() is null)
                    return;

                foreach (BonusPromotion prom in _bonusPromotions)
                {
                    prom.Check(player);
                }
            }
            catch (Exception ex)
            {
                _logger.WriteError("Check: \n" + ex.ToString());
            }
        }

        private void UpdateTime(ENetPlayer player)
        {
            try
            {
                var pTime = player.GetCharacter().Stats.TimePlaying;
                var bonus = _bonusPromotions.Find(x => x.Type == BonusType.BonusDays && x.ID == 0);

                string timeText = "Следующая награда завтра";
                if (pTime < bonus.Min)
                {
                    DateTime date = new DateTime().AddMinutes(bonus.Min - pTime);
                    var hour = date.Hour;
                    var min = date.Minute;
                    string time = $"{Helper.FormatZero(hour)}:{Helper.FormatZero(min)}";

                    timeText = "До новой награды: " + time;
                }

                ClientEvent.Event(player, "client.daily.time.update", timeText);
            }
            catch (Exception ex)
            {
                _logger.WriteError("UpdateTime: \n" + ex.ToString());
            }
        }
    }
}
