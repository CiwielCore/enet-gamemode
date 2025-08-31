using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using MySqlConnector;
using eNetwork.Framework.Singleton;
using eNetwork.Framework.Extensions;
using eNetwork.Game.Core.BansCharacter.Models;
using eNetwork.Framework;
using eNetwork.Framework.Utils;

namespace eNetwork.Game.Core.BansCharacter
{
    class BanHandler : Singleton<BanHandler>
    {
        private readonly Logger _logger = new Logger("bans-handler");
        private readonly List<AccountBanData> _accountBans;
        private readonly List<CharacterBanData> _characterBans;
        private readonly object _locker;

        private BanHandler()
        {
            _accountBans = new List<AccountBanData>();
            _characterBans = new List<CharacterBanData>();
            _locker = new object();
        }

        public void OnResourceStart()
        {
            ClearExpiredBans();
        }

        private void ClearExpiredBans()
        {
            _logger.WriteInfo("Start removing expired bans...");
            Task clearAccountBansTask = ClearExpiredAccountBans();
            Task clearCharacterBansTask = ClearExpiredCharacterBans();
            
            Task.WhenAll(clearAccountBansTask, clearCharacterBansTask)
                .ContinueWith(
                (t) =>
                {
                    _logger.WriteInfo("Загружаем активные баны...");
                    LoadAccountBans();
                    LoadCharacterBans();
                })
                .ContinueWith(
                (t) =>
                {
                    _logger.WriteInfo($"Загружены активные баны. [AccountBans: {_accountBans.Count} | CharacterBans: {_characterBans.Count}]");
                    TimeSpan nowTime = DateTime.Now.TimeOfDay;
                    TimeSpan updateTime = new TimeSpan(24, 0, 0);
                    TimeSpan nextTime = updateTime.Subtract(nowTime);
                    Timers.StartOnce(Convert.ToInt32(nextTime.TotalMilliseconds), ClearExpiredBans);
                });
        }

        #region Account Bans

        public AccountBanData GetBanAccount(string accountLogin)
        {
            lock (_locker)
            {
                return Validate(_accountBans.Find(b => b.AccountLogin == accountLogin));
            }
        }

        public AccountBanData GetBanAccount(ulong socialClubId)
        {
            lock (_locker)
            {
                return Validate(_accountBans.Find(b => b.SocialClubId == socialClubId));
            }
        }

        public AccountBanData GetBanAccountByHwid(string hardwareId)
        {
            lock (_locker)
            {
                return Validate(_accountBans.Find(b => b.HardwareId == hardwareId));
            }
        }

        public AccountBanData GetBanAccountByIp(string ipAddress)
        {
            lock (_locker)
            {
                return Validate(_accountBans.Find(b => b.IpAddress == ipAddress));
            }
        }

        public void AddAccountBan(AccountBanData banData)
        {
            lock (_locker)
            {
                _accountBans.Add(banData);
            }

            Task.Run(async () => await CreateAccountBanDataToDB(banData));
        }

        public void RemoveBan(AccountBanData ban)
        {
            lock (_locker)
            {
                _accountBans.Remove(ban);
            }

            Task.Run(async () => await RemoveAccountBanDataInDB(ban));
        }

        private AccountBanData Validate(AccountBanData banData)
        {
            if (banData == null)
                return null;

            if (banData.Ended < DateTime.UtcNow)
            {
                RemoveBan(banData);
                return null;
            }

            return banData;
        }

        private void LoadAccountBans()
        {
            lock (_locker)
            {
                _accountBans.Clear();
                _accountBans.Capacity = 0;

                DataTable result = ENet.Database.ExecuteRead("SELECT * FROM `account_bans`");
                if (result == null || result.Rows.Count == 0)
                    return;

                foreach (DataRow row in result.Rows)
                {
                    _accountBans.Add(new AccountBanData()
                    {
                        AccountLogin = Convert.ToString(row["account_login"]),
                        SocialClubId = Convert.ToUInt64(row["socialclub_id"]),
                        HardwareId = Convert.ToString(row["hardware_id"]),
                        IpAddress = Convert.ToString(row["ip_address"]),
                        Admin = Convert.ToString(row["admin"]),
                        Reason = Convert.ToString(row["reason"]),
                        Time = TimeUtils.UnixTimeToDateTime(Convert.ToInt64(row["date_issue"])),
                        Ended = TimeUtils.UnixTimeToDateTime(Convert.ToInt64(row["date_ended"])),
                    });
                }
            }
        }

