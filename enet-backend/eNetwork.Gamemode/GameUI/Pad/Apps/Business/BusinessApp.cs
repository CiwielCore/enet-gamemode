using eNetwork.Businesses;
using eNetwork.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.GameUI.Pad.Apps.Business
{
    public class BusinessApp
    {
        private static readonly Logger Logger = new Logger(nameof(BusinessApp));

        public static void On(ENetPlayer player)
        {
            try
            {
                var business = BusinessManager.GetPlayerBusiness(player);
                if (business is null)
                {
                    player.SendError("У вас нет бизнеса");
                    return;
                }

                var data = new
                {
                    Id = business.ID,
                    Name = business.GetName(),
                    Products = business.Products,
                    Markup = business.Markup,
                    business.Price,
                    business.Earning,
                    business.Tax
                };

                PadManager.OpenApp(player, "business", JsonConvert.SerializeObject(data));
            }   
            catch(Exception ex) { Logger.WriteError("On", ex); }
        }

        [CustomEvent("server.ipad.business.sell")]
        public static void SellBusiness(ENetPlayer player)
        {
            try
            {
                var business = BusinessManager.GetPlayerBusiness(player);
                if (business is null)
                {
                    player.SendError("У вас нет бизнеса");
                    PadManager.Close(player);
                    return;
                }

                int sellPrice = Convert.ToInt32(business.Price * .5);
                player.ChangeWallet(sellPrice);

                business.Owner = -1;
                player.SendDone($"Вы продали бизнес за {Helper.FormatPrice(sellPrice)}$");

                PadManager.Close(player);
            }
            catch(Exception ex) { Logger.WriteError("SellBusiness", ex); }
        }

        [CustomEvent("server.ipad.business.takeEarning")]
        public static void TakeEarning(ENetPlayer player)
        {
            try
            {
                var business = BusinessManager.GetPlayerBusiness(player);
                if (business is null)
                {
                    player.SendError("У вас нет бизнеса");
                    PadManager.Close(player);
                    return;
                }

                if (business.Earning <= 0)
                {
                    player.SendInfo("Вы ничего не заработали!");
                    return;
                }

                player.ChangeWallet(business.Earning);
                player.SendDone($"Вы сняли с бизнеса - {Helper.FormatPrice(Convert.ToInt32(business.Earning))}");

                business.Earning = 0;

                On(player);
            }
            catch(Exception ex) { Logger.WriteError("TakeEarning", ex); }
        }

        [CustomEvent("server.ipad.business.buyProduct")]
        public static void BuyProduct(ENetPlayer player, string productName, int count)
        {
            try
            {
                var business = BusinessManager.GetPlayerBusiness(player);
                if (business is null)
                {
                    player.SendError("У вас нет бизнеса");
                    PadManager.Close(player);
                    return;
                }

                business.BuyProduct(player, productName, count);
                On(player);
            }
            catch (Exception ex) { Logger.WriteError("BuyProduct", ex); }
        }

        [CustomEvent("server.ipad.business.changeMarkup")]
        public static void ChangeMarkup(ENetPlayer player, int markup)
        {
            try
            {
                var business = BusinessManager.GetPlayerBusiness(player);
                if (business is null)
                {
                    player.SendError("У вас нет бизнеса");
                    PadManager.Close(player);
                    return;
                }

                if (markup > 250)
                {
                    player.SendError($"Невозможно установить наценку больше чем 250%");
                    return;
                }

                business.Markup = markup;
                On(player);

                player.SendDone($"Вы установили наценку {markup}%");
            }
            catch (Exception ex) { Logger.WriteError("ChangeMarkup", ex); }
        }
    }
}
