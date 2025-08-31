using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using eNetwork.Framework.Classes;
using eNetwork.Framework.API.Database.Classes;

namespace eNetwork.Framework.API.Database
{
    public class MySql
    {
        private static readonly Logger _logger = new Logger("Database");
        private string Connection = null;

        public void Initialize(MySqlSettings MysqlSettings)
        {
            if (Connection is string) return;
            Connection =
                $"Host={MysqlSettings.Host};" +
                $"User={MysqlSettings.User};" +
                $"Password={MysqlSettings.Password};" +
                $"Database={MysqlSettings.Database};" +
                $"SslMode=None;";
        }

        public void Execute(MySqlCommand command)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Connection))
                {
                    connection.Open();
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e) { _logger.WriteError("Execute", e); }
        }
        public void Execute(string command)
        {
            using (MySqlCommand cmd = new MySqlCommand(command))
            {
                Execute(cmd);
            }
        }

        public async Task ExecuteAsync(MySqlCommand command)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Connection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e) { _logger.WriteError("ExecuteAsync", e); }
        }

        public async Task ExecuteAsync(string command)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Connection))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = command;
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e) { _logger.WriteError("ExecuteAsync", e); }
        }
        public async Task<int> ExecuteAsyncInt(string command)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Connection))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand cmd = new MySqlCommand(command, connection))
                    {
                        await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(cmd.LastInsertedId);
                    }
                }
            }
            catch (Exception e) { _logger.WriteError("ExecuteAsyncInt", e); }
            return -1;
        }

        public DataTable ExecuteRead(MySqlCommand command)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Connection))
                {
                    connection.Open();
                    command.Connection = connection;
                    DbDataReader reader = command.ExecuteReader();
                    DataTable result = new DataTable();
                    result.Load(reader);
                    return result;
                }
            }
            catch(Exception ex) { _logger.WriteError("ExecuteRead", ex); return null; }
        }
        public DataTable ExecuteRead(string command)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(command))
                {
                    return ExecuteRead(cmd);
                }
            }
            catch (Exception ex) { _logger.WriteError("ExecuteRead", ex); return null; }
        }

        public async Task<DataTable> ExecuteReadAsync(MySqlCommand command)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Connection))
                {
                    await connection.OpenAsync();
                    command.Connection = connection;
                    DbDataReader reader = await command.ExecuteReaderAsync();
                    DataTable result = new DataTable();
                    result.Load(reader);
                    return result;
                }
            }
            catch (Exception ex) { _logger.WriteError("ExecuteReadAsync", ex); return null; }
        }
        public async Task<DataTable> ExecuteReadAsync(string command)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(command))
                {
                    return await ExecuteReadAsync(cmd);
                }
            }
            catch (Exception ex) { _logger.WriteError("ExecuteReadAsync", ex); return null; }
        }
    }
}
