using eNetwork.Framework;
using eNetwork.Game;
using eNetwork.Game.Vehicles;
using eNetwork.GameUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using System.Linq;
using eNetwork.Framework.Enums;

namespace eNetwork.Businesses
{
    public class Showroom : Business
    {
        private static readonly Logger Logger = new Logger("showroom");

        [CustomEvent("server.business.showroom.select")]
        private static void Event_SelectModel(ENetPlayer player, int index)
        {
            try
            {
                if (!player.IsTimeouted("event.biz.select.model", 1) || !player.GetData<Showroom>("CURRENT_BUSINESS", out Showroom showroom)) return;
                showroom.ChangeModel(player, index);
            }
            catch (Exception ex) { Logger.WriteError("Event_SelectModel", ex); }
        }

        [CustomEvent("server.business.showroom.exit")]
        private static void Event_Exit(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData data) || !player.IsTimeouted("event.biz.showroom.exit", 1) || !player.GetData<Showroom>("CURRENT_BUSINESS", out Showroom showroom)) return;

                player.Position = showroom.Positions[0];
                player.Dimension = 0;
                data.ExteriosPosition = null;

                player.ResetData("CURRENT_BUSINESS");
            }
            catch (Exception ex) { Logger.WriteError("Event_Exit", ex); }
        }

        private static async void OnOpen(ENetPlayer player, params object[] arguments)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData data) || !player.IsTimeouted("events.biz.showroom.open", 1) || !player.GetData("CURRENT_PED_BUSINESS", out Showroom showroom) || !player.HasData("BUSINESS_POSITION_INDEX")) return;

                int index = player.GetData<int>("BUSINESS_POSITION_INDEX");
                Vector3 position = showroom.Positions.ElementAt(index);

                if (position is null) return;

                Transition.Open(player, "Загрузка транспорта");

                await Task.Delay(600);

                Dialogs.Close(player, 0);

                player.SetDimension(DimensionHandler.RequestPrivateDimension(player));
                player.SetData("CURRENT_BUSINESS", showroom);
                data.ExteriosPosition = showroom.Positions[0];

                var products = showroom.Products.Clone();
                products.ForEach(prod => prod.Price = prod.GetPrice(showroom));

                ClientEvent.Event(player, "client.business.showroom.open", (int)showroom.Type, showroom.Name, JsonConvert.SerializeObject(position), JsonConvert.SerializeObject(products));
                Game.Quests.QuestTasksHandler.Instance.AddProgressToTask(player, Game.Quests.QuestTaskId.TakeLookAtCarDealership);
            }
            catch (Exception ex) { Logger.WriteError("InteractionBiz", ex); }
        }

        [CustomEvent("server.business.showroom.buy")]
        private static void Event_Buy(ENetPlayer player, int index, string primColor, string secColor)
        {
            try
            {
                if (!player.IsTimeouted("event.biz.showroom.buy", 1) || !player.GetData("CURRENT_BUSINESS", out Showroom showroom)) return;
                showroom.Buy(player, index, primColor, secColor);
            }
            catch (Exception ex) { Logger.WriteError("Event_Buy", ex); }
        }

        [CustomEvent("server.business.showroom.testdrive")]
        private static void Event_Testdrive(ENetPlayer player, int index, string primColor, string secColor)
        {
            try
            {
                if (!player.IsTimeouted("event.biz.showroom.testdrive", 1) || !player.GetData("CURRENT_BUSINESS", out Showroom showroom)) return;
                showroom.StartTestdrive(player, index, primColor, secColor);
            }
            catch (Exception ex) { Logger.WriteError("Event_Buy", ex); }
        }

        public static async void EndTestdrive(ENetPlayer player, bool closeMenu = true)
        {
            try
            {
                if (!player.GetData("testdrive.vehicle", out ENetVehicle vehicle)) return;

                Transition.Open(player, "Отвозоим транспорт обратно в салон");
                ClientEvent.Event(player, "client.timer.stop", "TESTDRIVE");

                await Task.Delay(500);

                NAPI.Task.Run(() => vehicle.Delete());
                player.ResetData("testdrive.vehicle");
                if (player.HasData("testdrive.timer"))
                {
                    Timers.Stop(player.GetData<string>("testdrive.timer"));
                    player.ResetData("testdrive.timer");
                }

                if (closeMenu)
                    ClientEvent.Event(player, "client.business.showroom.exit");
            }
            catch (Exception ex) { Logger.WriteError("EndTestdrive", ex); }
        }

        public Showroom(int id, BusinessType type) : base(id, type)
        {
            BlipType = 824;
            BlipColor = 4;
            Name = "Автосалон Elision";
            ShapeRadius = 2;
            ShapeHeight = 2;

            InteractionPedText = "Нажмите чтобы поговорить с Памелой";

            PedHash = 0x0703F106;
        }

        public override void Initialize()
        {
            GTAElements();

            CreateBlip(null);
        }

        public override void InteractionNpc(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData)) return;

                var dialog = new Dialog()
                {
                    ID = (int)DialogType.Showroom,
                    Name = "Памела Фултон",
                    Description = $"Консультант {Name}",
                    Text = Language.GetText(TextType.WelcomePDM, player.GetName(), Helper.GenderString("", player.Gender)),
                    Answers = new List<DialogAnswer>() {
                        new DialogAnswer(Language.GetText(TextType.GoShowShowroom), OnOpen, "open"),
                        new DialogAnswer(Language.GetText(TextType.NoThanks), null, "close")
                    },
                };

                dialog.Open(player, Ped);
            }
            catch (Exception ex) { Logger.WriteError("InteractionNpc", ex); }
        }

        public void ChangeModel(ENetPlayer player, int index)
        {
            try
            {
                if (index < 0 || Products.Count <= index) return;
                var product = Products[index];

                object vehData = new
                {
                    Petrol = "undefined",
                    Weight = 0,
                    Speed = 0,
                };

                if (VehicleSync.GetVehicleConfig(product.Name, out var vehicleConfig))
                {
                    vehData = new
                    {
                        Petrol = vehicleConfig.PetrolType,
                        Weight = 0,
                        Speed = 0,
                    };
                }

                ClientEvent.Event(player, "client.business.showroom.select", index, JsonConvert.SerializeObject(vehData));
            }
            catch (Exception ex) { Logger.WriteError("ChangeModel", ex); }
        }

        public void Buy(ENetPlayer player, int index, string primaryColor, string secondaryColor)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData) || !Configs.BusinessConfig.Colors.ContainsKey(primaryColor) || !Configs.BusinessConfig.Colors.ContainsKey(secondaryColor)) return;

                var product = Products[index];
                int totalPrice = product.GetPrice(this);

                var primColor = Configs.BusinessConfig.Colors[primaryColor];
                var secColor = Configs.BusinessConfig.Colors[secondaryColor];

                if (characterData.Cash < totalPrice)
                {
                    player.SendError("Недостаточно средств");
                    return;
                }

                if (TakeProduct(1, product.Name, totalPrice))
                {
                    player.SendError(Language.GetText(TextType.NoModelsLeft));
                    return;
                }

                player.ChangeWallet(totalPrice);

                Position spawnPosition = GetSpawnPosition();
                if (spawnPosition is null) return;

                var vehicleId = VehicleManager.CreateVehicle(new VehicleOwner(OwnerVehicleEnum.Player, player.GetUUID()), product.Name, primColor.ColorId, secColor.ColorId, false).Result;
                var vehicle = VehicleManager.SpawnVehicle(player.GetUUID(), vehicleId, spawnPosition, 0);

                if (vehicle is null)
                {
                    player.SendError(Language.GetText(TextType.ErrorBuyVehicle));
                    return;
                }

                Transition.Open(player, "Паркуем ваш новый транспорт");
                ENet.Task.SetAsyncTask(async () =>
                {
                    try
                    {
                        await Task.Delay(500);

                        player.SendDone(Language.GetText(TextType.YouBuyVehicle, Configs.VehicleNames.Get(product.Name)));
                        ClientEvent.Event(player, "client.business.showroom.exit");
                    }
                    catch (Exception ex) { Logger.WriteError("Buy.SetAsyncTask", ex); }
                });
            }
            catch (Exception ex) { Logger.WriteError("Buy", ex); }
        }

        public void StartTestdrive(ENetPlayer player, int index, string primaryColor, string secondaryColor)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData)) return;

                var config = Configs.BusinessConfig.ShowroomPositions[(int)Type];

                var product = Products[index];
                var vehicle = ENet.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(product.Name), config.Testdrive, config.TestdriveHeading, 0, 0, "TESTDRIVE", 255, true, true, player.Dimension);
                vehicle.SetType(VehicleType.Testdrive);

                var primColor = Configs.BusinessConfig.Colors[primaryColor];
                var secColor = Configs.BusinessConfig.Colors[secondaryColor];

                vehicle.PrimaryColor = primColor.ColorId;
                vehicle.SecondaryColor = secColor.ColorId;

                player.SetIntoVehicle(vehicle, (int)VehicleSeat.Driver);
                player.SetData("testdrive.vehicle", vehicle);

                player.SetData("testdrive.timer", Timers.StartOnce(Configs.BusinessConfig.TestdriveTime * 1000, () =>
                {
                    try
                    {
                        if (player is null) return;

                        EndTestdrive(player);
                        player.SendInfo(Language.GetText(TextType.TestdriveEnded));
                    }
                    catch (Exception ex) { Logger.WriteError("Testdrive.timer", ex); }
                }));

                ClientEvent.Event(player, "client.timer.add", "TESTDRIVE", Language.GetText(TextType.Testdrive), Configs.BusinessConfig.TestdriveTime);
                ClientEvent.Event(player, "client.business.showroom.testdrive.ready");
            }
            catch (Exception ex) { Logger.WriteError("StartTestdrive", ex); }
        }

        private Position GetSpawnPosition()
        {
            try
            {
                var config = Configs.BusinessConfig.ShowroomPositions[(int)Type];
                int index = ENet.Random.Next(0, config.SpawnPositions.Count);

                return config.SpawnPositions[index];
            }
            catch (Exception ex) { Logger.WriteError("GetSpawnPosition", ex); return null; }
        }
    }
}
