using eNetwork.Framework;
using eNetwork.Game.Accounts.Methods;
using eNetwork.Game.Core.BansCharacter;
using eNetwork.Game.Core.BansCharacter.Models;
using GTANetworkAPI;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eNetwork.Game.Accounts
{
    public class AccountManager
    {
        private static readonly Logger Logger = new Logger("enet-authorization");
        public static readonly string DBName = "accounts";

        private static ConcurrentDictionary<string, AccountData> _accounts = new ConcurrentDictionary<string, AccountData>();
        private static ConcurrentDictionary<string, string> _socialClubs = new ConcurrentDictionary<string, string>();

        public static void Initialize()
        {
            DataTable data = ENet.Database.ExecuteRead($"SELECT * from `{DBName}`");
            if (data != null && data.Rows.Count != 0)
            {
                foreach (DataRow row in data.Rows)
                {
                    AccountData account = new AccountData()
                    {
                        Login = Convert.ToString(row["login"]),
                        Password = Convert.ToString(row["password"]),
                        IP = Convert.ToString(row["ip"]),
                        HWID = Convert.ToString(row["hwid"]),
                        EMail = Convert.ToString(row["email"]),
                        Characters = JsonConvert.DeserializeObject<List<int>>(row["characters"].ToString()),
                        SocialClub = Convert.ToString(row["socialclub"]),
                        SocialId = Convert.ToUInt64(row["socialid"]),
                        BonusPoints = Convert.ToInt64(row["bonuspoints"]),
                        DonatePoints = Convert.ToInt64(row["donatepoints"]),
                        LastLogin = (DateTime)row["lastlogin"],
                        Promocode = Convert.ToString(row["promocode"])
                    };

                    CreateAccount(account);
                }
            }

            Logger.WriteInfo($"Загружено {_accounts.Count} аккаунтов");
        }

        public static AccountData GetAccountByUUID(int characterId)
        {
            List<AccountData> accounts = _accounts.Values.ToList();
            return accounts.FirstOrDefault(a => a.Characters.Contains(characterId));
        }

        public static AccountData GetAccount(string accountName)
        {
            if (_accounts.TryGetValue(accountName, out var data)) return data;
            return null;
        }

        private static void CreateAccount(AccountData account)
        {
            _accounts.TryAdd(account.Login, account);
            _socialClubs.TryAdd(account.SocialClub, account.Login); ;
        }

        public static async Task Save(ENetPlayer player)
        {
            try
            {
                if (!player.GetAccountData(out AccountData accountData)) return;

                MySqlCommand sqlCommand = new MySqlCommand($"UPDATE `{DBName}` SET `password`=@PASSWORD, `ip`=@IP, `hwid`=@HWID, `email`=@EMAIL, `characters`=@CHARACTERS, `socialclub`=@SOCIAL, `socialid`=@SOCIALID," +
                    $"`bonuspoints`=@BONUS, `donatepoints`=@DONATE, `lastlogin`=@LASTLOGIN, `promocode`=@PROMO WHERE `login`=@LOGIN");

                sqlCommand.Parameters.AddWithValue("@LOGIN", accountData.Login);
                sqlCommand.Parameters.AddWithValue("@PASSWORD", accountData.Password);
                sqlCommand.Parameters.AddWithValue("@EMAIL", accountData.EMail);
                sqlCommand.Parameters.AddWithValue("@HWID", accountData.HWID);
                sqlCommand.Parameters.AddWithValue("@IP", accountData.IP);
                sqlCommand.Parameters.AddWithValue("@CHARACTERS", JsonConvert.SerializeObject(accountData.Characters));
                sqlCommand.Parameters.AddWithValue("@BONUS", accountData.BonusPoints);
                sqlCommand.Parameters.AddWithValue("@DONATE", accountData.DonatePoints);
                sqlCommand.Parameters.AddWithValue("@SOCIAL", accountData.SocialClub);
                sqlCommand.Parameters.AddWithValue("@SOCIALID", accountData.SocialId);
                sqlCommand.Parameters.AddWithValue("@LASTLOGIN", Helper.ConvertTime(accountData.LastLogin));
                sqlCommand.Parameters.AddWithValue("@PROMO", accountData.Promocode);

                await ENet.Database.ExecuteAsync(sqlCommand);
            }
            catch (Exception e) { Logger.WriteError("Save", e); }
        }

        [CustomEvent("server.authorization.user.check")]
        private void OnEventChecking(ENetPlayer player)
        {
            try
            {
                if (player.IsAccountData()) return;

                ClientEvent.Event(player, "client.authorization.close.intro");
                if (_socialClubs.TryGetValue(player.UserName, out string _login) && _accounts.TryGetValue(_login, out AccountData account))
                {
                    account.Load(player);
                    return;
                }

                ClientEvent.Event(player, "client.authorization.open");
            }
            catch(Exception ex) { Logger.WriteError("OnEventChecking", ex); }
        }

        [CustomEvent("server.authorization.account.leave")]
        private void OnEvent_ExitAccount(ENetPlayer player)
        {
            try
            {
                if (!player.IsTimeouted("on.exitAccount", 1) || player.GetCharacter() != null) return;
                
                if (!player.GetAccountData(out var accountData))
                {
                    player.ExtKick("Выход из несуществующего аккаунта");
                    return;
                }

                player.SetAccountData(null);
                ClientEvent.Event(player, "client.authorization.characters.exit");
            }
            catch(Exception ex) { Logger.WriteError("OnEvent_ExitAccount", ex); }
        }

        [CustomEvent("server.authorization.login")]
        private void OnEventLogin(ENetPlayer player, string login, string pass)
        {
            try
            {
                if (!player.IsTimeouted("loginEvent", 1)) return;

                if (player.IsAccountData())
                {
                    player.SendError(Language.GetText(TextType.SocialRegister));
                    player.Kick("EAC Anticheat 3.1");
                    return;
                }

                var account = GetAccount(login);
                if (account is null)
                {
                    player.SendError(Language.GetText(TextType.AccountNotFound, login));
                    return;
                }

                if (account.SocialClub != player.UserName)
                {
                    player.SendError("Данный аккаунт был зарегестрирован на другой SocialClub!");
                    return;
                }

                if (!BcryptPasswordVerify(pass, account.Password))
                {
                    player.SendError(Language.GetText(TextType.WrongPassword, 3, "Попытки"));

                    ClientEvent.Event(player, "client.authorization.inputStatus", "auth", "login", "done");
                    ClientEvent.Event(player, "client.authorization.inputStatus", "auth", "password", "error");
                }

                if (account.IsLogined || account.Player != null)
                {
                    player.SendInfo($"Кто-то уже играет под вашим аккаунтом!");
                    return;
                }

                account.Load(player);
            }
            catch(Exception ex) { Logger.WriteError("OnEventLogin", ex); }
        }

        [CustomEvent("server.authorization.register")]
        private async void OnEventRegister(ENetPlayer player, string login, string password, string confirmPass)
        {
            try
            {
                if (!player.IsTimeouted("registerEvent", 1) || player.IsAccountData()) return;

                if (login.Replace(" ", "").Length < 3 || !Regex.IsMatch(login, "[a-zA-Z1-9]"))
                {
                    player.SendError(Language.GetText(TextType.FormatLogin));
                    ClientEvent.Event(player, "client.authorization.inputStatus", "reg", "login", "error");
                    return;
                }

                if (password.Replace(" ", "").Length < 3 || !Regex.IsMatch(password, "[a-zA-Z1-9]"))
                {
                    player.SendError(Language.GetText(TextType.FormatPassword));

                    ClientEvent.Event(player, "client.authorization.inputStatus", "reg", "login", "done");
                    ClientEvent.Event(player, "client.authorization.inputStatus", "reg", "password", "error");
                    return;
                }

                if (_accounts.ContainsKey(login))
                {
                    player.SendError(Language.GetText(TextType.LoginRegister));

                    ClientEvent.Event(player, "client.authorization.inputStatus", "reg", "login", "error");
                    return;
                }
                if (_socialClubs.ContainsKey(player.UserName))
                {
                    player.SendError(Language.GetText(TextType.SocialRegister));
                    return;
                }
                if (password != confirmPass)
                {
                    player.SendError(Language.GetText(TextType.CheckWrongPassword));

                    ClientEvent.Event(player, "client.authorization.inputStatus", "reg", "login", "done");
                    ClientEvent.Event(player, "client.authorization.inputStatus", "reg", "password", "error");
                    ClientEvent.Event(player, "client.authorization.inputStatus", "reg", "confirmPassword", "error");
                    return;
                }

                var data = new AccountData()
                {
                    Login = login,
                    IP = player.IP,
                    HWID = player.HardwareID,
                    SocialClub = player.UserName,
                    SocialId = player.UserId,
                    LastLogin = DateTime.Now,
                    Password = BcryptPasswordHash(password),
                };

                MySqlCommand sqlCommand = new MySqlCommand($@"INSERT INTO `{DBName}` (`login`,`password`,`email`,`ip`,`hwid`,`socialclub`,`socialid`,`lastlogin`,`characters`,`donatepoints`,`bonuspoints`,`promocode`)"
                    + "VALUES (@LOGIN, @PASSWORD, @EMAIL, @IP, @HWID, @SOCIAL, @SOCIALID, @LASTLOGIN, @CHARACTERS, @DONATE, @BONUS, @PROMOCODE)");

                sqlCommand.Parameters.AddWithValue("@LOGIN", data.Login);
                sqlCommand.Parameters.AddWithValue("@PASSWORD", data.Password);
                sqlCommand.Parameters.AddWithValue("@EMAIL", data.EMail);
                sqlCommand.Parameters.AddWithValue("@HWID", data.HWID);
                sqlCommand.Parameters.AddWithValue("@IP", data.IP);
                sqlCommand.Parameters.AddWithValue("@CHARACTERS", JsonConvert.SerializeObject(data.Characters));
                sqlCommand.Parameters.AddWithValue("@BONUS", data.BonusPoints);
                sqlCommand.Parameters.AddWithValue("@DONATE", data.DonatePoints);
                sqlCommand.Parameters.AddWithValue("@SOCIAL", data.SocialClub);
                sqlCommand.Parameters.AddWithValue("@SOCIALID", data.SocialId);
                sqlCommand.Parameters.AddWithValue("@LASTLOGIN", Helper.ConvertTime(data.LastLogin));
                sqlCommand.Parameters.AddWithValue("@PROMOCODE", data.Promocode);

                await ENet.Database.ExecuteAsync(sqlCommand);

                CreateAccount(data);

                await NAPI.Task.WaitForMainThread();
                data.Load(player);
            }
            catch (Exception ex) { Logger.WriteError("OnEventRegister", ex); }
        }

        public static string BcryptPasswordHash(string data)
        {
            return BCrypt.Net.BCrypt.HashPassword(data);
        }

        public static bool BcryptPasswordVerify(string enterPassword, string userPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enterPassword, userPassword);
        }
    }
}
