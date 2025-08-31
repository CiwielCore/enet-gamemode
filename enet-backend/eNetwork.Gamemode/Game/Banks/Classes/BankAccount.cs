using eNetwork.Game.Banks.Player;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Banks.Classes
{
    public class BankAccount
    {
        public long Id { get; set; }
        public int Owner { get; set; }
        public int PinCode { get; set; }
        public double Balance { get; set; }

        public ENetPlayer GetOwner()
        {
            return ENet.Pools.GetPlayerByUUID(Owner);
        }

        public bool Change(double money, out ENetPlayer owner, bool notify = false, ENetPlayer player = null)
        {
            if (player is null)
                player = GetOwner();

            var state = Change(money, notify, player);
            owner = player;

            return state;
        }

        public bool Change(double money, bool notify = false, ENetPlayer player = null)
        {
            if (player is null)
                player = GetOwner();

            if (Balance + money < 0)
            {
                if (notify && player != null)
                    player.SendError("Ошибка списания денег с банковского счета: Недостаточно средств");

                return false;
            }
            if (Balance + money > Double.MaxValue)
            {
                if (notify && player != null)
                    player.SendError("Ошибка пополнения банковского баланса");

                return false;
            }

            Balance += money;

            if (player != null)
            {
                NAPI.Task.Run(() => player.SetSharedData("player.bank", Balance));
            }
            else
            {
                Save();
            }

            return true;
        }

        public void Create()
        {
            ENet.Database.Execute($"INSERT INTO `bank_accounts` (`id`, `owner`, `balance`, `pincode`) VALUES({Id}, {Owner}, {Balance}, {PinCode})");
        }

        public void Save()
        {
            ENet.Database.Execute($"UPDATE `bank_accounts` SET `balance`={Balance}, `pincode`={PinCode} WHERE `id`={Id}");
        }
    }
}
