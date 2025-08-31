using eNetwork.Framework;
using eNetwork.Framework.Classes.Character.Customization;
using eNetwork.Framework.Configs.Tattoo.Enums;
using eNetwork.Framework.Enums;
using eNetwork.Game;
using eNetwork.Game.Banks.Player;
using eNetwork.GameUI;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Businesses.List
{
    public class TattooShop : Business
    {
        private static readonly Logger Logger = new Logger("tattoo-shop");

        private static readonly Position TATTOO_SHOPS_POSITIONS = new Position(323.9927, 180.26501, 103.58648, 95);
        
        public class Manager
        {
            public static void Initialize()
            {
                try
                {
                    string url = @"client_packages\web\views\Businesses\Tattoo\data.js";
                    File.WriteAllText(url, string.Empty);

                    using (var file = new StreamWriter(url, true, Encoding.UTF8))
                    {
                        file.Write("const tattooConfig = ");
                        file.Write(JsonConvert.SerializeObject(Framework.Configs.Tattoo.TattooConfig.Tattoos));

                        file.Close();
                    }
                }
                catch (Exception ex) { Logger.WriteError("Initialize", ex); }
            }

            [CustomEvent("server.business.tattoo_shop.buy")]
            private void OnBuy(ENetPlayer player, string currentCategory, int currentProd, string paymentType)
            {
                if (!player.GetData<TattooShop>("CURRENT_BUSINESS", out var tattooShop)) return;
                tattooShop.Buy(player, currentCategory, currentProd, paymentType);
            }

            [CustomEvent("server.business.tattoo_shop.close")]
            private void OnClose(ENetPlayer player)
            {
                if (!player.GetData<TattooShop>("CURRENT_BUSINESS", out var tattooShop)) return;
                tattooShop.Close(player);
            }
        }

        public TattooShop(int id, BusinessType type) : base (id, type)
        {
            BlipType = 75;
            BlipColor = 4;
            Name = "Татуировки";
            ShapeRadius = 2;
            ShapeHeight = 2;

            PedHash = NAPI.Util.GetHashKey("u_m_y_tattoo_01");
            InteractionPedText = "Посмотреть каталог";
        }

        public override void Initialize()
        {
            GTAElements();
            CreateBlip(null);

            CreatePed();
        }

        public override async void InteractionNpc(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) 
                    return;

                Transition.Open(player, "Открываем каталог...");

                await NAPI.Task.WaitForMainThread();

                characterData.ExteriosPosition = player.Position;
                TATTOO_SHOPS_POSITIONS.Set(player);

                player.SetDimension(DimensionHandler.RequestPrivateDimension(player));
                player.SetData("CURRENT_BUSINESS", this);

                ClientEvent.Event(player, "client.business.tattoo_shop.open", player.Gender.ToString(), Markup);
            }
            catch(Exception ex) { Logger.WriteError("InteractionNpc", ex); }
        }

        public void Buy(ENetPlayer player, string currentCategory, int currentProd, string paymentType)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;

                TattooZone tattooZone = (TattooZone)Enum.Parse(typeof(TattooZone), currentCategory);
                if (!Framework.Configs.Tattoo.TattooConfig.Tattoos.TryGetValue(tattooZone, out var tattooList)) 
                    return;

                var tattooProduct = tattooList.ElementAt(currentProd);
                if (tattooProduct is null) return;

                int price = GetPrice(tattooProduct.Price);

                if (paymentType == "Cash")
                {
                    if (characterData.Cash < price)
                    {
                        player.SendError("Недостаточно средств");
                        return;
                    }
                }
                else
                {
                    if (!player.GetBankAccount(out var bankAccountData) || bankAccountData.Balance < price)
                    {
                        player.SendError("Недостаточно средств");
                        return;
                    }
                }

                if (!TakeProduct(1, "Краска", price))
                {
                    player.SendError("Этот тату-шоп не может оказать эту услугу в данный момент");
                    return;
                }

                if (paymentType == "Cash")
                {
                    player.ChangeWallet(-price);
                }
                else
                {
                    player.ChangeBank(-price);
                }

                characterData.CustomizationData.Tattoos[tattooZone].Add(new PlayerTattooData(tattooProduct.Collection, characterData.CustomizationData.Gender == Gender.Male ? tattooProduct.OverlayMale : tattooProduct.OverlayFemale));
                player.ApplyCustomization();

                player.SendDone($"Вы купили тату!");
            }
            catch(Exception ex) { Logger.WriteError("Buy", ex); }
        }

        public async void Close(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData))
                    return;

                Transition.Open(player, "Выходим из тату салона...");

                await NAPI.Task.WaitForMainThread();

                player.SetDimension(0);
                player.ResetData("CURRENT_BUSINESS");

                if (characterData.ExteriosPosition != null)
                    player.Position = characterData.ExteriosPosition;
                
                characterData.ExteriosPosition = null;

                ClientEvent.Event(player, "client.business.tattoo_shop.close");
            }
            catch(Exception ex) { Logger.WriteError("Close", ex); }
        }
    }
}
