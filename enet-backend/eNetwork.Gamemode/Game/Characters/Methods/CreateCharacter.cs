using eNetwork.Framework;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eNetwork.Game.Characters.Methods
{
    internal class CreateCharacter
    {
        private static readonly Logger Logger = new Logger("create-character");

        public static async Task<int> Create(ENetPlayer player, string name, string surname)
        {
            try
            {
                if (player.GetCharacter() != null) return -1;

                var data = new CharacterData();

                string dataName = $"{name}_{surname}";

                if (name.Replace(" ", "").Length < 3 || !Regex.IsMatch(name, "[a-zA-Z]") || !Helper.GetUpperInWord(name, 2))
                {
                    player.SendError(Language.GetText(TextType.CharacterErrorName), 3000);
                    return -1;
                }
                if (surname.Replace(" ", "").Length < 3 || !Regex.IsMatch(surname, "[a-zA-Z]") || !Helper.GetUpperInWord(surname, 2))
                {
                    player.SendError(Language.GetText(TextType.CharacterErrorSurname), 3000);
                    return -1;
                }

                if (CharacterManager.GetCharacterData(dataName) != null)
                {
                    player.SendError(Language.GetText(TextType.CharacterErrorExists));
                    return -1;
                }

                data.Name = dataName;
                data.UUID = CharacterManager.GenerateUuid();
                data.BirthDay = DateTime.Now;
                data.LastVector = CharacterManager.GetRandomSpawn();
                if (!CharacterManager.TryAddCharacter(data)) return -1;

                MySqlCommand sqlCommand = new MySqlCommand($"INSERT INTO `{CharacterManager.DBName}` (`uuid`,`name`,`cash`,`lvl`,`exp`,`bank`,`hp`,`armor`,`birthday`,`status`,`Stats`,`customization`,`lastvector`," +
                    $"`indicators`,`faction`,`factionrank`, `jobId`, `adminData`, `warn_count`) " +
                    $"VALUES(@UUID, @NAME, @CASH, @LVL, @EXP, @BANK, @HP, @ARMOR, @BIRTH, @STATUS, @Stats, @CUSTOMIZE, @LASTVECTOR, @indicators, @faction, @factionrank, @jobId, @adminData, @warn_count)");
                sqlCommand.Parameters.AddWithValue("@UUID", data.UUID);
                sqlCommand.Parameters.AddWithValue("@NAME", data.Name);
                sqlCommand.Parameters.AddWithValue("@CASH", Convert.ToInt64(data.Cash * 100));
                sqlCommand.Parameters.AddWithValue("@LVL", data.Lvl);
                sqlCommand.Parameters.AddWithValue("@EXP", data.Exp);
                sqlCommand.Parameters.AddWithValue("@BANK", data.BankID);
                sqlCommand.Parameters.AddWithValue("@HP", data.HP);
                sqlCommand.Parameters.AddWithValue("@ARMOR", data.Armor);
                sqlCommand.Parameters.AddWithValue("@BIRTH", data.BirthDay.ToString("s"));
                sqlCommand.Parameters.AddWithValue("@STATUS", (int)data.Status);
                sqlCommand.Parameters.AddWithValue("@Stats", JsonConvert.SerializeObject(data.Stats));
                sqlCommand.Parameters.AddWithValue("@CUSTOMIZE", JsonConvert.SerializeObject(data.CustomizationData));
                sqlCommand.Parameters.AddWithValue("@LASTVECTOR", JsonConvert.SerializeObject(data.LastVector));

                sqlCommand.Parameters.AddWithValue("@adminData", JsonConvert.SerializeObject(data.AdminData));
                sqlCommand.Parameters.AddWithValue("@warn_count", data.Warn);
                sqlCommand.Parameters.AddWithValue("@jobId", (int)data.JobId);

                sqlCommand.Parameters.AddWithValue("@indicators", JsonConvert.SerializeObject(data.Indicators));

                sqlCommand.Parameters.AddWithValue("@faction", (int)data.FactionId);
                sqlCommand.Parameters.AddWithValue("@factionrank", data.FactionRank);

                await ENet.Database.ExecuteAsync(sqlCommand);

                player.SetCharacter(data);

                return data.UUID;
            }
            catch (Exception e) { Logger.WriteError("Create", e); return -1; }
        }
    }
}