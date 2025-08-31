using eNetwork.Framework;
using eNetwork.Framework.API.ChatCommand;
using eNetwork.Game.Characters;
using MySqlConnector;
using System;
using System.Threading.Tasks;

namespace eNetwork.Services.Rewards.RewardKinds
{
    class MoneyReward : IReward
    {
        public int Amount { get; set; }

        public string GetName()
        {
            return $"деньги на сумму {Amount}";
        }

        public void GiveReward(ENetPlayer player)
        {
            player.ChangeWallet(Amount);
            player.SendInfo($"Вы получили {GetName()}");
        }

        public void GiveRewardOffline(uint characterId)
        {
            CharacterData character = CharacterManager.GetCharacterData((int)characterId);
            if (character is null)
                throw new NullReferenceException($"Character with id {characterId} not found");

            character.Cash += Amount;
            MySqlCommand command = new MySqlCommand(@"
                UPDATE `characters`
                SET `cash`=@amount
                WHERE `uuid`=@characterId;
            ");

            command.Parameters.AddWithValue("@amount", character.Cash);
            command.Parameters.AddWithValue("@characterId", characterId);
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
