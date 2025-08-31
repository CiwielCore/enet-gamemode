using MySqlConnector;

namespace eNetwork.Services.Logging.LogMessages
{
    internal class CashTransferLogMessage : LogMessage
    {
        public CashTransferLogMessage(int senderCharacterId, string senderName, int recipientCharacterId, string recipientName, int amount)
        {
            SenderCharacterId = senderCharacterId;
            SenderFullName = senderName;
            RecipientCharacterId = recipientCharacterId;
            RecipientFullName = recipientName;
            Amount = amount;
        }

        public int SenderCharacterId { get; set; }
        public string SenderFullName { get; set; }
        public int RecipientCharacterId { get; set; }
        public string RecipientFullName { get; set; }
        public int Amount { get; set; }

        public override MySqlCommand ToInsertCommand()
        {
            MySqlCommand command = new MySqlCommand(@"
                INSERT INTO `cash_transfer_logs`
                (`sender_character_id`, `sender_character_name`, `recipient_character_id`, `recipient_character_name`, `amount`, `date`)
                VALUES (@sender, @senderName, @recipient, @recipientName, @amount, UNIX_TIMESTAMP());
            ");

            command.Parameters.AddWithValue("@sender", SenderCharacterId);
            command.Parameters.AddWithValue("@senderName", SenderFullName);
            command.Parameters.AddWithValue("@recipient", RecipientCharacterId);
            command.Parameters.AddWithValue("@recipientName", RecipientFullName);
            command.Parameters.AddWithValue("@amount", Amount);
            return command;
        }

        public override string ToMessageString()
        {
            return $"Игрок {SenderFullName} ({SenderCharacterId}) передал деньги игроку {RecipientFullName} {RecipientCharacterId} в количестве ${Amount}";
        }
    }
}
