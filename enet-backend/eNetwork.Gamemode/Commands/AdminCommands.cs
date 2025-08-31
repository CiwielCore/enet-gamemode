using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GTANetworkAPI;
using Newtonsoft.Json;
using System.Threading;
using eNetwork.Framework;
using eNetwork.Game;
using eNetwork.Businesses;
using MySqlConnector;
using eNetwork.Game.Characters.Customization;
using eNetwork.Framework.Enums;
using eNetwork.Admin;
using eNetwork.Services.Logging;
using eNetwork.Services.Logging.LogMessages;
using eNetwork.External;
using static System.Net.Mime.MediaTypeNames;
using GTANetworkMethods;
using ClientEvent = eNetwork.Framework.ClientEvent;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System.Data;
using Vehicle = GTANetworkAPI.Vehicle;
using eNetwork.Game.Characters;
using System.Threading.Tasks;
using eNetwork.Factions;
using eNetwork.Game.Core.BansCharacter;
using System.Linq;
using eNetwork.Inv;

namespace eNetwork.Commands
{
    public class AdminCommands : Script
    {
        private static readonly Logger Logger = new Logger("admin-commands");

        [ChatCommand("asms", Description = "Отправить игроку сообщение", Access = PlayerRank.Helper, Arguments = "[ID] [Текст]", GreedyArg = true)]
        public static void Command_ASms(ENetPlayer player, int id, string text)
        {

            ENetPlayer target = ENet.Pools.GetPlayerByDynamic(id);

            if (target is null)
            {
                ENet.Chat.SendMessage(player, "Игрок не найден");
                return;
            }

            ENet.Chat.SendMessage(target, $"Администратор {player.Name}: {text}");

            foreach (ENetPlayer targetSort in ENet.Pools.GetAllPlayers())
            {
                if (targetSort.CharacterData.Status != PlayerRank.Player || targetSort.CharacterData.Status != PlayerRank.Media)
                {
                    ENet.Chat.SendMessage(targetSort, $"Администратор {player.Name} игроку {target.Name}: {text}");
                }
            }
        }

        [ChatCommand("flip", Description = "Перевернуть автомобиль на колеса", Access = PlayerRank.Helper, Arguments = "[ID]")]
        public static void Command_Flip(ENetPlayer player, int id)
        {
            try
            {
                ENetVehicle vehicle = ENet.Pools.GetVehicleById(id);

                if (vehicle is null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "Транспорт не найден");
                    return;
                }

                vehicle.Rotation = new Vector3(180.0, 180.0, 180.0);
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("gh", Description = "Телепортировать игрока к себе", Access = PlayerRank.Helper, Arguments = "[ID]")]
        public static void Command_Gh(ENetPlayer player, int id)
        {
            try
            {

                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(id);

                if (target is null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "Игрок не найден");
                    return;
                }

                target.Position = player.Position;
                ENet.Chat.SendMessage(target, $"Вас телепортировал администратор {player.Name}");
                
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("tpveh", Description = "Телепортироваться к транспорту", Access = PlayerRank.Helper, Arguments = "[ID]")]
        public static void Command_TpVeh(ENetPlayer player, int id)
        {
            try
            {

                ENetVehicle vehicle = ENet.Pools.GetVehicleById(id);

                if (vehicle is null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "Транспорт не найден");
                    return;
                }

                player.Position = vehicle.VehicleData.Position.GetVector3();

            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("kick", Description = "Кикнуть игрока с сервера", Access = PlayerRank.Helper, Arguments = "[ID] [Reason]")]
        public static void Command_Kick(ENetPlayer player, int id, string reason)
        {
            ENetPlayer target;

            try
            {
                foreach (ENetPlayer targetSort in ENet.Pools.GetAllPlayers())
                {
                    if (targetSort.Id == id)
                    {
                        target = targetSort;
                        targetSort.Kick(reason);
                    }
                    else
                    {
                        ENet.Chat.SendMessage(player, "Игрок не найден");
                        return;
                    }

                    ENet.Chat.SendMessage(targetSort, $"Администратор {player.Nametag} кикнул игрока {target} по причине {reason}");
                }
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("skick", Description = "Кикнуть игрока с сервера", Access = PlayerRank.Helper, Arguments = "[ID] [Reason]")]
        public static void Command_SKick(ENetPlayer player, int id, string reason)
        {
            try
            {
                foreach (ENetPlayer targetSort in ENet.Pools.GetAllPlayers())
                {
                    if (targetSort.Id == id)
                    {
                        targetSort.Kick(reason);
                    }
                    else
                    {
                        ENet.Chat.SendMessage(player, "Игрок не найден");
                        return;
                    }
                }
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("freeze", Description = "Заморозить игрока", Access = PlayerRank.Helper, Arguments = "[ID]")]
        public static void Command_Freeze(ENetPlayer player, int id)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(id);

                target.Freeze(true);
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("unfreeze", Description = "Разморозить игрока", Access = PlayerRank.Helper, Arguments = "[ID]")]
        public static void Command_UnFreeze(ENetPlayer player, int id)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(id);

                target.Freeze(false);
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("skin", Description = "Выдать скин", Access = PlayerRank.Helper, Arguments = "[ID] [Hash]")]
        public static void Command_Skin(ENetPlayer player, int id, string Hash)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(id);

