using eNetwork.Framework;
using eNetwork.Game.Banks.Classes;
using eNetwork.Game.Banks.Data;
using eNetwork.Game.Banks.Player;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace eNetwork.Game.Banks
{
    public class BankManager
    {
        private static readonly Logger Logger = new Logger("bank-manager");
        public static ConcurrentDictionary<long, BankAccount> BankAccounts = new ConcurrentDictionary<long, BankAccount>();

        public static ConcurrentDictionary<int, BankLog> BankLogs = new ConcurrentDictionary<int, BankLog>();

        public static void Initialize()
        {
            try
            {
                DataTable data = ENet.Database.ExecuteRead("SELECT * FROM `bank_accounts`");
                if (data != null && data.Rows.Count > 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        long id = Convert.ToInt64(row["id"]);
                        int owner = Convert.ToInt32(row["owner"]);
                        int pincode = Convert.ToInt32(row["pincode"]);
                        double balance = Convert.ToInt32(row["balance"]);

                        var accountData = new BankAccount() { Id = id, Owner = owner, Balance = balance, PinCode = pincode };
                        BankAccounts.TryAdd(id, accountData);
                    }
                }

                Logger.WriteInfo($"Загржено банковских аккаунтов ({BankAccounts.Count})");

                data = ENet.Database.ExecuteRead("SELECT * FROM `bank_logs`");
                if (data != null && data.Rows.Count > 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        int id = Convert.ToInt32(row["id"]);
                        BankLogType logType = (BankLogType)row["type"];
                        long from = Convert.ToInt64(row["from"]);
                        long to = Convert.ToInt64(row["to"]);
                        double amount = Convert.ToDouble(row["amount"]);
                        DateTime date = (DateTime)row["date"];

                        if (date > DateTime.Now.AddDays(-Config.MAX_COUNT_OF_LOGS_IN_HISTORY))
                        {
                            var log = new BankLog(id, logType, from, to, amount);
                            log.Date = date;

                            BankLogs.TryAdd(id, log);
                        }
                    }
                }

                Config.BANK_POINTS.ForEach(bank => bank.GTAElements());
            }
            catch(Exception e) { Logger.WriteError("Inititalize: " + e.ToString()); }
        }

        public static BankAccount GetBankAccount(long id)
        {
            BankAccounts.TryGetValue(id, out var accountData);
            return accountData;
        }

        public static long GenerateCardNumber()
        {
            string cardNumber = "";

            while (cardNumber == "" || BankAccounts.ContainsKey(Convert.ToInt64(cardNumber)))
            {
                cardNumber = "5375";
                for (int i = 0; i < 12; i++)
                {
                    cardNumber += ENet.Random.Next(0, 10);
                }
            }

            return Convert.ToInt64(cardNumber);
        }

        public static void SendLog(BankLogType bankLogType, long from, long to, double amount)
        {
            try
            {
                int id = 1;
                if (BankLogs.Count > 0)
                    id = BankLogs.Last().Key + 1;

                var bankLog = new BankLog(id, bankLogType, from, to, amount);
            }
            catch(Exception ex) { Logger.WriteError("SendLog", ex); }
        }

        public static List<object> GetHistory(BankAccount bankAccount)
        {
            try
            {
                var history = new List<object>();

                var bankLogs = BankLogs.Values.Where(x => (x.From == bankAccount.Id || x.To == bankAccount.Id)).Reverse().ToList();
                foreach (var bankLog in bankLogs)
                {
                    if (history.Count >= Config.MAX_COUNT_OF_LOGS_IN_HISTORY) 
                        break;

                    history.Add(bankLog.GetJsonData(bankAccount.Id));
                }

                return history;
            }
            catch(Exception ex) { Logger.WriteError("GetHistory", ex); return new List<object>(); }
        }
    }
}
