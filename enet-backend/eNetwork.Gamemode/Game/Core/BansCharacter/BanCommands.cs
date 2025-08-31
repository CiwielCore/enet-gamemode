using eNetwork.External;
using eNetwork.Framework;
using eNetwork.Game.Accounts;
using eNetwork.Game.Core.BansCharacter.Models;
using System;

namespace eNetwork.Game.Core.BansCharacter
{
    class BanCommands
    {
        [ChatCommand("ban", Description = "Выдать бан персонажу", Access = PlayerRank.Admin, Arguments = "[uuid] [days] [reason]", GreedyArg = true)]
        public static void Command_Ban(ENetPlayer admin, int uuid, int days, string reason)
        {

            if (admin is null)
                return;

            if (BanHandler.Instance.GetBanCharacter(uuid) != null)
            {
                admin.SendWarning("Данный персонаж уже находится в бане");
                return;
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                admin.SendWarning("Укажите причину бана");
                return;
            }

            CharacterBanData banData = new CharacterBanData()
            {
                UUID = uuid,
                Admin = admin.Name,
                Reason = reason,
                Time = DateTime.UtcNow,
                Ended = DateTime.UtcNow.AddDays(days)
            };
            BanHandler.Instance.AddCharacterBan(banData);

            admin.SendInfo($"Вы выдали бан по статику {uuid} сроком на {days} дней");

            ENetPlayer bannedPlayer = ENet.Pools.GetPlayerByUUID(uuid);
            if (bannedPlayer is null)
                return;

            // Discord.SendMessage("log", $"Администратор {banData.Admin} #{admin.CharacterData.UUID} выдал бан {uuid}, по причине: {banData.Reason}");

            bannedPlayer.ExtKick($"Администратор {banData.Admin} выдал вам бан до {banData.Ended:g}, по причине: \"{banData.Reason}\"");
        }

        [ChatCommand("sban", Description = "Выдать бан персонажу", Access = PlayerRank.Admin, Arguments = "[uuid] [days] [reason]", GreedyArg = true)]
        public static void Command_SBan(ENetPlayer admin, int uuid, int days, string reason)
        {

            if (admin is null)
                return;

            if (BanHandler.Instance.GetBanCharacter(uuid) != null)
            {
                admin.SendWarning("Данный персонаж уже находится в бане");
                return;
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                admin.SendWarning("Укажите причину бана");
                return;
            }

            CharacterBanData banData = new CharacterBanData()
            {
                UUID = uuid,
                Admin = admin.Name,
                Reason = reason,
                Time = DateTime.UtcNow,
                Ended = DateTime.UtcNow.AddDays(days)
            };
            BanHandler.Instance.AddCharacterBan(banData);

            admin.SendInfo($"Вы выдали бан по статику {uuid} сроком на {days} дней");

            ENetPlayer bannedPlayer = ENet.Pools.GetPlayerByUUID(uuid);
            if (bannedPlayer is null)
                return;

            // Discord.SendMessage("log", $"Администратор {banData.Admin} #{admin.CharacterData.UUID} выдал бан {uuid}, по причине: {banData.Reason}");

            bannedPlayer.ExtKick($"Администратор {banData.Admin} выдал вам бан до {banData.Ended:g}, по причине: \"{banData.Reason}\"");
        }

        [ChatCommand("unban", Description = "Снять бан с персонажа", Access = PlayerRank.Admin, Arguments = "[uuid] [reason]", GreedyArg = true)]
        public static void Command_Unban(ENetPlayer admin, int uuid, string reason)
        {
            CharacterBanData banData = BanHandler.Instance.GetBanCharacter(uuid);
            if (banData is null)
            {
                admin.SendWarning("Данный персонаж не забанен");
                return;
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                admin.SendWarning("Укажите причину разбана");
                return;
            }

            BanHandler.Instance.RemoveBan(banData);
            // Discord.SendMessage("log", $"Администратор {banData.Admin} #{admin.CharacterData.UUID} снял бан {uuid}");
            admin.SendInfo($"Вы сняли бан с персонажа {uuid}");
            // TODO: log unban with reason
        }

        [ChatCommand("hardban", Description = "Выдать хард-бан на аккаунт", Access = PlayerRank.SeniorAdmin, Arguments = "[uuid] [days] [reason]")]
        public static void Command_HardBan(ENetPlayer admin, int uuid, int days, string reason)
        {
            AccountData account = AccountManager.GetAccountByUUID(uuid);
            if (account is null)
            {
                admin.SendWarning("Аккаунт для данного персонажа не найден");
                return;
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                admin.SendWarning("Укажите причину бана");
                return;
            }

            ENetPlayer player = ENet.Pools.GetPlayerByUUID(uuid);

            AccountBanData banData = new AccountBanData()
            {
                AccountLogin = account.Login,
                SocialClubId = player is null ? account.SocialId : player.SocialClubId,
                HardwareId = player is null ? account.HWID : player.Serial,
                IpAddress = player is null ? account.IP : player.Address,
                Admin = admin.Name,
                Reason = reason,
                Time = DateTime.UtcNow,
                Ended = DateTime.UtcNow.AddDays(days),
            };

            BanHandler.Instance.AddAccountBan(banData);

            admin.SendInfo($"Вы выдали хард-бан по статику {uuid} сроком на {days} дней");

            if (player is null)
                return;

            // Discord.SendMessage("log", $"Администратор {banData.Admin} #{admin.CharacterData.UUID} выдал хард-бан {uuid}, по причине: {banData.Reason}");
            player.ExtKick($"Администратор {banData.Admin} выдал вам хард-бан до {banData.Ended:g}, по причине: \"{banData.Reason}\"");
        }

        [ChatCommand("unhardban", Description = "Снять хард-бан с аккаунта", Access = PlayerRank.SeniorAdmin, Arguments = "[uuid] [reason]")]
        public static void Command_UnHardBan(ENetPlayer admin, int uuid, string reason)
        {
            AccountData account = AccountManager.GetAccountByUUID(uuid);
            if (account is null)
            {
                admin.SendWarning("Аккаунт для данного персонажа не найден");
                return;
            }

            AccountBanData banData = BanHandler.Instance.GetBanAccount(account.Login);
            if (banData is null)
            {
                admin.SendWarning("На данном аккаунте нет бана");
                return;
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                admin.SendWarning("Укажите причину разбана");
                return;
            }

            BanHandler.Instance.RemoveBan(banData);
            // Discord.SendMessage("log", $"Администратор {banData.Admin} #{admin.CharacterData.UUID} снял хард-бан {uuid}, по причине: \"{banData.Reason}\"");
            admin.SendInfo($"Вы сняли хард-бан с персонажа {uuid}");
            // TODO: log unhardban
        }
    }
}