                target.SetSkin((PedHash)Enum.Parse(typeof(PedHash), Hash));
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("checkmoney", Description = "Проверить количество денег", Access = PlayerRank.Helper, Arguments = "[ID]")]
        public static void Command_CheckMoney(ENetPlayer player, int id)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(id);
                var moneyValue = target.GetSharedData<Single>("player.cash");

                NAPI.Chat.SendChatMessageToPlayer(player, $"Количество денег: {moneyValue}");
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("setjob", Description = "Устроить на работу", Access = PlayerRank.Helper, Arguments = "[ID] [ID Работы]")]
        public static void Command_SetJob(ENetPlayer player, int id, int jobId)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(id);
                target.CharacterData.JobId = (JobId)jobId;
                ENet.Chat.SendMessage(player, "Вы устроили игрока на работу");
                ENet.Chat.SendMessage(target, $"Администратор {player.Name} устроил вас на работу");

            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("tpbiz", Description = "Телепортироваться к бизнесу", Access = PlayerRank.Helper, Arguments = "[ID]")]
        public static void Command_TpBiz(ENetPlayer player, int id)
        {
            try
            {
                string dbName = "business";
                DataTable result = ENet.Database.ExecuteRead($"SELECT * FROM `{dbName}`");

                if (result is null) return;

                foreach (DataRow row in result.Rows)
                {
                    int idRow = Convert.ToInt32(row["id"]);
                    List<Vector3> position = JsonConvert.DeserializeObject<List<Vector3>>(row["position"].ToString());

                    if (idRow == id)
                    {
                        player.Position = position[0];
                    }
                }

            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("leaders", Description = "Получить список лидеров", Access = PlayerRank.Helper)]
        public static void Command_Leaders(ENetPlayer player)
        {
            try
            {
                DataTable resultFactionData = ENet.Database.ExecuteRead($"SELECT * FROM `factions_ranks`");
                List<ENetPlayer> resultForeach = new List<ENetPlayer>();

                foreach (ENetPlayer target in ENet.Pools.GetAllPlayers())
                {
                    if (!target.CharacterData.isLeader)
                        continue;

                    resultForeach.Add(target);
                    ENet.Chat.SendMessage(player, $"{target.Name} [{target.Id}] - {target.CharacterData.Faction}");
                }

                if (resultForeach.Count <= 0)
                {
                    ENet.Chat.SendMessage(player, "Лидеров в сети нет.");
                }
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("tph", Description = "Телепортироваться к дому", Access = PlayerRank.Helper, Arguments = "[ID]")]
        public static void Command_Tph(ENetPlayer player, int id)
        {
            try
            {
                string dbName = "houses";
                DataTable result = ENet.Database.ExecuteRead($"SELECT * FROM `{dbName}`");

                if (result is null) return;

                foreach (DataRow row in result.Rows)
                {
                    int idRow = Convert.ToInt32(row["id"]);
                    Vector3 position = JsonConvert.DeserializeObject<Vector3>(row["position"].ToString());

                    if (idRow == id)
                    {
                        player.Position = new Vector3(position.X, position.Y, position.Z);
                    }
                }

            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("checkip", Description = "Узнать информацию о персонажах", Access = PlayerRank.Helper, Arguments = "[StaticID]")]
        public static void Command_CheckIp(ENetPlayer player, int id)
        {
            try
            {
                List<int> idRow = new List<int>();
                string accountName = "";

                string dbName = "accounts";
                DataTable result = ENet.Database.ExecuteRead($"SELECT * FROM `{dbName}` WHERE characters LIKE `{id}`");

                if (result is null)
                {
                    ENet.Chat.SendMessage(player, "Записей в базе данных не обнаружено");
                }

                foreach (DataRow row in result.Rows)
                {
                    idRow.Add(Convert.ToInt32(row["characters"]));
                    accountName = Convert.ToString(row["login"]);
                }

                ENet.Chat.SendMessage(player, $"Информация о аккаунте: {accountName}");

                for (int i = 0; i < idRow.Count; i++)
                {
                    if (idRow[i] < 0) return;

                    DataTable resultCharactersTable = ENet.Database.ExecuteRead($"SELECT * FROM `characters` WHERE uuid=`{idRow[i]}`");
                    
                    foreach (DataRow row in resultCharactersTable.Rows)
                    {
                        string targetName = Convert.ToString(row["name"]);
                        int valueMoney = Convert.ToInt32(row["cash"]);
                        int factionId = Convert.ToInt32(row["faction"]);
                        int factionRank = Convert.ToInt32(row["factionrank"]);

                        ENet.Chat.SendMessage(player, "#####################");
                        ENet.Chat.SendMessage(player, $"Персонаж #{i}");
                        ENet.Chat.SendMessage(player, $"Никнейм персонажа: {targetName}");
                        ENet.Chat.SendMessage(player, $"Количество денег: {valueMoney}");
                        ENet.Chat.SendMessage(player, $"Фракция: {factionId}");
                        ENet.Chat.SendMessage(player, $"Ранг: {factionRank}");
                    }
                }
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("warn", Description = "Выдать варн", Access = PlayerRank.Helper, Arguments = "[StaticID] [Reason]")]
        public static async Task<int> Command_Warn(ENetPlayer player, int staticId, string reason)
        {
            try
            {
                string targetName = "";
                int warnCount = 0;
                string dbName = "characters";
                DataTable result = ENet.Database.ExecuteRead($"SELECT * FROM `{dbName}` WHERE uuid={staticId}");

                if (result is null) return 0;

                foreach (DataRow row in result.Rows)
                {
                    warnCount = Convert.ToInt32(row["warn_count"]);
                    targetName = Convert.ToString(row["name"]);
                }
                
                if (warnCount == 0 || warnCount == 1)
                {
                    warnCount++;
                    MySqlCommand mySqlCommand = new MySqlCommand($"UPDATE `{dbName}` SET `warn_count`=@WARN_COUNT WHERE `uuid`=@UUID");
                    mySqlCommand.Parameters.AddWithValue("WARN_COUNT", warnCount);
                    mySqlCommand.Parameters.AddWithValue("UUID", player.GetUUID());
                    await ENet.Database.ExecuteAsync(mySqlCommand);

                    player.CharacterData.FactionId = 0;
                    player.CharacterData.FactionRank = 0;
                } 
                else if (warnCount == 2)
                {
                    MySqlCommand mySqlCommand = new MySqlCommand($"UPDATE `{dbName}` SET `warn_count`=@WARN_COUNT WHERE `uuid`=@UUID");
                    mySqlCommand.Parameters.AddWithValue("WARN_COUNT", warnCount = 0);
                    mySqlCommand.Parameters.AddWithValue("UUID", player.GetUUID());
                    await ENet.Database.ExecuteAsync(mySqlCommand);
                    BanCommands.Command_SBan(player, staticId, 20, "warn 3/3");
                }

                foreach (ENetPlayer playerPool in ENet.Pools.GetAllPlayers())
                {
                    NAPI.Task.Run(() => ENet.Chat.SendMessage(playerPool, $"Администратор {player.Name} выдал варн игроку {targetName}"));
                }

                return staticId;
            }
            catch (Exception error)
            {
                ENet.Chat.SendMessage(player, "Exeption: " + error);
                return -1;
            }
        }

        [ChatCommand("unwarn", Description = "Снять варн", Access = PlayerRank.Helper, Arguments = "[StaticID]")]
        public static async Task<int> Command_UnWarn(ENetPlayer player, int staticId)
        {
            try
            {
                int warnCount = 0;
                string dbName = "characters";
                DataTable result = ENet.Database.ExecuteRead($"SELECT * FROM `{dbName}` WHERE uuid={staticId}");

                if (result is null) return 0;

                foreach (DataRow row in result.Rows)
                {
                    warnCount = Convert.ToInt32(row["warn_count"]);
                }

                if (warnCount <= 0) 
                {
                    ENet.Chat.SendMessage(player, "У игрока нет варнов");
                    return -1;
                }
                warnCount = warnCount - 1;
                MySqlCommand mySqlCommand = new MySqlCommand($"UPDATE INTO`{dbName}`(warn_count) VALUES(@WARN_COUNT) WHERE uuid=`@UUID`");
                mySqlCommand.Parameters.AddWithValue("WARN_COUNT", warnCount);
                mySqlCommand.Parameters.AddWithValue("UUID", staticId);
                await ENet.Database.ExecuteAsync(mySqlCommand);

                return staticId;
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
                return -1;
            }
        }

        [ChatCommand("inv", Description = "Включить невидимость", Access = PlayerRank.Helper)]
        public static void Command_Inv(ENetPlayer player)
        {
            try
            {
                bool playerInvisibility = player.GetSharedData<bool>("IS_INVISIBILITY");

                if (playerInvisibility)
                {
                    player.Invisibility(false);
                } 
                else
                {
                    player.Invisibility(true);
                }
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("gm", Description = "Включить бессмертие", Access = PlayerRank.Helper)]
        public static void Command_Gm(ENetPlayer player)
        {
            try
            {
                bool playerInvincible = player.GetSharedData<bool>("IS_GODMODE");

                if (playerInvincible)
                {
                    player.Godmode(false);
                }
                else
                {
                    player.Godmode(true);
                }
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("oguns", Description = "Удалить оружие у игрока", Access = PlayerRank.Helper, Arguments = "[ID]")]
        public static void Command_OGuns(ENetPlayer player, int id)
        {
            try
            {
                ENetPlayer target = ENet.Pools.GetPlayerByDynamic(id);

                if (target is null)
                {
                    ENet.Chat.SendMessage(player, "Игрока нет в сети");
                    return;
                }

                target.RemoveAllWeapons();
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("id", Description = "Найти игрока в сети по нику или ID", Access = PlayerRank.Helper, Arguments = "[ID или Никнейм]")]
        public static void Command_Id(ENetPlayer player, string targetIdOrName)
        {
            try
            {
                List<ENetPlayer> resultForeach = new List<ENetPlayer>();
                ENetPlayer target;
                if (Int32.TryParse(targetIdOrName, out int result))
                {
                    target = ENet.Pools.GetPlayerByDynamic(result);

                    if (target is null)
                        return;

                    resultForeach.Add(target);
                    ENet.Chat.SendMessage(player, $"{target.Name} [{target.Id}]");
                }
                else
                {
                    foreach (ENetPlayer targetPool in ENet.Pools.GetAllPlayers())
                    {
                        if (targetPool.Name.Contains(targetIdOrName, StringComparison.OrdinalIgnoreCase))
                        {
                            resultForeach.Add(targetPool);
                            ENet.Chat.SendMessage(player, $"{targetPool.Name} [{targetPool.Id}]");
                        }
                    }
                }

                if(resultForeach.Count <= 0)
                {
                    ENet.Chat.SendMessage(player, "Совпадений не найдено");
                }
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
        }

        [ChatCommand("tempname", Description = "Поставить временный ник", Access = PlayerRank.Helper, Arguments = "[Временный ник (Nick_Name)]")]
        public static void Command_TempName(ENetPlayer player, string temporaryNickname)
        {
            ENet.Chat.SendMessage(player, $"Вы включили временный никнейм: {temporaryNickname}. Будьте аккуратны, игроки не знают о ваших правах администратора!");
            player.SetSharedData("TEMPORARY_NICKNAME", temporaryNickname);
            player.Name = temporaryNickname;
        }

        [ChatCommand("resettempname", Description = "Вернуть свой никнейм", Access = PlayerRank.Helper)]
        public static void Command_ResetTempName(ENetPlayer player) 
        {
            string dbName = "characters";
            ENet.Chat.SendMessage(player, $"Вы выключили временный никнейм.");
            player.SetSharedData("TEMPORARY_NICKNAME", false);
            player.Name = player.CharacterData.Name;
        }

        [ChatCommand("esp", Description = "Включить/выключить ESP игроков", Access = PlayerRank.Helper)]
        public static void Command_Esp(ENetPlayer player)
        {
            ClientEvent.Event(player, "client.esp.toggle.players");
        }

        [ChatCommand("esp2", Description = "Включить/выключить ESP для транспорта", Access = PlayerRank.Helper)]
        public static void Command_Esp2(ENetPlayer player)
        {
            ClientEvent.Event(player, "client.esp2.toggle.players");
        }

        [ChatCommand("dl", Description = "Включить/выключить ESP для транспорта", Access = PlayerRank.Helper)]
        public static void Command_Dl(ENetPlayer player)
        {
            ClientEvent.Event(player, "client.esp.toggle.vehicles");
        }

        [ChatCommand("admins", Description = "Список администрации в сети", Access = PlayerRank.Helper)]
        public static void Command_Admins(ENetPlayer player)
        {
            var admins = AdminsManager.GetAdmins();
            ENet.Chat.SendMessage(player, $"<p style=\"color: {Helper.DollarColor}\">В сети: {admins.Count} администраторов</p>");
            foreach (var p in admins)
            {
                ENet.Chat.SendMessage(player, $"Name: {p.GetName()} - STATUS: {p.CharacterData.Status} - [{p.Id}] - #{p.CharacterData.UUID}");
            }
        }

        [ChatCommand("a", Description = "Админ чат", Access = PlayerRank.Helper, Arguments = "[Сообщение]", GreedyArg = true)]
        public static void Command_aChat(ENetPlayer player, string message)
        {
            // Discord.SendMessage("chat", $"{player.Name} #{player.CharacterData.UUID} сказал: {message} ");
            AdminsManager.SendChat($"<span style=\"color: #EE204D\">[ADMIN] {player.GetName()} [{player.Id}]: </span>{message}");
        }

        [ChatCommand("hp", Description = "Установить здоровье игроку", Access = PlayerRank.JuniorAdmin, Arguments = "[ид] [хп]")]
        public static void Command_SetHp(ENetPlayer player, int playerId, int hp)
        {
            ENetPlayer target = ENet.Pools.GetPlayerByDynamic(playerId);
            if (target is null)
            {
                player.SendError("Игрок с таким id не найден");
                return;
            }

            LoggingService.Instance.Create(new AdminCommandUsageLogMessage(player.GetUUID(), player.GetName(), target.GetUUID(), target.GetName(), "hp"));

            ENet.Task.SetMainTask(() =>
            {
                NAPI.Player.SetPlayerHealth(target, hp);
            });
        }

        [ChatCommand("save", Arguments = "[имя]", Description = "Сохранить координаты в текстовый файл", Access = PlayerRank.CheifAdmin)]
        public static void Command_SaveCoords(ENetPlayer player, string name)
        {
            Vector3 pos = NAPI.Entity.GetEntityPosition(player);
            pos.Z -= 1.12f;
            Vector3 rot = NAPI.Entity.GetEntityRotation(player);
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                Vehicle vehicle = player.Vehicle;
                pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                rot = NAPI.Entity.GetEntityRotation(vehicle);
            }
            try
            {
                StreamWriter saveCoords = new StreamWriter("./data/coordinates.txt", true, Encoding.UTF8);
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                saveCoords.Write($"\r\n{name}   Позиция: new Vector3({pos.X}, {pos.Y}, {pos.Z}),    JSON: {JsonConvert.SerializeObject(pos)}      \r\n");
                saveCoords.Write($"{name}   Ротация: new Vector3({rot.X}, {rot.Y}, {rot.Z}),     JSON: {JsonConvert.SerializeObject(rot)}    \r\n");
                saveCoords.Write($"========================================================");
                saveCoords.Close();
            }

            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }

            finally
            {
                ENet.Chat.SendMessage(player, $"Координаты {name}: " + NAPI.Entity.GetEntityPosition(player));
            }
        }

        #region Whitelist commands
        [ChatCommand("addwhitelist", Description = "Добавить пользователя в белый список", Access = PlayerRank.Owner, Arguments = "[сошлклаб]")]
        public static void Command_AddToWhitelist(ENetPlayer player, string username)
        {
            try
            {
                bool isAdded = Game.Core.Whitelist.Manager.AddUser(username);
                if (isAdded)
                {
                    player.SendInfo(Language.GetText(TextType.WhitelistAdd, username));
                }
                else
                {
                    player.SendError(Language.GetText(TextType.WhitelistAddError));
                }
            }
            catch (Exception e) { Logger.WriteError("Command_AddToWhitelist", e); }
        }

        [ChatCommand("removewhitelist", Description = "Удалить пользователя из белого списка", Access = PlayerRank.Owner, Arguments = "[сошлклаб]")]
        public static void Command_RemoveFromWhitelist(ENetPlayer player, string username)
        {
            try
            {
                bool isAdded = Game.Core.Whitelist.Manager.RemoveUser(username);
                if (isAdded)
                {
                    player.SendInfo(Language.GetText(TextType.WhitelistRemove, username));
                }
                else
                {
                    player.SendError(Language.GetText(TextType.WhitelistRemoveError, username));
                }
            }
            catch (Exception e) { Logger.WriteError("Command_AddToWhitelist", e); }
        }
        #endregion

        #region Businesses commands 
        [ChatCommand("createbusiness", Description = "Создать точку бизнеса", Access = PlayerRank.Owner, Arguments = "[тип] [цена]")]
        private static void Command_CreateBusiness(ENetPlayer player, string type, int price)
        {
            try
            {
                if (!Enum.IsDefined(typeof(BusinessType), type))
                {
                    player.SendError("Такого типа бизнеса не существует!");
                    return;
                }
                BusinessType businessType = (BusinessType)Enum.Parse(typeof(BusinessType), type);
                var business = BusinessManager.Create(businessType, BusinessManager.GenID(), -1, price, 0, null, new List<Vector3>() { player.Position }, new Position(0, 0, 0, 0), new Vector3(0, 0, 0), 0, 0, 100);
                if (business is null)
                {
                    player.SendError("Не удалось создать бизнес...");
                    return;
                }

                MySqlCommand sqlCommand = new MySqlCommand("INSERT INTO `business` (`id`, `type`, `owner`, `price`, `fraction`, `products`, `position`, `npc`, `posinfo`, `tax`, `earning`, `markup`) " +
                    "VALUES(@ID, @TYPE, @OWNER, @PRICE, @FRACTION, @PROD, @POSITION, @NPC, @POSINFO, @TAX, @EARNING, @MARKUP)");
                sqlCommand.Parameters.AddWithValue("@ID", business.ID);
                sqlCommand.Parameters.AddWithValue("@TYPE", business.Type);
                sqlCommand.Parameters.AddWithValue("@OWNER", business.Owner);
                sqlCommand.Parameters.AddWithValue("@PRICE", business.Price);
                sqlCommand.Parameters.AddWithValue("@FRACTION", business.ControlFraction);
                sqlCommand.Parameters.AddWithValue("@PROD", JsonConvert.SerializeObject(business.Products));
                sqlCommand.Parameters.AddWithValue("@POSITION", JsonConvert.SerializeObject(business.Positions));
                sqlCommand.Parameters.AddWithValue("@NPC", JsonConvert.SerializeObject(business.PositionNPC));
                sqlCommand.Parameters.AddWithValue("@POSINFO", JsonConvert.SerializeObject(business.PositionInfo));
                sqlCommand.Parameters.AddWithValue("@TAX", business.Tax);
                sqlCommand.Parameters.AddWithValue("@EARNING", business.Earning);
                sqlCommand.Parameters.AddWithValue("@MARKUP", business.Markup);

                ENet.Database.Execute(sqlCommand);
            }
            catch(Exception ex) { Logger.WriteError("Command_CreateBusiness", ex); }
        }

        [ChatCommand("setbizped", Description = "Установить позицию НПС у бизнеса", Arguments = "[ID Бизнеса]", Access = PlayerRank.Owner)]
        private static void Command_SetBusinessPed(ENetPlayer player, int id)
        {
            try
            {
                var business = BusinessManager.GetBusiness(id);
                if (business is null)
                {
                    player.SendError($"Бизнес #{id} не найден");
                    return;
                }

                business.PositionNPC = new Position(player.Position.X, player.Position.Y, player.Position.Z, player.Heading);
                business.CreatePed();

                ENet.Database.Execute($"UPDATE `business` SET `npc`='{JsonConvert.SerializeObject(business.PositionNPC)}' WHERE `id`={business.ID}");
            }
            catch(Exception ex) { Logger.WriteError("Command_SetBusinessPed", ex); }
        }

        [ChatCommand("addbizpos", Description = "Добавить позицию взаимодествия к бизнесу", Arguments = "[ID Бизнеса]", Access = PlayerRank.Owner)]
        private static void Command_AddPositionToBiz(ENetPlayer player, int id)
        {
            try
            {
                var business = BusinessManager.GetBusiness(id);
                if (business is null)
                {
                    player.SendError($"Бизнес #{id} не найден");
                    return;
                }

                business.Positions.Add(player.Position);
                ENet.Database.Execute($"UPDATE `business` SET `position`='{JsonConvert.SerializeObject(business.Positions)}' WHERE `id`={business.ID}");
            }
            catch(Exception ex) { Logger.WriteError("Command_AddPositionToBiz", ex); }
        }
        #endregion

        [ChatCommand("sendcreator", Description = "Отправить игрока в создание персонажа", Arguments = "[статик]", Access = PlayerRank.Owner)]
        public static void Command_SendToCreator(ENetPlayer player, int staticId)
        {
            try
            {
                var target = ENet.Pools.GetPlayerByUUID(staticId);
                if (target is null)
                {
                    player.SendError(Language.GetText(TextType.PlayerDontExsistStatic, staticId));
                    return;
                }

                target.SendToCreator(false);
                player.SendDone(Language.GetText(TextType.YouSendToCreator, target.GetName()));
            }
            catch (Exception e) { Logger.WriteError("Command_SendToCreator", e); }
        }
        [ChatCommand("addclothes", Description = "Выдать одежду", Arguments = "[динамик] [id вещи]", Access = PlayerRank.CheifAdmin)]
        public static void Command_AddClothes(ENetPlayer player, int dynamicId, int id)
        {
            try
            {
                var target = ENet.Pools.GetPlayerByDynamic(dynamicId);
                if (target is null)
                {
                    player.SendError(Language.GetText(TextType.PlayerDontExsistDynamic, dynamicId));
                    return;
                }
                if (!target.GetCharacter(out CharacterData characterData)) return;

                var clothe = eNetwork.Framework.Configs.ClothesConfig.Get(id);
                if(clothe == null)
                {
                    player.SendError("Одежда не найдена");
                    return;
                }

                var item = TypedItems.Get(new Item(clothe.ItemId, 1, dataStr: JsonConvert.SerializeObject(new { ClotheId = id })));
                
                item.OwnerType = ItemOwnerType.Player;
                item.OwnerId = player.GetUUID();
                _ = Inventory.CreateNewItem(player, item);
                player.SendDone($"Вы выдали игроку {target.GetName()} предмет - {item.ItemData.Name}");
                LoggingService.Instance.Create(new AdminCommandUsageLogMessage(player.GetUUID(), player.GetName(), target.GetUUID(), target.GetName(), "addclothes"));
            }
            catch (Exception e) { Logger.WriteError("Command_AddClothes", e); }
        }

        [ChatCommand("giveitem", Description = "Выдать предмет инвентаря", Arguments = "[динамик] [название] [кол-во?] [дата?]", Access = PlayerRank.CheifAdmin)]
        public static void Command_GiveItem(ENetPlayer player, int dynamicId, string itemName, int count = 1, string data = "")
        {
            try
            {
                var target = ENet.Pools.GetPlayerByDynamic(dynamicId);
                if (target is null)
                {
                    player.SendError(Language.GetText(TextType.PlayerDontExsistDynamic, dynamicId));
                    return;
                }
                if (!target.GetCharacter(out CharacterData characterData)) return;

                ItemId itemId = (ItemId)Enum.Parse(typeof(ItemId), itemName);
                string itemData = data;

                ItemType itemType = InvItems.GetType(itemId);
                var itemInfo = InvItems.Get(itemId);

                if (itemType == ItemType.Clothes)
                {
                    player.SendError("Выдача одежды через /addclothes");
                    return;
                    //string[] splited = data.Split("_");
                    //if (itemId == ItemId.Bag)
                    //{
                    //    if (splited.Length != 3)
                    //    {
                    //        player.SendError("Неправильные данные");
                    //        return;
                    //    }
                    //    itemData = JsonConvert.SerializeObject(new InvBackpackData(characterData.CustomizationData.Gender, Convert.ToInt32(splited[0]), Convert.ToInt32(splited[1]), Convert.ToInt32(splited[2])));
                    //}
                    //else
                    //{
                    //    if (splited.Length != 2)
                    //    {
                    //        player.SendError("Неправильные данные");
                    //        return;
                    //    }
                    //    itemData = JsonConvert.SerializeObject(new InvClothesData(characterData.CustomizationData.Gender, Convert.ToInt32(splited[0]), Convert.ToInt32(splited[1])));
                    //}
                }
                if (itemType == ItemType.Weapon)
                {
                    itemData = JsonConvert.SerializeObject(
                        new
                        {
                            Endurance = 100,
                            Serial = WeaponController.GenerateSerial(FactionId.None, 99),
                            Components = new WeaponComponentsData()
                        }
                    );
                }

                var item = TypedItems.Get(new Item(itemId, count, dataStr: itemData));

                if (!Inventory.CheckCanAddItem(target, item))
                {
                    player.SendError("Недостаточно места в инвентаре!");
                    return;
                }
                item.OwnerType = ItemOwnerType.Player;
                item.OwnerId = player.GetUUID();
                _ = Inventory.CreateNewItem(player, item);
                player.SendDone($"Вы выдали игроку {target.GetName()} предмет - {itemInfo.Name}");
                LoggingService.Instance.Create(new AdminCommandUsageLogMessage(player.GetUUID(), player.GetName(), target.GetUUID(), target.GetName(), "giveitem"));
                
            }
            catch (Exception e) { Logger.WriteError("Command_GiveItem", e); }
        }

        [ChatCommand("setindicator", "Установить значение индикатора игрока", Access = PlayerRank.Admin, Arguments = "[динамик] [тип] [значение]")]
        public static void Command_SetIndicator(ENetPlayer player, int dynamicId, string type, int value)
        {
            try
            {
                var target = ENet.Pools.GetPlayerByDynamic(dynamicId);
                if (target is null)
                {
                    player.SendError(Language.GetText(TextType.PlayerDontExsistDynamic, dynamicId));
                    return;
                }
                if (!target.GetCharacter(out CharacterData characterData)) return;

                switch (type)
                {
                    case "water":
                        characterData.Indicators.SetWater(target, value);
                        player.SendInfo($"Вы установили показатели воды игроку {target.GetName()} на - {characterData.Indicators.Water}");
                        break;
                    case "hungry":
                        characterData.Indicators.SetHungry(target, value);
                        player.SendInfo($"Вы установили показатели еды игроку {target.GetName()} на - {characterData.Indicators.Hungry}");
                        break;
                }

                LoggingService.Instance.Create(new AdminCommandUsageLogMessage(player.GetUUID(), player.GetName(), target.GetUUID(), target.GetName(), "setindicator"));
            }
            catch (Exception ex) { Logger.WriteError("Command_SetIndicator", ex); }
        }

        [ChatCommand("setstatus", Description = "Установить статус на игрока", Arguments = "[динамик] [тип]", Access = PlayerRank.DepCheifAdmin)]
        public static void Command_SetStatus(ENetPlayer player, int dynamicId, string status)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData)) return;
                var target = ENet.Pools.GetPlayerByDynamic(dynamicId);
                if (target is null)
                {
                    player.SendError(Language.GetText(TextType.PlayerDontExsistDynamic, dynamicId));
                    return;
                }
                if (!target.GetCharacter(out CharacterData targetCharacterData)) return;

                if (!Enum.TryParse(typeof(PlayerRank), status, out object enumObject))
                {
                    player.SendError($"Статус {status} - не найден!");
                    return;
                }

                PlayerRank playerRank = (PlayerRank)enumObject;
                if (characterData.Status <= playerRank)
                {
                    player.SendWarning("Вы не можете выдать такой статус");
                    return;
                }

                targetCharacterData.Status = playerRank;
                target.SetData("player.status", (int)targetCharacterData.Status);
                LoggingService.Instance.Create(new AdminCommandUsageLogMessage(player.GetUUID(), player.GetName(), target.GetUUID(), target.GetName(), "setstatus"));
            }
            catch(Exception ex) { Logger.WriteError("Command_SetStatus", ex); }
        }

        [ChatCommand("setdim", Description = "Установить игровое измерение игроку", Arguments = "[игрок] [измерение]", Access = PlayerRank.JuniorAdmin)]
        public static void Command_Dimension(ENetPlayer player, int dynamicId, int dim)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData)) return;
                var target = ENet.Pools.GetPlayerByDynamic(dynamicId);
                if (target is null)
                {
                    player.SendError(Language.GetText(TextType.PlayerDontExsistDynamic, dynamicId));
                    return;
                }
                if (!target.GetCharacter(out CharacterData targetCharacterData)) return;

                player.SetDimension((uint)dim);
                LoggingService.Instance.Create(new AdminCommandUsageLogMessage(player.GetUUID(), player.GetName(), target.GetUUID(), target.GetName(), "setdim"));
            }
            catch(Exception ex) { Logger.WriteError("Command_Dimension", ex); }
        }

        [ChatCommand("sc", Description = "Установить компонент одежды", Arguments = "[тип] [компонент] [текстура]", Access = PlayerRank.DepCheifAdmin)]
        public static void Command_SetClothes(ENetPlayer player, int clothesType, int components, int texture)
        {
            player.SetClothes(clothesType, components, texture);    
        }

        [ChatCommand("saveserver", Description = "Сохранение сервера", Access = PlayerRank.CheifAdmin)]
        public static void Command_SaveServer(ENetPlayer player) => Modules.SafeActions.SafeActionsManager.SavingDatabase.Action(null);
        [ChatCommand("giveexp", Description = "Выдача опыта", Access = PlayerRank.CheifAdmin)]
        public static void Command_GiveExp(ENetPlayer player) => Game.Player.Leveling.Exp.Up(player);
    }
}
