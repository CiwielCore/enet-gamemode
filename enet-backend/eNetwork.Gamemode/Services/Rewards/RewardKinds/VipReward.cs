using eNetwork.Services.VipServices;
using System;
using System.Threading.Tasks;

namespace eNetwork.Services.Rewards.RewardKinds
{
    class VipReward : IReward
    {
        public string VipName { get; set; }
        public int VipDays { get; set; }

        public string GetName()
        {
            return $"VIP-статус {VipName} на {VipDays}д.";
        }

        public void GiveReward(ENetPlayer player)
        {
            GiveRewardOffline((uint)player.GetUUID());
            player.SendInfo($"Вы получили в награду {GetName()}");
        }

        public void GiveRewardOffline(uint characterId)
        {
            VipStatus status = new VipStatus()
            {
                VipName = VipName,
                DateOfIssue = DateTime.Now,
                DateOfEnd = DateTime.Now.AddDays(VipDays)
            };

            Task.Run(() => VipService.Instance.GiveVipStatusToCharacter((int)characterId, status));
        }

        public void Init(string rewardData)
        {
            if (!IsValidData(rewardData))
                throw new ArgumentException("RewardData not valid");

            string[] splitted = rewardData.Split('_');
            VipName = splitted[0];
            VipDays = Convert.ToInt32(splitted[1]);
        }

        public bool IsValidData(string rewardData)
        {
            if (string.IsNullOrWhiteSpace(rewardData))
                return false;

            string[] splitted = rewardData.Split('_');
            if (splitted.Length < 2)
                return false;

            if (int.TryParse(splitted[1], out _))
                return false;

            if (VipService.Instance.GetVipByName(splitted[0]) is null)
                return false;

            return true;
        }
    }
}
