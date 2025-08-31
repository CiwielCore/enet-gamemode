using eNetwork.Framework.Classes;
using eNetwork.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GTANetworkAPI;
using System.Linq;
using eNetwork.Game.Characters.Customization;
using eNetwork.Game.Characters.Methods;
using MySqlConnector;
using System.Threading.Tasks;
using eNetwork.Framework.Enums;

namespace eNetwork.Game.Characters
{
    public class CharacterManager
    {
        private static readonly Logger Logger = new Logger("character");
        private static ConcurrentDictionary<int, CharacterData> _characters = new ConcurrentDictionary<int, CharacterData>();
        private static ConcurrentDictionary<int, string> _characterNames = new ConcurrentDictionary<int, string>();
        public const string DBName = "characters";

        private static int _lastUuid = 0;

        /// <summary>
        ///     Инициализация персонажей для аккаунта
        /// </summary>
        public static void Initialize()
        {
            try
            {
                DataTable result = ENet.Database.ExecuteRead($"SELECT * FROM `{DBName}`");
                DataTable resultFactionMaximumRank = ENet.Database.ExecuteRead($"SELECT * FROM `factions_ranks`");

                if (result is null)
                    return;

                foreach (DataRow Row in result.Rows)
                {
                    var data = new CharacterData();

                    data.UUID = Convert.ToInt32(Row["uuid"]);
                    data.Name = Convert.ToString(Row["name"]);
                    data.Cash = Convert.ToDouble(Row["cash"]);

                    data.Lvl = Convert.ToInt32(Row["lvl"]);
                    if (data.Lvl < 1) data.Lvl = 1;
                    data.Exp = Convert.ToInt32(Row["exp"]);
                    data.BankID = Convert.ToInt32(Row["bank"]);
                    data.HP = Convert.ToInt32(Row["hp"]);
                    data.Armor = Convert.ToInt32(Row["armor"]);
                    data.Warn = Convert.ToInt32(Row["warn_count"]);

                    data.BirthDay = (DateTime)(Row["birthday"]);
                    data.Status = (PlayerRank)Convert.ToInt32(Row["status"]);
                    data.Stats = JsonConvert.DeserializeObject<CharacterStats>(Row["stats"].ToString());
                    data.CustomizationData = JsonConvert.DeserializeObject<CustomizationData>(Row["customization"].ToString());
                    data.LastVector = JsonConvert.DeserializeObject<Vector3>(Row["lastvector"].ToString());
                    data.AdminData = JsonConvert.DeserializeObject<AdminData>(Row["adminData"].ToString());

                    data.JobId = (JobId)Convert.ToInt32(Row["jobId"]);

                    data.FactionId = Convert.ToInt32(Row["faction"]);
                    data.FactionRank = Convert.ToInt32(Row["factionrank"]);

                    data.Indicators = JsonConvert.DeserializeObject<PlayerIndicators>(Row["indicators"].ToString());

                    if (data.UUID > _lastUuid)
                        _lastUuid = data.UUID;

                    _characterNames.TryAdd(data.UUID, data.Name);
                    _characters.TryAdd(data.UUID, data);
                }

                Logger.WriteInfo($"Загружено {_characters.Count} персонажей");

                GTAElements();
            }
            catch (Exception e) { Logger.WriteError("Initialize", e); }
        }

        /// <summary>
        ///     Поиск существующего персонажа по его NickName
        /// </summary>
        /// <param name="name">Nickname персонажа</param>
        /// <returns>Существующий объект класса персонажа</returns>
        public static CharacterData GetCharacterData(string name)
        {
            return _characters
                .FirstOrDefault(x => x.Value.Name == name).Value;
        }

        /// <summary>
        ///     Поиск никнейма существующего персонажа по его уникальному индентификатору
        /// </summary>
        /// <param name="uuid">Уникальный индентификатор персонажа</param>
        /// <returns>Никнейм персонажа</returns>
        public static string GetName(int uuid)
        {
            if (!_characterNames.TryGetValue(uuid, out string name)) return String.Empty;
            return name;
        }

