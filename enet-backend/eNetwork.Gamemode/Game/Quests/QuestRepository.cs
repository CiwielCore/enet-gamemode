using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace eNetwork.Game.Quests
{
    class QuestRepository
    {
        public Task CreateQuestDataInDB(QuestPlayerData data)
        {
            MySqlCommand command = new MySqlCommand(@"
                INSERT INTO `player_quest_data`
                (`character_id`, `quest_line_id`, `quest_task_index`, `quest_progress`)
                VALUES (@charId, @questId, @taskIndex, @progress);
            ");

            command.Parameters.AddWithValue("@charId", data.CharacterId);
            command.Parameters.AddWithValue("@questId", (int)data.ActiveQuestLineId);
            command.Parameters.AddWithValue("@taskIndex", data.ActiveQuestTaskIndex);
            command.Parameters.AddWithValue("@progress", JsonConvert.SerializeObject(data.Progress));
            
            return ENet.Database.ExecuteAsync(command);
        }

        public Task UpdateQuestDataInDB(QuestPlayerData data)
        {
            MySqlCommand command = new MySqlCommand(@"
                UPDATE `player_quest_data`
                SET 
                    `quest_line_id`=@questId,
                    `quest_task_index`=@taskIndex,
                    `quest_progress`=@progress
                WHERE `character_id`=@charId;
            ");

            command.Parameters.AddWithValue("@charId", data.CharacterId);
            command.Parameters.AddWithValue("@questId", (int)data.ActiveQuestLineId);
            command.Parameters.AddWithValue("@taskIndex", data.ActiveQuestTaskIndex);
            command.Parameters.AddWithValue("@progress", JsonConvert.SerializeObject(data.Progress));

            return ENet.Database.ExecuteAsync(command);
        }

        public Task DeleteQuestDataFromDB(int characterId)
        {
            MySqlCommand command = new MySqlCommand(@"
                DELETE FROM `player_quest_data`
                WHERE `character_id`=@characterId;
            ");

            command.Parameters.AddWithValue("@characterId", characterId);

            return ENet.Database.ExecuteAsync(command);
        }

        public async Task<bool> ExistsQuestDataForCharacter(int characterId)
        {
            MySqlCommand command = new MySqlCommand(@"
                SELECT 1 FROM `player_quest_data`
                WHERE `character_id`=@characterId
                LIMIT 1;
            ");

            command.Parameters.AddWithValue("@characterId", characterId);
            DataTable dataTable = await ENet.Database.ExecuteReadAsync(command);
            if (dataTable is null || dataTable.Rows.Count < 1)
                return false;

            return true;
        }

        public async Task<QuestPlayerData> GetQuestDataOfCharacter(int characterId)
        {
            MySqlCommand command = new MySqlCommand(@"
                SELECT * FROM `player_quest_data`
                WHERE `character_id`=@characterId
                LIMIT 1;
            ");

            command.Parameters.AddWithValue("@characterId", characterId);
            DataTable result = await ENet.Database.ExecuteReadAsync(command);
            if (result is null || result.Rows.Count == 0)
                return null;

            DataRow row = result.Rows[0];
            return new QuestPlayerData()
            {
                CharacterId = characterId,
                ActiveQuestLineId = (QuestLineId)Convert.ToInt32(row["quest_line_id"]),
                ActiveQuestTaskIndex = Convert.ToUInt32(row["quest_task_index"]),
                Progress = JsonConvert.DeserializeObject<Dictionary<QuestTaskId, int>>(Convert.ToString(row["quest_progress"]))
            };
        }
    }
}
