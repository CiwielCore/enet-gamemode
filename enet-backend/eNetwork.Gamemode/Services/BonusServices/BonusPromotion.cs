using eNetwork.Framework;
using System;
using System.Collections.Generic;

namespace eNetwork.Services.BonusServices
{
    public class BonusPromotion
    {
        private static Logger _logger = new Logger("bonus-promotion");

        public int ID { get; set; }
        public BonusType Type { get; set; }
        public List<BonusItem> Prize { get; set; }
        public int Min { get; set; }
        public DateTime Ending { get; set; }
        public bool IsTaked { get; set; } = false;

        public BonusPromotion(int id, BonusType bonusType, List<BonusItem> prize, int min, DateTime end)
        {
            ID = id; Type = bonusType; Prize = prize; Min = min; Ending = end;
        }

        public int CheckLastBonusDay(int uuid)
        {
            try
            {
                int i = 0;
                var list = DailyBonus.Instance.GetPlayerBonusData(uuid).BonusDays;
                foreach (var item in list)
                    foreach (var b in item)
                    {
                        if (!b) return i;
                        i++;
                    }
                return i;
            }
            catch (Exception ex)
            {
                _logger.WriteError("BonusPromotion-Check: \n" + ex.ToString());
                return 0;
            }
        }

        public void Check(ENetPlayer player)
        {
            try
            {
                if (Ending < DateTime.Now)
                    return;

                int uuid = player.GetUUID();

                if (DailyBonus.Instance.GetPlayerBonusData(uuid) is null)
                    DailyBonus.Instance.LoadPromonionts(uuid);

                PlayerBonus bonusData = DailyBonus.Instance.GetPlayerBonusData(uuid);

                var pTime = player.GetCharacter().Stats.TimePlaying;
                switch (Type)
                {
                    case BonusType.BonusDays:
                        var list = bonusData.BonusDays;
                        int lastBonued = CheckLastBonusDay(uuid);

                        if (pTime == Min && !list[ID][lastBonued])
                        {
                            if (Prize.Count > lastBonued)
                            {
                                bonusData.BonusDays[ID][lastBonued] = true;
                                player.SendInfo($"Вы получили {Prize[lastBonued].Name} за достижение {lastBonued + 1} уровня ежедневного события");
                                bonusData.Storage.Add(Prize[lastBonued]);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.WriteError("BonusPromotion-Check: \n" + ex.ToString());
            }
        }
    }
}
