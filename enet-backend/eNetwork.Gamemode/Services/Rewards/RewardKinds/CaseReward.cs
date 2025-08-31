using System;

namespace eNetwork.Services.Rewards.RewardKinds
{
    class CaseReward : IReward
    {
        public int Amount { get; set; }
        public string CaseName { get; set; }

        public string GetName()
        {
            return CaseName;
        }

        public void GiveReward(ENetPlayer player)
        {
            throw new NotImplementedException();
        }

        public void GiveRewardOffline(uint characterId)
        {
            throw new NotImplementedException();
        }

        public void Init(string rewardData)
        {
            throw new NotImplementedException();
        }

        public bool IsValidData(string rewardData)
        {
            throw new NotImplementedException();
        }
    }
}
