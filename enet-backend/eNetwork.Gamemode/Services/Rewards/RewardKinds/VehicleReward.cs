using eNetwork.Game.Vehicles;
using System;
using System.Threading.Tasks;

namespace eNetwork.Services.Rewards.RewardKinds
{
    internal class VehicleReward : IReward
    {
        public string ModelName { get; set; }

        public string GetName()
        {
            return $"транспорт модели {ModelName}";
        }

        public void GiveReward(ENetPlayer player)
        {
            GiveRewardOffline((uint)player.GetUUID());
            player.SendInfo($"Вы получили в награду {GetName()}");
        }

        public void GiveRewardOffline(uint characterId)
        {
            Task.Run(() => VehicleManager.CreateVehicle(new VehicleOwner(OwnerVehicleEnum.Player, (int)characterId), ModelName, 0, 0));
        }

        public void Init(string rewardData)
        {
            if (!IsValidData(rewardData))
                throw new ArgumentException("RewardData not valid");

            ModelName = rewardData;
        }

        public bool IsValidData(string rewardData)
        {
            return string.IsNullOrWhiteSpace(rewardData) is false;
        }
    }
}