using MySqlConnector;

namespace eNetwork.Services.Logging
{
    public abstract class LogMessage
    {
        public abstract MySqlCommand ToInsertCommand();
        public abstract string ToMessageString();
    }
}
