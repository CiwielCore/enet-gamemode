using eNetwork.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using eNetwork.Game.Vehicles;
using eNetwork.Framework.Classes;
using Newtonsoft.Json;
using System.Linq;
using eNetwork.Framework.Enums;
using System.Diagnostics;

namespace eNetwork.Businesses
{
    public class PetrolStation : Business
    {
        private static readonly Logger Logger = new Logger("petrol-station");

        [CustomEvent("server.business.petrol.buy.gas")]
        private static void Event_BuyGaz(ENetPlayer player, string petrolType, int count, string payType)
        {
            if (!player.IsTimeouted("event.biz.buyGaz", 1) || !player.GetData<PetrolStation>("CURRENT_BUSINESS", out PetrolStation petrol)) return;
            petrol.BuyGas(player, petrolType, count, payType);
        }

        [CustomEvent("server.business.petrol.exit")]
        private static void Event_Exit(ENetPlayer player)
        {
            try
            {
                if (!player.GetData<PetrolStation>("CURRENT_BUSINESS", out PetrolStation petrol)) return;
                player.ResetData("CURRENT_BUSINESS");
            }
            catch (Exception ex) { Logger.WriteError("Event_Exit", ex); }
        }

        public PetrolStation(int id, BusinessType type) : base(id, type)
        {
            BlipType = 361;
            BlipColor = 6;
            Name = "Заправочная станция";
            ShapeRadius = 3;
            ShapeHeight = 2;

            InteractionText = "Нажмите чтобы заправиться";
        }

        public override void Initialize()
        {
            string petrols = "";
            foreach (BizProduct product in Products)
            {
                if (product.Count > 0 && !product.Disable)
                    petrols += $"- {product.Name}<br>";
            }

            GTAElements();
            CreateBlip(new BlipInformation()
            {
                Name = Name,
                Description = Language.GetText(TextType.PetrolDescription),
                Type = BlipInfoType.Business.ToString(),
                Picture = "xerogas",
                ExtraData = Language.GetText(TextType.EnablePetrolTypes) + ":<div class=\"bi_list\">" + petrols + "</div>",
            });
        }

        public override void InteractionBiz(ENetPlayer player)
        {
            try
            {
                if (!player.IsInVehicle || player.VehicleSeat != (int)VehicleSeat.Driver) return;
                ENetVehicle vehicle = (ENetVehicle)player.Vehicle;

                if (!vehicle.HasSharedData("max.petrol") || !vehicle.HasSharedData("petrol")) return;

                int maxPetrol = Convert.ToInt32(vehicle.GetSharedData<float>("max.petrol"));
                int petrol = Convert.ToInt32(vehicle.GetSharedData<float>("petrol"));

                var data = new List<object>();
                foreach (BizProduct product in Products)
                {
                    if (product.Count > 0 && !product.Disable)
                    {
                        data.Add(new
                        {
                            Type = GetProdType(product.Name)[0],
                            EnumType = GetProdType(product.Name)[1].ToString(),
                            Name = product.Name,
                            Price = product.GetPrice(this),
                        });
                    }
                }

                if (data.Count == 0)
                {
                    player.SendError(Language.GetText(TextType.PetrolStationDoesntWork));
                    return;
                }

                vehicle.EngineState(false);

                ClientEvent.Event(player, "client.business.petrol.open",
                    JsonConvert.SerializeObject(data),
                    maxPetrol,
                    petrol
                );
            }
            catch (Exception ex) { Logger.WriteError("Interaction", ex); }
        }

        private object[] GetProdType(string name)
        {
            switch (name)
            {
                case "92 Топливо": return new object[] { "92", PetrolType.P92 };
                case "95 Топливо": return new object[] { "95", PetrolType.P95 };
                case "98 Топливо": return new object[] { "98", PetrolType.P98 };
                case "100 Топливо": return new object[] { "100", PetrolType.P100 };
                case "Дизельное топливо": return new object[] { "D", PetrolType.Diesel };
                default: return new object[] { "", "" };
            }
        }

        private BizProduct GetProductByType(PetrolType petrolType)
        {
            string prodName = "";
            switch (petrolType)
            {
                case PetrolType.P92:
                    prodName = "92 Топливо";
                    break;
                case PetrolType.P95:
                    prodName = "95 Топливо";
                    break;
                case PetrolType.P98:
                    prodName = "98 Топливо";
                    break;
                case PetrolType.P100:
                    prodName = "100 Топливо";
                    break;
                case PetrolType.Diesel:
                    prodName = "Дизельное топливо";
                    break;
                case PetrolType.Electro:
                    prodName = "Электричество";
                    break;
            }

            return Products.Find(x => x.Name == prodName);
        }

        public void BuyGas(ENetPlayer player, string petrolType, int count, string payType)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData character) || !player.IsInVehicle || count <= 0) return;
                ENetVehicle vehicle = (ENetVehicle)player.Vehicle;

                VehicleConfig vehicleConfig = VehicleSync.GetVehicleConfig(vehicle);
                if (vehicleConfig is null) return;

                int currentPetrol = Convert.ToInt32(vehicle.GetPetrol());
                int maxPetrol = vehicleConfig.MaxFuel;

                if (currentPetrol == maxPetrol)
                {
                    player.SendInfo(Language.GetText(TextType.CarHaveFullPetrol));
                    return;
                }

                PetrolType pType = (PetrolType)Enum.Parse(typeof(PetrolType), petrolType);
                var product = GetProductByType(pType);
                if (product is null)
                {
                    player.SendError("Ошибка покупки топлива");
                    return;
                }

                int totalPrice = product.GetPrice(this) * count;
                if (character.Cash < totalPrice)
                {
                    player.SendError("Недостаточно средств");
                    return;
                }

                if (!TakeProduct(count, product.Name, totalPrice))
                {
                    player.SendError("На складе закончились предметы данного типа");
                    return;
                }

                player.ChangeWallet(totalPrice);

                float newPetrol = currentPetrol + count > maxPetrol ? maxPetrol : currentPetrol + count;
                vehicle.SetSharedData("petrol", newPetrol);
                player.SendDone(Language.GetText(TextType.YouPetrolingVehicle, count));
            }
            catch (Exception e) { Logger.WriteError("BuyGas", e); }
        }
    }
}
