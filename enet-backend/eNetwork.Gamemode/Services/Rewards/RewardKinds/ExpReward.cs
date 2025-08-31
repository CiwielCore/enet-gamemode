using System;
using System.Threading.Tasks;
using eNetwork.Framework;
using eNetwork.Game.Characters;
using GTANetworkAPI;
using MySqlConnector;

namespace eNetwork.Services.Rewards.RewardKinds
{
    class ExpReward : IReward
    {
        public int Amount { get; set; }

        public string GetName()
        {
            return $"опыт x{Amount}";
        }

        public void GiveReward(ENetPlayer player)
        {
            GiveRewardOffline((uint)player.GetUUID());
            player.SendInfo($"Вы получили в награду {GetName()}");
        }

        public void GiveRewardOffline(uint characterId)
        {
            CharacterData character = CharacterManager.GetCharacterData((int)characterId);
            if (character is null)
                throw new NullReferenceException($"Character with id {characterId} not found");

            character.Exp += Amount;
            MySqlCommand command = new MySqlCommand(@"
                UPDATE `characters`
                SET `exp`=@amount
                WHERE `uuid`=@characterId;
            ");

            command.Parameters.AddWithValue("@amount", character.Exp);
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
