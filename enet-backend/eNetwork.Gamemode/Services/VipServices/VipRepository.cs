using eNetwork.Framework.Extensions;
using eNetwork.Framework.Utils;
using MySqlConnector;
using System;
using System.Data;
using System.Threading.Tasks;

namespace eNetwork.Services.VipServices
{
    public class VipRepository
    {
        public Task CreateVipStatusInDB(VipStatus vip)
        {
            MySqlCommand command = new MySqlCommand(@"
                INSERT INTO `vip_statuses`
                (`character_id`, `status_name`, `date_of_issue`, `date_of_end`)
                VALUES (@characterId, @vipName, @dateOfIssue, @dateOfEnd);
            ");

            command.Parameters.AddWithValue("@characterId", vip.CharacterId);
            command.Parameters.AddWithValue("@vipName", vip.VipName);
            command.Parameters.AddWithValue("@dateOfIssue", vip.DateOfIssue.ToUnix());
            command.Parameters.AddWithValue("@dateOfEnd", vip.DateOfEnd.ToUnix());

            return ENet.Database.ExecuteAsync(command);
        }

        public Task DeleteVipStatusForCharacter(int characterId)
        {
            MySqlCommand command = new MySqlCommand(@"
                DELETE FROM `vip_statuses`
                WHERE `character_id`=@characterId;
            ");

            command.Parameters.AddWithValue("@characterId", characterId);

            return ENet.Database.ExecuteAsync(command);
        }

        public Task ClearExpiredVipStatuses()
        {
            MySqlCommand command = new MySqlCommand(@"
                DELETE FROM `vip_statuses`
                WHERE `date_of_end` <= UNIX_TIMESTAMP();
            ");

            return ENet.Database.ExecuteAsync(command);
        }

        public async Task<VipStatus> GetVipStatusByCharacterId(int characterId)
        {
            MySqlCommand command = new MySqlCommand(@"
                SELECT * FROM `vip_statuses`
                WHERE `character_id`=@characterId
                LIMIT 1;
            ");

            command.Parameters.AddWithValue("@characterId", characterId);
            DataTable result = await ENet.Database.ExecuteReadAsync(command);
            if (result is null || result.Rows.Count == 0)
                return null;

            DataRow row = result.Rows[0];
            string vipName = Convert.ToString(row["status_name"]);
            DateTime dateOfIssue = TimeUtils.UnixTimeToDateTime(Convert.ToInt64(row["date_of_issue"]));
            DateTime dateOfEnd = TimeUtils.UnixTimeToDateTime(Convert.ToInt64(row["date_of_end"]));

            return new VipStatus()
            {
                CharacterId = characterId,
                VipName = vipName,
                DateOfIssue = dateOfIssue,
                DateOfEnd = dateOfEnd,
            };
        }
    }
}