        private Task ClearExpiredAccountBans()
        {
            MySqlCommand command = new MySqlCommand(@"
                DELETE FROM `account_bans`
                WHERE `date_ended` <= UNIX_TIMESTAMP();
            ");

            return ENet.Database.ExecuteAsync(command);
        }

        private Task CreateAccountBanDataToDB(AccountBanData ban)
        {
            MySqlCommand command = new MySqlCommand(@"
                INSERT INTO `account_bans`
                (`account_login`, `socialclub_id`, `hardware_id`, `ip_address`, `admin`, `reason`, `date_issue`, `date_ended`)
                VALUES (@accountLogin, @socialClubId, @hardwareId, @ipAddress, @admin, @reason, @dateIssue, @dateEnded);
            ");

            command.Parameters.AddWithValue("@accountLogin", ban.AccountLogin);
            command.Parameters.AddWithValue("@socialClubId", ban.SocialClubId);
            command.Parameters.AddWithValue("@hardwareId", ban.HardwareId);
            command.Parameters.AddWithValue("@ipAddress", ban.IpAddress);
            command.Parameters.AddWithValue("@admin", ban.Admin);
            command.Parameters.AddWithValue("@reason", ban.Reason);
            command.Parameters.AddWithValue("@dateIssue", ban.Time.ToUnix());
            command.Parameters.AddWithValue("@dateEnded", ban.Ended.ToUnix());

            return ENet.Database.ExecuteAsync(command);
        }

        private Task RemoveAccountBanDataInDB(AccountBanData ban)
        {
            MySqlCommand command = new MySqlCommand(@"
                DELETE FROM `account_bans`
                WHERE `account_login`=@accountLogin;
            ");

            command.Parameters.AddWithValue("@accountLogin", ban.AccountLogin);

            return ENet.Database.ExecuteAsync(command);
        }
        #endregion

        #region Character Ban
        
        public CharacterBanData GetBanCharacter(int uuid)
        {
            lock (_locker)
            {
                return Validate(_characterBans.Find(b => b.UUID == uuid));
            }
        }

        public void AddCharacterBan(CharacterBanData banData)
        {
            lock (_locker)
            {
                _characterBans.Add(banData);
            }

            Task.Run(async () => await CreateCharacterBanDataToDB(banData));
        }

        public void RemoveBan(CharacterBanData ban)
        {
            lock (_locker)
            {
                _characterBans.Remove(ban);
            }

            Task.Run(async () => await RemoveCharacterBanDataInDB(ban));
        }

        private CharacterBanData Validate(CharacterBanData banData)
        {
            if (banData == null)
                return null;

            if (banData.Ended < DateTime.UtcNow)
            {
                RemoveBan(banData);
                return null;
            }

            return banData;
        }

        private void LoadCharacterBans()
        {
            lock (_locker)
            {
                _characterBans.Clear();
                _characterBans.Capacity = 0;

                DataTable result = ENet.Database.ExecuteRead("SELECT * FROM `character_bans`");
                if (result == null || result.Rows.Count == 0)
                    return;

                foreach (DataRow row in result.Rows)
                {
                    _characterBans.Add(new CharacterBanData()
                    {
                        UUID = Convert.ToInt32(row["character_id"]),
                        Admin = Convert.ToString(row["admin"]),
                        Reason = Convert.ToString(row["reason"]),
                        Time = TimeUtils.UnixTimeToDateTime(Convert.ToInt64(row["date_issue"])),
                        Ended = TimeUtils.UnixTimeToDateTime(Convert.ToInt64(row["date_ended"])),
                    });
                }
            }
        }

        private Task ClearExpiredCharacterBans()
        {
            MySqlCommand command = new MySqlCommand(@"
                DELETE FROM `character_bans`
                WHERE `date_ended` <= UNIX_TIMESTAMP();
            ");

            return ENet.Database.ExecuteAsync(command);
        }

        private Task CreateCharacterBanDataToDB(CharacterBanData ban)
        {
            MySqlCommand command = new MySqlCommand(@"
                INSERT INTO `character_bans`
                (`character_id`, `admin`, `reason`, `date_issue`, `date_ended`)
                VALUES (@uuid, @admin, @reason, @dateIssue, @dateEnded);
            ");

            command.Parameters.AddWithValue("@uuid", ban.UUID);
            command.Parameters.AddWithValue("@admin", ban.Admin);
            command.Parameters.AddWithValue("@reason", ban.Reason);
            command.Parameters.AddWithValue("@dateIssue", ban.Time.ToUnix());
            command.Parameters.AddWithValue("@dateEnded", ban.Ended.ToUnix());

            return ENet.Database.ExecuteAsync(command);
        }

        private Task RemoveCharacterBanDataInDB(CharacterBanData ban)
        {
            MySqlCommand command = new MySqlCommand(@"
                DELETE FROM `character_bans`
                WHERE `character_id`=@uuid;
            ");

            command.Parameters.AddWithValue("@uuid", ban.UUID);

            return ENet.Database.ExecuteAsync(command);
        }

        #endregion
    }
}