        /// <summary>
        ///     Поиск существующего персонажа по его уникальному индентификатору
        /// </summary>
        /// <param name="uuid">Уникальный индентификатор персонажа</param>
        /// <returns>Существующий объект класса персонажа</returns>
        public static CharacterData GetCharacterData(int uuid)
        {
            _characters.TryGetValue(uuid, out var characterData);
            return characterData;
        }

        /// <summary>
        ///     Попытаться добавить персонажа в словарь "_characters"
        /// </summary>
        /// <param name="data">Объект класса CharacterData содержащий информацию о персонаже</param>
        /// <returns>true/false в зависимости от исхода попытки добавить</returns>
        public static bool TryAddCharacter(CharacterData data)
        {
            return _characters.TryAdd(data.UUID, data);
        }

        /// <summary>
        ///     Генерация уникального индентификатора персонажа
        /// </summary>
        /// <returns>Сгенерированный индетификатор</returns>
        public static int GenerateUuid()
        {
            _lastUuid++;
            return _lastUuid;
        }

        /// <summary>
        ///     Получить случайную точку спавна игрового персонажа
        /// </summary>
        /// <returns>Вектор3 содержащий случайную точку для появления</returns>
        public static Vector3 GetRandomSpawn()
        {
            return Engine.Config.SpawnPositions[ENet.Random.Next(0, Engine.Config.SpawnPositions.Length)];
        }

        /// <summary>
        ///     Создает блип на игровой карте с местом спавна игрового персонажа
        /// </summary>
        public static void GTAElements()
        {
            foreach (Vector3 spawnPoint in Engine.Config.SpawnPositions)
                NAPI.Blip.CreateBlip(409, spawnPoint, 0.8f, 5, "Место приезда", 255, 0, true, 0, 0);
        }

