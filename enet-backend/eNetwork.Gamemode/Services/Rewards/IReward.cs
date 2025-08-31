namespace eNetwork.Services.Rewards
{
    public interface IReward
    {
        void Init(string rewardData);
        bool IsValidData(string rewardData);
        void GiveReward(ENetPlayer player);
        void GiveRewardOffline(uint characterId);
        string GetName();
    }
}
