using eNetwork.Framework.Singleton;

namespace eNetwork.Services.Rewards
{
    class RewardService : Singleton<RewardService>
    {
        private readonly RewardFactory _factory;

        private RewardService()
        {
            _factory = new RewardFactory();
        }

        public IReward GetRewardByType(RewardTypes rewardType, string rewardData)
        {
            IReward reward = _factory.CreateReward(rewardType);
            reward.Init(rewardData);
            return reward;
        }

        public bool IsValidRewardData(RewardTypes rewardType, string rewardData)
        {
            IReward reward = _factory.CreateReward(rewardType);
            return reward.IsValidData(rewardData);
        }
    }
}
