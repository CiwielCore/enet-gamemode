using eNetwork.Game.Banks.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Banks.Classes
{
    public class BankLog
    {
        public int Id { get; set; }
        public BankLogType Type { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public BankLog(int id, BankLogType type, long from, long to, double amount)
        {
            Type = type;
            From = from;
            To = to;
            Amount = amount;

            Id = id;
        }

        public object GetJsonData(long bankId)
        {
            return new
            {
                Type = Type.ToString(),
                Text = 
                    Type == BankLogType.Pay ? "Трата по карте" :
                    Type == BankLogType.Transfer ? 
                        From == bankId ? "Перевод средств" : "Поступление средств" 
                    
                    : "-",

                Target = To,
                Count = Amount,
                IsSpent = From == bankId,
            };
        }
    }
}
