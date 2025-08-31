using System;
using System.Data;
using System.Collections.Generic;
using MySqlConnector;
using eNetwork.Demorgan.Models;
using eNetwork.Framework;
using eNetwork.Framework.Singleton;
using eNetwork.Framework.Utils;
using eNetwork.Framework.Extensions;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace eNetwork.Demorgan
{
    class DemorganRepository : Singleton<DemorganRepository>
    {
        private readonly Logger _logger = new Logger("demorgan-system");

        private readonly object _demorganInfoLocker = new object();
        private readonly List<DemorganInfo> _demorganInfos = new List<DemorganInfo>();

        private DemorganRepository() { }

        public void OnResourceStart()
        {
            LoadDemorganInfos();
        }

        public DemorganInfo AddDemorganInfo(int characterId, int adminCharacterId, uint minutes, string reason)
        {
            DemorganInfo info = new DemorganInfo()
            {
                CharacterId = characterId,
                AdminCharacterId = adminCharacterId,
                MinutesLeft = minutes,
                Reason = reason
            };

            lock (_demorganInfoLocker)
            {
                _demorganInfos.Add(info);
            }

            CreateDemorganInfoRecordInDatabase(info);

            return info;
        }

        public void RemoveDemorganInfo(int characterId)
        {
            lock (_demorganInfoLocker)
            {
                DemorganInfo info = _demorganInfos.SingleOrDefault(i => i.CharacterId == characterId);
                _demorganInfos.Remove(info);
                DeleteRecordDemorganInfoInDatabase(characterId);
            }
        }

        public bool IsCharacterInDemorgan(int characterId)
        {
            lock (_demorganInfoLocker)
            {
                return _demorganInfos.Exists(i => i.CharacterId == characterId);
            }
        }

        public DemorganInfo GetDemorganInfo(int characterId)
        {
            lock (_demorganInfoLocker)
            {
                return _demorganInfos.FirstOrDefault(i => i.CharacterId == characterId);
            }
        }

        private void LoadDemorganInfos()
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM `demorgan_infos`");
            DataTable result = ENet.Database.ExecuteRead(command);

            if (result is null || result.Rows.Count < 1)
            {
                _logger.WriteInfo("Demorgan_infos table is empty");
                return;
            }

            foreach (DataRow row in result.Rows)
            {
                int characterId = Convert.ToInt32(row[0]);
                int adminCharacterId = Convert.ToInt32(row[1]);
                uint minutesLeft = Convert.ToUInt32(row[2]);
                string reason = Convert.ToString(row[3]);

                _demorganInfos.Add(new DemorganInfo()
                {
                    CharacterId = characterId,
                    AdminCharacterId = adminCharacterId,
                    MinutesLeft = minutesLeft,
                    Reason = reason
                });
            }
        }

        public async Task SaveRecordsInDatabase()
        {
            if (_demorganInfos.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            lock (_demorganInfoLocker)
            {
                foreach (DemorganInfo info in _demorganInfos)
                {
                    sb.Append(@$"
                        UPDATE `demorgan_infos`
                        SET `minutes_left`={info.MinutesLeft}
                        WHERE `character_id`={info.CharacterId};
                    ");
                }
            }

            MySqlCommand command = new MySqlCommand(sb.ToString());
            await ENet.Database.ExecuteAsync(command);
        }

        private void CreateDemorganInfoRecordInDatabase(DemorganInfo info)
        {
            MySqlCommand command = new MySqlCommand("INSERT INTO `demorgan_infos` " +
                "(character_id, admin_character_id, minutes_left, reason) " +
                "VALUES (@charId, @adminId, @minutes, @reason);");

            command.Parameters.AddWithValue("@charId", info.CharacterId);
            command.Parameters.AddWithValue("@adminId", info.AdminCharacterId);
            command.Parameters.AddWithValue("@minutes", info.MinutesLeft);
            command.Parameters.AddWithValue("@reason", info.Reason);

            Task.Run(async () =>
            {
                await ENet.Database.ExecuteAsync(command);
            });
        }

        private void DeleteRecordDemorganInfoInDatabase(int characterId)
        {
            MySqlCommand command = new MySqlCommand("DELETE FROM `demorgan_infos` WHERE `character_id` = @charId;");
            command.Parameters.AddWithValue("@charId", characterId);

            Task.Run(async () =>
            {
                await ENet.Database.ExecuteAsync(command);
            });
        }
    }
}
