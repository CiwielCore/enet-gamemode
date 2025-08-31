using System;
using eNetwork.Services.Rewards.RewardKinds;

namespace eNetwork.Services.Rewards
{
    class RewardFactory
    {
        public IReward CreateReward(RewardTypes rewardType)
        {
            IReward reward = rewardType switch
            {
                RewardTypes.Money => new MoneyReward(),
                RewardTypes.DonatePoints => new DonatePointsReward(),
                RewardTypes.Exp => new ExpReward(),
                RewardTypes.Vip => new VipReward(),
                RewardTypes.Vehicle => new VehicleReward(),
                _ => throw new NotImplementedException($"Reward by type ({rewardType:G}) not Implemented")
            };

            return reward;
        }
    }
}
