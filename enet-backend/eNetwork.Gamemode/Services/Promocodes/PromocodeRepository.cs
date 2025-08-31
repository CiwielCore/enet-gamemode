using eNetwork.Framework;
using eNetwork.Framework.Extensions;
using eNetwork.Framework.Utils;
using eNetwork.Services.Rewards;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace eNetwork.Services.Promocodes
{
    class PromocodeRepository
    {
        private readonly Logger _logger;
        private readonly List<Promocode> _promocodes;
        private readonly List<PromocodeInfo> _promocodesInfos;

        public PromocodeRepository(Logger logger)
        {
            _logger = logger;
            _promocodes = new List<Promocode>();
            _promocodesInfos = new List<PromocodeInfo>();
        }

        public void OnResourceStart()
        {
            LoadPromocodes();
            LoadPromocodeLevels();
            LoadPromocodeInfos();
        }

        public Promocode GetPromocodeByText(string promocodeText)
        {
            return _promocodes.FirstOrDefault(p => p.Text == promocodeText);
        }

        public Promocode GetPromocodeById(int promocodeId)
        {
            return _promocodes.FirstOrDefault(p => p.Id == promocodeId);
        }

        public IReadOnlyCollection<PromocodeInfo> GetPromocodeActivations(int promocodeId)
        {
            return _promocodesInfos.Where(p => p.PromocodeId == promocodeId).ToList();
        }

        public IReadOnlyCollection<PromocodeInfo> GetPromocodeInfos(int characterId)
        {
            return _promocodesInfos.Where(p => p.CharacterId == characterId).ToList();
        }

        public async Task CreatePromocodeInDB(Promocode promocode)
        {
            MySqlCommand command = new MySqlCommand(@"
                INSERT INTO `promocodes`
                (`promocode_text`, `owner_id`, `reward_type`, `reward_data`, `activations_limit`, `validity_period`)
                VALUES (@promo, @owner, @reward, @rewardData, @activationsLimit, @validityPeriod);
                SELECT LAST_INSERT_ID();
            ");

            command.Parameters.AddWithValue("@promo", promocode.Text);
            command.Parameters.AddWithValue("@owner", promocode.OwnerId);
            command.Parameters.AddWithValue("@reward", (int)promocode.RewardType);
            command.Parameters.AddWithValue("@rewardData", promocode.RewardData);
            command.Parameters.AddWithValue("@activationsLimit", promocode.ActivationsLimit);
            command.Parameters.AddWithValue("@validityPeriod", promocode.ValidityPeriod.ToUnix());

            DataTable result = await ENet.Database.ExecuteReadAsync(command);
            if (result is null || result.Rows.Count == 0)
                return;

            DataRow row = result.Rows[0];
            ulong id = Convert.ToUInt64(row[0]);
            promocode.Id = (int)id;
            _promocodes.Add(promocode);
        }

        public Task CreatePromocodeLevelInDB(PromocodeLevel level)
        {
            MySqlCommand command = new MySqlCommand(@"
                INSERT INTO `promocodes_levels`
                (`promocode_id`, `index`, `hours_to_play`, `reward_type`, `reward_data`)
                VALUES (@promoId, @index, @hours, @reward, @rewardData);
            ");

            command.Parameters.AddWithValue("@promoId", level.PromocodeId);
            command.Parameters.AddWithValue("@index", level.Index);
            command.Parameters.AddWithValue("@hours", level.HoursToPlay);
            command.Parameters.AddWithValue("@reward", (int)level.RewardType);
            command.Parameters.AddWithValue("@rewardData", level.RewardData);

            return ENet.Database.ExecuteAsync(command);
        }

        public Task CreatePromocodeInfoInDB(PromocodeInfo info)
        {
            MySqlCommand command = new MySqlCommand(@"
                INSERT INTO `promocodes_infos`
                (`character_id`, `promocode_id`, `promocode_level_index`)
                VALUES (@characterId, @promoId, @levelIndex);
            ");

            command.Parameters.AddWithValue("@characterId", info.CharacterId);
            command.Parameters.AddWithValue("@promoId", info.PromocodeId);
            command.Parameters.AddWithValue("levelIndex", info.PromocodeLevelIndex);

            _promocodesInfos.Add(info);

            return ENet.Database.ExecuteAsync(command);
        }

        public Task UpdatePromocodeInfoInDB(PromocodeInfo info)
        {
            MySqlCommand command = new MySqlCommand(@"
                UPDATE `promocodes_infos`
                SET 
                    `promocode_level_index`=@levelIndex,
                    `played_hours`=@hours,
                    `is_complete`=@isComplete
                WHERE `character_id`=@characterId;
            ");

            command.Parameters.AddWithValue("@characterId", info.CharacterId);
            command.Parameters.AddWithValue("@levelIndex", info.PromocodeLevelIndex);
            command.Parameters.AddWithValue("@hours", info.PlayedHours);
            command.Parameters.AddWithValue("@isComplete", info.IsComplete);

            return ENet.Database.ExecuteAsync(command);
        }

        private void LoadPromocodes()
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM `promocodes`;");
            DataTable result = ENet.Database.ExecuteRead(command);
            if (result is null || result.Rows.Count == 0)
            {
                _logger.WriteInfo("Promocodes table is empty.");
                return;
            }

            foreach (DataRow row in result.Rows)
            {
                int id = Convert.ToInt32(row["id"]);
                string text = Convert.ToString(row["promocode_text"]);
                int ownerId = Convert.ToInt32(row["owner_id"]);
                int rewardType = Convert.ToInt32(row["reward_type"]);
                string rewardData = Convert.ToString(row["reward_data"]);
                int activationsLimit = Convert.ToInt32(row["activations_limit"]);
                DateTime validityPeriod = TimeUtils.UnixTimeToDateTime(Convert.ToInt64(row["validity_period"]));
                
                Promocode promocode = new Promocode()
                {
                    Id = id,
                    Text = text,
                    OwnerId = ownerId,
                    RewardType = (RewardTypes)rewardType,
                    RewardData = rewardData,
                    ActivationsLimit = activationsLimit,
                    ValidityPeriod = validityPeriod
                };

                if (_promocodes.Exists(p => p.Text == text))
                    throw new InvalidOperationException($"Duplicate promocode text: {text}");

                _promocodes.Add(promocode);
            }

            _logger.WriteInfo($"Loaded {_promocodes.Count} promocodes");
        }

        private void LoadPromocodeLevels()
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM `promocodes_levels`;");
            DataTable result = ENet.Database.ExecuteRead(command);
            if (result is null || result.Rows.Count == 0)
            {
                _logger.WriteInfo("Promocodes levels table is empty.");
                return;
            }

            foreach (DataRow row in result.Rows)
            {
                int promocodeId = Convert.ToInt32(row["promocode_id"]);
                uint levelIndex = Convert.ToUInt32(row["index"]);
                uint hoursToPlay = Convert.ToUInt32(row["hours_to_play"]);
                int rewardType = Convert.ToInt32(row["reward_type"]);
                string rewardData = Convert.ToString(row["reward_data"]);

                if (RewardService.Instance.IsValidRewardData((RewardTypes)rewardType, rewardData) is false)
                    throw new ArgumentException($"Incorrect reward type or reward data for the promo code level: {levelIndex}");

                Promocode promocode = _promocodes.FirstOrDefault(p => p.Id == promocodeId);
                if (promocode is null)
                    throw new NullReferenceException($"{nameof(Promocode)} with this id ({promocodeId}) has been null");

                promocode.Levels.Add(levelIndex, new PromocodeLevel()
                {
                    Index = levelIndex,
                    PromocodeId = promocodeId,
                    HoursToPlay = hoursToPlay,
                    RewardType = (RewardTypes)rewardType,
                    RewardData = rewardData
                });
            }
        }

        private void LoadPromocodeInfos()
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM `promocodes_infos`;");
            DataTable result = ENet.Database.ExecuteRead(command);
            if (result is null || result.Rows.Count == 0)
            {
                _logger.WriteInfo("Promocodes infos table is empty.");
                return;
            }

            foreach (DataRow row in result.Rows)
            {
                int characterId = Convert.ToInt32(row["character_id"]);
                int promocodeId = Convert.ToInt32(row["promocode_id"]);
                uint promocodeLevelIndex = Convert.ToUInt32(row["promocode_level_index"]);
                uint playedHours = Convert.ToUInt32(row["played_hours"]);
                bool isComplete = Convert.ToBoolean(row["is_complete"]);

                _promocodesInfos.Add(new PromocodeInfo()
                {
                    CharacterId = characterId,
                    PromocodeId = promocodeId,
                    PromocodeLevelIndex = promocodeLevelIndex,
                    PlayedHours = playedHours,
                    IsComplete = isComplete
                });
            }
        }
    }
}
