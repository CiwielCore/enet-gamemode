using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using eNetwork.Game.Core.Whitelist.Classes;

namespace eNetwork.Game.Core.Whitelist
{
    internal class Manager
    {
        private static readonly Logger Logger = new Logger("whitelist-manager");
        private static readonly string DBName = "whitelist";
        private static List<WhitelistUser> _whiteListUsers = new List<WhitelistUser>();
        public static void Initialize()
        {
            try
            {
                DataTable result = ENet.Database.ExecuteRead($"SELECT * FROM `{DBName}`");
                if (result != null && result.Rows.Count != 0) {
                    foreach (DataRow row in result.Rows)
                    {
                        var data = new WhitelistUser();
                        data.SocialClub = Convert.ToString(row["social"]);

                        _whiteListUsers.Add(data);
                    }
                }

                if (Engine.Config.IsWhitelist)
                    Logger.WriteInfo($"Белый список включен. Загружено {_whiteListUsers.Count} белых игроков");
                else
                    Logger.WriteInfo($"Белый список отключен");
            }
            catch(Exception e) { Logger.WriteError("Initialize", e); }
        }

        public static bool CanJoin(ENetPlayer player)
        {
            if (!Engine.Config.IsWhitelist || (Engine.Config.IsWhitelist && _whiteListUsers.Find(x => x.SocialClub.ToUpper() == player.GetSharedData<string>("user.socialclub").ToUpper()) != null)) return true;
            return false;
        }

        public static bool AddUser(string username)
        {
            try
            {
                if (_whiteListUsers.Find(x => x.SocialClub == username) != null) return false;
                _whiteListUsers.Add(new WhitelistUser() 
                { 
                    SocialClub = username 
                });

                return true;
            }
            catch(Exception e) { Logger.WriteError("AddUser", e); return false; }
        }

        public static bool RemoveUser(string username)
        {
            try
            {
                WhitelistUser user = _whiteListUsers.Find(x => x.SocialClub == username);
                if (user is null) return false;
                _whiteListUsers.Remove(user);

                return true;
            }
            catch(Exception e) { Logger.WriteError("RemoveUser", e); return false; }
        }
    }
}
