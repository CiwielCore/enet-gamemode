using eNetwork.Businesses;
using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Game.Banks.Classes;
using eNetwork.Game.Banks.Player;
using System;

namespace eNetwork.Game.Banks
{
    public class BankController
    {
        private static readonly Logger Logger = new Logger("bank-controller");

        [InteractionDeprecated(ColShapeType.Bank)]
        public static void OnInteraction(ENetPlayer player)
        {
            try
            {
                if (!player.GetData("bank.point", out BankPoint bank)) return;

                bank.Interaction(player);
            }
            catch(Exception ex) { Logger.WriteError("OnInteraction", ex); }
        }

        [CustomEvent("server.bank.transferCard")]
        public static void TransferCard(ENetPlayer player, int value, long cardNumber)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetBankAccount(out var bankAccount)) return;

                var targetBankAccount = BankManager.GetBankAccount(cardNumber);
                if (targetBankAccount is null)
                {
                    player.SendError("Банковский счет не найден!");
                    return;
                }

                if (characterData.UUID == targetBankAccount.Owner)
                {
                    player.SendInfo("Вы не можете перевести себе деньги!");
                    return;
                }

                if (!player.ChangeBank(-value)) return;

                targetBankAccount.Change(value, out var target);
                if (target != null)
                {
                    player.SendDone($"Поступление средств на банковский счет - {Helper.FormatPrice(value)}");
                }
            }
            catch (Exception ex) { Logger.WriteError("Transfer: " + ex.ToString()); }
        }

        [CustomEvent("server.bank.transferPhone")]
        public static void TransferPhone(ENetPlayer player, int value, int phoneNumber)
        {
            try
            {
                // соси хуй неверидзе
            }
            catch(Exception ex) { Logger.WriteError("TransferPhone", ex); }
        }

        [CustomEvent("server.bank.topUp")]
        public static void TopUp(ENetPlayer player, int value, int pinCode)
        {
            try
            {
                if (!player.GetBankAccount(out var bankAccount)) return;

                if (bankAccount.PinCode != pinCode)
                {
                    player.SendError("Неверный пинкод!");
                    return;
                }

                if (value <= 0)
                {
                    player.SendError("Неверный ввод данных!");
                    return;
                }

                if (!player.ChangeWallet(-value)) return;

                if (player.ChangeBank(value))
                {
                    player.SendDone($"Пополнение средств: +{Helper.FormatPrice(value)}$");
                }
            }
            catch(Exception ex) { Logger.WriteError("TopUp", ex); }
        }

        [CustomEvent("server.bank.writeOff")]
        public static void WriteOff(ENetPlayer player, int value, int pinCode)
        {
            try
            {
                if (!player.GetBankAccount(out var bankAccount)) return;

                if (bankAccount.PinCode != pinCode)
                {
                    player.SendError("Неверный пинкод!");
                    return;
                }

                if (value <= 0)
                {
                    player.SendError("Неверный ввод данных!");
                    return;
                }

                if (player.ChangeBank(-value))
                {
                    player.SendDone($"Списание средств: -{Helper.FormatPrice(value)}$");
                    player.ChangeWallet(value);
                }
            }
            catch(Exception ex) { Logger.WriteError("WriteOff", ex); }
        }

        [CustomEvent("server.bank.business.tax")]
        public static void BusinessTax(ENetPlayer player, int value)
        {
            try
            {
                var business = BusinessManager.GetPlayerBusiness(player);
                if (business is null)
                {
                    player.SendError("У вас нет бизнеса!");
                    return;
                }

                if (value <= 0)
                {
                    player.SendError("Неверный ввод данных!");
                    return;
                }

                if (!player.ChangeBank(-value)) return;

                business.Tax += value;
                player.SendDone($"Вы пополнили счет вашего бизнеса на {Helper.FormatPrice(value)}$");
            }
            catch(Exception ex) { Logger.WriteError("BusinessTax", ex); }
        }

        [CustomEvent("server.bank.changePinCode")]
        public static void ChangePinCode(ENetPlayer player, int currentPinCode, int newPinCode, int confirmNewPinCode)
        {
            try
            {
                if (!player.GetBankAccount(out var bankAccount)) return;

                if (bankAccount.PinCode != currentPinCode)
                {
                    player.SendError("Неверный пинкод!");
                    return;
                }

                if (newPinCode != confirmNewPinCode)
                {
                    player.SendError("Новый пинкод не совпадает!");
                    return;
                }

                bankAccount.PinCode = newPinCode;
                bankAccount.Save();

                player.SendDone("Вы сменили пинкод!");
                ClientEvent.Event(player, "client.bank.close");
            }
            catch(Exception ex) { Logger.WriteError("ChangePinCode", ex); }
        }

        [CustomEvent("server.bank.create")]
        public static void Create(ENetPlayer player, int pinCode)
        {
            try
            {
                if (player.GetBankAccount(out var _))
                {
                    player.SendError("У вас уже есть банковский счет!");
                    return;
                }

                var bankAccount = new BankAccount()
                {
                    Balance = 0,
                    Id = BankManager.GenerateCardNumber(),
                    Owner = player.GetUUID(),
                    PinCode = pinCode
                };

                BankManager.BankAccounts.TryAdd(bankAccount.Id, bankAccount);
                bankAccount.Create();

                ClientEvent.Event(player, "client.bank.close");
                player.SendDone("Вы создали свой банковский счет! Откройте интерфейс еще раз!");
                Quests.QuestTasksHandler.Instance.AddProgressToTask(player, Quests.QuestTaskId.OpenBankAccount);
            }
            catch(Exception ex) { Logger.WriteError("Create", ex); }
        }
    }
}
