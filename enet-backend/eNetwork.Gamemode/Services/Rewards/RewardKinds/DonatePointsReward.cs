using System;
using System.Threading.Tasks;
using eNetwork.Game.Accounts;
using MySqlConnector;

namespace eNetwork.Services.Rewards.RewardKinds
{
    class DonatePointsReward : IReward
    {
        public int Amount { get; set; }

        public string GetName()
        {
            return $"донат валюта на сумму {Amount}";
        }

        public void GiveReward(ENetPlayer player)
        {
            GiveRewardOffline((uint)player.GetUUID());
            player.SendInfo($"Вы получили {GetName()}");
        }

        public void GiveRewardOffline(uint characterId)
        {
            AccountData account = AccountManager.GetAccountByUUID((int)characterId);
            if (account is null)
                throw new NullReferenceException($"For player {(int)characterId} account not found");

            account.DonatePoints += Amount;
            MySqlCommand command = new MySqlCommand(@"
                UPDATE `accounts`
                SET `donatepoints`=@amount
                WHERE `login`=@login;
            ");

            command.Parameters.AddWithValue("@amount", account.DonatePoints);
            command.Parameters.AddWithValue("@login", account.Login);
            Task.Run(() => ENet.Database.ExecuteAsync(command));
        }

        public void Init(string rewardData)
        {
            if (!IsValidData(rewardData))
                throw new ArgumentException("RewardData not valid");

            Amount = int.Parse(rewardData);
        }

        public bool IsValidData(string rewardData)
        {
            return int.TryParse(rewardData, out _);
        }
    }
}
