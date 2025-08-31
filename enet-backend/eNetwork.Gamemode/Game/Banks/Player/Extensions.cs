using eNetwork.Framework;
using eNetwork.Game.Banks.Classes;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Game.Banks.Player
{
    public static class Extensions
    {
        private static readonly Logger Logger = new Logger("bank-player");

        public static bool GetBankAccount(this ENetPlayer player, out BankAccount bankAccount)
        {
            bankAccount = BankManager.BankAccounts.Values.ToList().Find(x => x.Owner == player.GetUUID());
            return bankAccount != null;
        }

        public static bool ChangeBank(this ENetPlayer player, double money, bool notify = true)
        {
            if (!player.GetBankAccount(out var bankAccount)) return false;
            return bankAccount.Change(money, notify, player);
        }
    }
}
