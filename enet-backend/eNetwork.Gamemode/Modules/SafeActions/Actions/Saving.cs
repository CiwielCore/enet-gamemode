using eNetwork.Framework;
using eNetwork.Game;
using eNetwork.Game.Banks.Player;
using eNetwork.Modules.SafeActions.Classes;
using System;

namespace eNetwork.Modules.SafeActions.Actions
{
    public class Saving : SafeIntervalAction
    {
        private static readonly Logger Logger = new Logger("saving");

        public Saving()
        {
            ThreadName = "SavingDatabase";
            IntervalTime = 480;
            AtOnce = false;
        }

        public override void Action(object obj)
        {
            try
            {
                foreach (ENetPlayer player in ENet.Pools.GetAllRegisteredPlayers())
                    player.SavePosition();

                ENet.Task.SetAsyncTask(async () =>
                {
                    Logger.WriteWarning("Сохранение базы данных...");

                    DateTime now = DateTime.Now;

                    //await Factions.Factionnager.Save();
                    //Logger.WriteSave($"Сохраняем фракции... ({(DateTime.Now - now).TotalSeconds})");

                    now = DateTime.Now;
                    foreach (ENetVehicle vehicle in ENet.Pools.GetAllVehicles())
                    {
                        if (vehicle.GetVehicleData(out VehicleData data))
                        {
                            Game.Vehicles.VehicleManager.Save(vehicle);
                        }
                    }

                    Logger.WriteSave($"Сохраняем транспорт... ({(DateTime.Now - now).TotalSeconds})");

                    now = DateTime.Now;
                    await Businesses.BusinessManager.SavingBusinesses();
                    Logger.WriteSave($"Сохраняем бизнесы... ({(DateTime.Now - now).TotalSeconds})");

                    now = DateTime.Now;
                    foreach (ENetPlayer player in ENet.Pools.GetAllPlayers())
                    {
                        if (player.GetCharacter(out CharacterData characterData))
                        {
                            await Game.Characters.CharacterManager.Save(player);
                            await Inventory.Save(player.GetUUID());
                            await Services.BonusServices.DailyBonus.Instance.Save(player.GetUUID());
                        }

                        if (player.GetAccountData(out AccountData accountData))
                        {
                            await Game.Accounts.AccountManager.Save(player);
                        }

                        if (player.GetBankAccount(out var bankAccount))
                        {
                            bankAccount.Save();
                        }
                    }

                    Logger.WriteSave($"Сохраняем игроков... ({(DateTime.Now - now).TotalSeconds})");

                    now = DateTime.Now;
                    await Demorgan.DemorganRepository.Instance.SaveRecordsInDatabase();
                    Logger.WriteSave($"Сохранение базы деморгана... ({(DateTime.Now - now).TotalSeconds})");

                    Logger.WriteDone($"Сохранение прошло успешно!");
                });
            }
            catch (Exception e) { Logger.WriteError("SavingDatabase", e); }
        }
    }
}