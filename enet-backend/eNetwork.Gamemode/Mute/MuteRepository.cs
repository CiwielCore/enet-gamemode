using eNetwork.Demorgan.Models;
using eNetwork.Framework;
using eNetwork.Framework.Singleton;
using eNetwork.Mute.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace eNetwork.Mute
{
    public class MuteRepository : Singleton<MuteRepository>
    {
        private readonly Logger _logger = new Logger("mute-system");

        private readonly object _muteInfoLocker = new object();
        private readonly List<MuteInfo> _muteInfos = new List<MuteInfo>();

        private MuteRepository() { }

        public void Initialization()
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM `mute_infos`");
            DataTable result = ENet.Database.ExecuteRead(command);

            if (result is null || result.Rows.Count < 1)
            {
                _logger.WriteInfo("Mute_infos table is empty");
                return;
            }

            foreach (DataRow row in result.Rows)
            {
                int playerUUID = Convert.ToInt32(row[0]);
                int adminUUID = Convert.ToInt32(row[1]);
                uint minutesLeft = Convert.ToUInt32(row[2]);
                string reason = Convert.ToString(row[3]);

                _muteInfos.Add(new MuteInfo()
                {
                    PlayerUUID = playerUUID,
                    AdminUUID = adminUUID,
                    MinutesLeft = minutesLeft,
                    Reason = reason
                });
            }
        }

        public MuteInfo AddMuteInfo(int playerUUID, int adminUUID, uint minutesLeft, string reason)
        {
            MuteInfo info = new MuteInfo()
            {
                PlayerUUID = playerUUID,
                AdminUUID = adminUUID,
                MinutesLeft = minutesLeft,
                Reason = reason
            };

            lock (_muteInfos)
            {
                _muteInfos.Add(info);
            }

            CreateMuteInfoRecordInDatabase(info);

            return info;
        }

        public void RemoveMuteInfo(int playerUUID)
        {
            lock( _muteInfoLocker)
            {
                MuteInfo info = _muteInfos.SingleOrDefault(i => i.PlayerUUID == playerUUID);
                _muteInfos.Remove(info);
                DeleteRecordMuteInfoDatabase(playerUUID);
            }
        } 

        private void CreateMuteInfoRecordInDatabase(MuteInfo info)
        {
            MySqlCommand command = new MySqlCommand("INSERT INTO `mute_infos` " +
                "(playerUUID, adminUUID, minutesLeft, reason) " +
                "VALUES (@playerUUID, @adminUUID, @minutes, @reason);");

            command.Parameters.AddWithValue("@playerUUID", info.PlayerUUID);
            command.Parameters.AddWithValue("@adminUUID", info.AdminUUID);
            command.Parameters.AddWithValue("@minutes", info.MinutesLeft);
            command.Parameters.AddWithValue("@reason", info.Reason);

            Task.Run(async () =>
            {
                await ENet.Database.ExecuteAsync(command);
            });
        }

        private void DeleteRecordMuteInfoDatabase(int playerUUID)
        {
            MySqlCommand command = new MySqlCommand("DELETE FROM `mute_infos` WHERE `playerUUID` = @playerUUID;");
            command.Parameters.AddWithValue("@playerUUID", playerUUID);

            Task.Run(async () =>
            {
                await ENet.Database.ExecuteAsync(command);
            });
        }

        public MuteInfo GetMuteInfo(int playerUUID)
        {
            lock (_muteInfoLocker)
            {
                return _muteInfos.FirstOrDefault(i => i.PlayerUUID == playerUUID);
            }
        }

        public bool IsCharacterInMute(int playerUUID)
        {
            lock (_muteInfoLocker)
            {
                return _muteInfos.Exists(i => i.PlayerUUID == playerUUID);
            }
        }
    }
}
