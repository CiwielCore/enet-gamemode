using MySqlConnector;

namespace eNetwork.Services.Logging.LogMessages
{
    internal class AdminCommandUsageLogMessage : LogMessage
    {
        public AdminCommandUsageLogMessage(int adminCharacterId, string adminName, int targetCharacterId, string targetName, string command)
        {
            AdminCharacterId = adminCharacterId;
            AdminFullName = adminName;
            TargetCharacterId = targetCharacterId;
            TargetFullName = targetName;
            Command = command;
        }

        public int AdminCharacterId { get; set; }
        public string AdminFullName { get; set; }
        public int TargetCharacterId { get; set; }
        public string TargetFullName { get; set; }
        public string Command { get; set; }

        public override MySqlCommand ToInsertCommand()
        {
            MySqlCommand command = new MySqlCommand(@"
                INSERT INTO `admin_commands_usage_logs`
                (`command`, `admin_character_id`, `admin_character_name`, `target_character_id`, `target_character_name`, `date`)
                VALUES (@command, @admin, @adminName, @target, @targetName, UNIX_TIMESTAMP());
            ");

            command.Parameters.AddWithValue("@command", Command);
            command.Parameters.AddWithValue("@admin", AdminCharacterId);
            command.Parameters.AddWithValue("@adminName", AdminFullName);
            command.Parameters.AddWithValue("@target", TargetCharacterId);
            command.Parameters.AddWithValue("@targetName", TargetFullName);
            return command;
        }

        public override string ToMessageString()
        {
            return $"Админ {AdminFullName} ({AdminCharacterId}) использовал команду {Command} на игроке {TargetFullName} {TargetCharacterId}";
        }
    }
}
