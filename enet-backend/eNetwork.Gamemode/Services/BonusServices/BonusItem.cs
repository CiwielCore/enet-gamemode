using eNetwork.Framework;
using eNetwork.Services.Rewards;
using System;

namespace eNetwork.Services.BonusServices
{
    public class BonusItem
    {
        private static Logger _logger = new Logger("bonus-item");

        public RewardTypes Type { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public string Url { get; set; }

        public BonusItem(RewardTypes lootItemType, string name, string parameters, string url)
        {
            Type = lootItemType; Name = name; Parameters = parameters; Url = url;
        }

        public bool Get(ENetPlayer player)
        {
            try
            {
                if (RewardService.Instance.IsValidRewardData(Type, Parameters) is false)
                {
                    player.SendError($"Неправильные параметры приза для {Type} -> {Parameters}");
                    return false;
                }

                IReward reward = RewardService.Instance.GetRewardByType(Type, Parameters);
                if (reward is null)
                    return false;

                reward.GiveReward(player);
                return true;
            }
            catch (Exception ex)
            {
                _logger.WriteError("BonusPromotion-Get: \n" + ex.ToString());
                return false;
            }
        }
    }
}
