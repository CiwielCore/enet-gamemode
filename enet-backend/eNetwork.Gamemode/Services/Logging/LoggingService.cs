using eNetwork.Framework;
using eNetwork.Framework.API.Database;
using eNetwork.Framework.API.Database.Classes;
using eNetwork.Framework.Singleton;
using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace eNetwork.Services.Logging
{
    internal class LoggingService : Singleton<LoggingService>
    {
        private readonly Logger _logger;
        private readonly MySql _database;
        private readonly Thread _worker;
        private readonly ConcurrentQueue<LogMessage> _cache;

        private LoggingService()
        {
            _logger = new Logger("logging");
            _database = new MySql();
            _worker = new Thread(Tick);
            _cache = new ConcurrentQueue<LogMessage>();
        }

        public void OnResourceStart()
        {
            _logger.WriteInfo("Logging service starting...");
            _database.Initialize(ConfigReader.Read<MySqlSettings>("enet_logs_db.json"));
            _worker.IsBackground = true;
            _worker.Start();
            _logger.WriteDone("Logging service started.");
        }

        public void Create(LogMessage message)
        {
            try
            {
                _cache.Enqueue(message);
            }
            catch (Exception ex)
            {
                _logger.WriteError($"Insert: {ex}");
            }
        }

        private void Tick()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(200);

                    if (_cache.Count == 0)
                        continue;

                    if (!_cache.TryDequeue(out LogMessage log))
                        continue;

                    if (log == null)
                        continue;

                    MySqlCommand command = log.ToInsertCommand();
                    _database.ExecuteAsync(command).ContinueWith((t) =>
                    {
                        _logger.WriteInfo("Logged: " + log.ToMessageString());
                    });
                }
                catch (Exception ex)
                {
                    _logger.WriteError($"Tick: {ex}");
                }
            }
        }
    }
}
