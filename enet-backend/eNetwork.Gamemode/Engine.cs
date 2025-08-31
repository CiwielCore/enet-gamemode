using Colorful;
using eNetwork.External;
using eNetwork.Framework;
using eNetwork.Framework.API.Database.Classes;
using eNetwork.Modules;
using eNetwork.Modules.SafeActions;
using eNetwork.World.Hunting;
using GTANetworkAPI;
using System;
using System.Threading;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace eNetwork
{
    public class Engine : Script
    {
        private static readonly Logger Logger = new Logger("e-net");

        public static ServerConfig Config;

        [ServerEvent(Event.ResourceStart)]
        public void OnStart()
        {
            try
            {
                Thread.CurrentThread.Name = "Main";

                Config = new ServerConfig()
                {
                    ServerName = "Project: Elision",
                    ServerNumber = 1,
                    UIUrl = "http://package/web/index.html",
                    DiscordToken = "",
                    IsWhitelist = true,
                };

                #region ConsoleConfig

                Console.Clear();
                string configText = "[============================================================]\n" +
                              "||\n";

                var config = Engine.Config;
                var properties = config.GetType().GetProperties();
                var parameters = new Formatter[properties.Length];
                int i = 0;

                foreach (var item in properties)
                {
                    configText += "||   " + item.Name + ": {" + i + "} \n";
                    parameters[i] = new Formatter(Convert.ToString(item.GetValue(config)), System.Drawing.Color.LightGray);
                    i++;
                };

                configText += "||\n" +
                              "[============================================================]\n";

                Console.Title = $"EternalDev | {config.ServerName} #{config.ServerNumber}";
                Console.WriteLineFormatted(configText, System.Drawing.Color.Orange, parameters);
                Console.WriteAscii($" {config.ServerName}", System.Drawing.Color.Aquamarine);
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                #endregion ConsoleConfig

                // Запуск игровых моментов
                ServerEvents.ResourceStart.Initialize();

                // Загрузка всех ивентов
                ENet.CustomEvent.Initialize();

                // Загрузка всех взаимодействий
                ENet.Interaction.Initialize();

                // Загрузка сценовых действий в основной сборке

                ENet.SceneAction.Initialize();

                // Загрузка всех команд
                ENet.ChatCommands.Initialize();

                // Подгрузка клиентской части
                //ENet.Clientside.Initialize();

                // Загрузка таймеров
                Timers.Initialize();

                // Загрузка базы данных
                ENet.Database.Initialize(ConfigReader.ReadAsync("db", new MySqlSettings()));

                // Загрузка базы конфигов
                ENet.Config.Initialize(ConfigReader.ReadAsync("config", new MySqlSettings()));

                Factions.FactionsManager.Initialization();

                // Other
                Game.Core.BansCharacter.BanHandler.Instance.OnResourceStart();
                Gambles.Lotteries.Lottery.Instance.OnResourceStart();
                Demorgan.DemorganRepository.Instance.OnResourceStart();
                Mute.MuteRepository.Instance.Initialization();

                // Services
                Services.CarRental.CarRentalRepository.Instance.OnResourceStart();
                Services.VipServices.VipService.Instance.OnResourceStart();
                Services.Promocodes.PromocodeService.Instance.OnResourceStart();
                Services.Logging.LoggingService.Instance.OnResourceStart();
                Services.BonusServices.DailyBonus.Instance.OnResourceStart();

                Game.Autoschool.AutoschoolManager.Instance.OnResourceStart();

                // Загрузка конфигов
                Framework.Configs.ConfigsManager.Initialize();

                Framework.Configs.ClothesConfig.Initialize();

                Framework.Configs.VehiclesConfig.Initialize();

                Configs.VehicleNames.Initialize();
                Businesses.Products.Ammunations.Initialize();
                Businesses.Products.Shops24.Initialize();
                Businesses.List.TattooShop.Manager.Initialize();

                // Загрузка кат. сцен
                ENet.SceneManager.Initialize();

                // Загрузка аккаунтов
                Game.Accounts.AccountManager.Initialize();

                //Загрузка персонажей
                Game.Characters.CharacterManager.Initialize();

                // Загрузка инвентаря
                Game.Inventory.Initialize();

                // Загрузка бизнесов
                Businesses.BusinessManager.Initialize();

                // Загрузка модулей ***************
                World.WeatherHandler.Initialize();
                World.IPLs.Initialize();
                TeleportsPoints.Initialize();

                Jobs.BusDriver.BusManager.Initialize();
                Jobs.Trucker.TruckerManager.Initialize();
                Jobs.Fishing.FishingManager.Initialize();
                Jobs.Builder.BuilderManager.Initialize();

                Game.Casino.CasinoManager.Initialize();

                Game.LunaPark.FerrisWheel.Initialize();

                Game.Vehicles.VehicleSync.Initialize();
                Game.Vehicles.VehicleManager.Initialize();

                Game.Player.Animation.Initialize();

                HuntingHandler.Initialize();
                Game.Banks.BankManager.Initialize();
                Game.Banks.MazeBank.Initialize();
                Game.Core.Whitelist.Manager.Initialize();

                Game.HiddingBox.HiddingBoxManager.Initialize();

                //Factions.Factionnager.Initialize();
                //Factions.Tasks.CarTheft.CarTheftManager.Initialize();
                //Factions.Tasks.Smuggling.SmugglingManager.Initialize();

                Property.Parking.ParkingManager.Initialize();
                Houses.HousesManager.Initialize();

                //Discord Webhooks
                // Discord.Init();
                // Discord.SendMessage("log", "Bot started");

                Timers.StartOnce(60000 - (DateTime.Now.Second * 1000), () =>
                {
                    SafeActionsManager.EveryMinute.Initialize();
                    Logger.WriteDone("Запуск синхронизированного ежеминутного таймера");
                });

                SafeActionsManager.SavingDatabase.Initialize();
                // ********************************

                //ENet.Sounds.Play3DSoundAtCoord(new Position(-33, -1098, 27, 0), "https://mp3uks.ru/mp3/files/primu-phonk-workout-mp3.mp3", 0.25, 122, true);

                AppDomain.CurrentDomain.ProcessExit += (o, e) =>
                {
                    try
                    {
                        Console.Clear();

                        NAPI.Task.Run(() => SafeActionsManager.SavingDatabase.Action(null));
                        Task.Delay(1);

                        Environment.Exit(0);
                    }
                    catch (Exception ex) { Logger.WriteError("ProccesExit", ex); Task.Delay(-1); }
                };
            }
            catch (Exception e) { Logger.WriteError("ErrorMessageParams", e); }
        }
    }
}