        /// <summary>
        /// Сохраняем раз в определенное время игровых персонажей в базу данных
        /// </summary>
        /// <param name="player">Игровой персонаж</param>
        /// <param name="isDisconnect">Вышел ли игрок, изначальное значение аргумента false</param>
        public static async Task Save(ENetPlayer player, bool isDisconnect = false)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData data) || !data.IsSpawned) return;

                if (data.ExteriosPosition != null)
                {
                    data.LastVector = data.ExteriosPosition;
                }

                await Casino.CasinoManager.Save(player);

                MySqlCommand mySqlCommand = new MySqlCommand($"UPDATE `{DBName}` SET `name`=@name, `cash`=@cash, `lvl`=@lvl, `exp`=@exp, `bank`=@bank, `hp`=@hp, `armor`=@armor, " +
                    $"`birthday`=@birthday, `status`=@status, `Stats`=@Stats, `customization`=@customization, `adminData`=@adminData, `warn_count`=@warn_count, `lastvector`=@lastvector, `indicators`=@indicators, " +
                    $"`faction`=@faction, `factionrank`=@factionrank, `jobId`=@jobId WHERE `uuid`=@uuid");

                mySqlCommand.Parameters.AddWithValue("@uuid", data.UUID);
                mySqlCommand.Parameters.AddWithValue("@name", data.Name);
                mySqlCommand.Parameters.AddWithValue("@cash", data.Cash);
                mySqlCommand.Parameters.AddWithValue("@lvl", data.Lvl);
                mySqlCommand.Parameters.AddWithValue("@exp", data.Exp);
                mySqlCommand.Parameters.AddWithValue("@bank", data.BankID);
                mySqlCommand.Parameters.AddWithValue("@hp", data.HP);
                mySqlCommand.Parameters.AddWithValue("@armor", data.Armor);
                mySqlCommand.Parameters.AddWithValue("@birthday", data.BirthDay.ToString("s"));
                mySqlCommand.Parameters.AddWithValue("@status", (int)data.Status);
                mySqlCommand.Parameters.AddWithValue("@Stats", JsonConvert.SerializeObject(data.Stats));
                mySqlCommand.Parameters.AddWithValue("@customization", JsonConvert.SerializeObject(data.CustomizationData));
                mySqlCommand.Parameters.AddWithValue("@lastvector", JsonConvert.SerializeObject(data.LastVector));
                mySqlCommand.Parameters.AddWithValue("@adminData", JsonConvert.SerializeObject(data.AdminData));
                mySqlCommand.Parameters.AddWithValue("@warn_count", data.Warn);

                mySqlCommand.Parameters.AddWithValue("@jobId", (int)data.JobId);

                mySqlCommand.Parameters.AddWithValue("@faction", (int)data.FactionId);
                mySqlCommand.Parameters.AddWithValue("@factionrank", data.FactionRank);

                mySqlCommand.Parameters.AddWithValue("@indicators", JsonConvert.SerializeObject(data.Indicators));

                await ENet.Database.ExecuteAsync(mySqlCommand);
            }
            catch (Exception e) { Logger.WriteError("Save", e); }
        }

        /// <summary>
        ///     Вызывается с клиента при окончании создания персонажа, спавнит его и применяет на сервере лицо персонажа.
        /// </summary>
        /// <param name="player">Игровой персонаж</param>
        /// <param name="args">Имя, Фамилия, Данные о лице персонажа</param>
        [CustomEvent("server.character.create")]
        private async void CreateCharacter(ENetPlayer player, params object[] args)
        {
            try
            {
                if (!player.IsTimeouted("CREATE_CHARACTER", 1) || !player.GetAccountData(out AccountData accountData) || !player.GetData("creator.isFirst", out bool isFirst) || !player.GetData("creator.slot", out int slot) || args.Length < 16) return;

                int characterUuid = -1;
                if (isFirst && accountData.Characters[slot] <= 0)
                {
                    string _name = Convert.ToString(args[0]);
                    string _surname = Convert.ToString(args[1]);

                    characterUuid = await Methods.CreateCharacter.Create(player, _name, _surname);
                    if (characterUuid == -1) return;

                    accountData.Characters[slot] = characterUuid;
                }
                else
                {
                    characterUuid = accountData.Characters[slot];
                }

                bool gender = Convert.ToBoolean(args[2]);
                int age = Convert.ToInt32(args[3]);
                int father = Convert.ToInt32(args[4]);
                int mother = Convert.ToInt32(args[5]);
                float skinSimilarity = Convert.ToSingle(args[6]);
                int hairId = Convert.ToInt32(args[7]);
                int hairColor = Convert.ToInt32(args[8]);
                AppearanceData[] appearanceDatas = JsonConvert.DeserializeObject<AppearanceData[]>(args[9].ToString());
                float[] features = JsonConvert.DeserializeObject<float[]>(args[10].ToString());
                int eyeColor = Convert.ToInt32(args[11]);
                int tops = Convert.ToInt32(args[12]);
                int legs = Convert.ToInt32(args[13]);
                int shoes = Convert.ToInt32(args[14]);

                CharacterData data = CustomizationManager.SaveCreator(player, characterUuid, gender, age, mother, father, skinSimilarity, hairId, hairColor, appearanceDatas, features, eyeColor, tops, legs, shoes);
                if (data is null) return;

                player.SetCharacter(data);
                player.ApplyCustomization();

                data.Spawn(player, "CreateCharacter");

                ClientEvent.Event(player, "client.creator.close");
            }
            catch (Exception e) { Logger.WriteError("CreateCharacter", e); }
        }

        /// <summary>
        ///     Вызывается с CEF, выбирает одного из 3-х персонажей и спавнит его
        /// </summary>
        /// <param name="player">Игровой персонаж</param>
        /// <param name="index">Номер игрового персонажа от 1 до 3</param>
        /// <param name="spawnType">Вероятнее всего место спавна. На текущий момент неизвестно</param>
        [CustomEvent("server.authorization.selector.start")]
        private void SelectCharacter(ENetPlayer player, int index, string spawnType)
        {
            try
            {
                if (!player.IsTimeouted("select.character", 1) || !player.GetAccountData(out var accountData) || index < 0 || player.GetCharacter() != null || index > accountData.Characters.Count) return;
                int characterId = accountData.Characters[index];
                if (characterId <= 0)
                {
                    CustomizationManager.SendToCreator(player, true, index);
                    return;
                }
                CharacterData characterData = GetCharacterData(characterId);
                if (characterData is null)
                {
                    player.SendError(Language.GetText(TextType.ErrorTryGetCharacterData));
                    return;
                }
                player.SetCharacter(characterData);
                characterData.Spawn(player, spawnType);
            }
            catch (Exception e) { Logger.WriteError("SelectCharacter", e); }
        }
    }
}