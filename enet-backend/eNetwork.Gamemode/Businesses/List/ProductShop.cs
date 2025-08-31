using eNetwork.Businesses.Models;
using eNetwork.Businesses.Products;
using eNetwork.Framework;
using eNetwork.Framework.Enums;
using eNetwork.Gambles.Lotteries;
using eNetwork.Game;
using eNetwork.GameUI;
using eNetwork.Inv;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Businesses.List
{
    public class ProductShop : Business
    {
        public static event EventHandler<BuyProductArgs> BuyProductEvent;

        private static readonly Logger Logger = new Logger("shop24");

        private static async void OnOpen(ENetPlayer player, params object[] arguments)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData data) || !player.GetData("CURRENT_PED_BUSINESS", out ProductShop shop24) || !player.HasData("BUSINESS_POSITION_INDEX")) return;

                int index = player.GetData<int>("BUSINESS_POSITION_INDEX");
                Vector3 position = shop24.Positions.ElementAt(index);

                if (position is null) return;

                Transition.Open(player, "Достаем булочку с сосиской");
                player.SetData("CURRENT_BUSINESS", shop24);
                await Task.Delay(600);

                Transition.Close(player);

                Dialogs.Close(player, 0);
                ClientEvent.Event(player, "client.business.shop24.open", shop24.Markup);
            }
            catch (Exception ex) { Logger.WriteError("Open", ex); }
        }

        [CustomEvent("server.business.shop24.close")]
        private static void Event_Close(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData data)) return;

                Transition.Close(player);
                player.ResetData("CURRENT_BUSINESS");
            }
            catch (Exception ex) { Logger.WriteError("Close", ex); }
        }

        [CustomEvent("server.business.shop24.buy")]
        private static void Event_Buy(ENetPlayer player, string category, int itemId)
        {
            try
            {
                if (!player.GetData("CURRENT_BUSINESS", out ProductShop shop24)) return;

                shop24.Buy(player, category, itemId);
            }
            catch (Exception ex) { Logger.WriteError("Event_SelectModel", ex); }
        }

        public static void LoadConfig(ENetPlayer player)
        {
            try
            {
                var categories = Shops24.GetCatregories()[BusinessType.Shop24];
                ClientEvent.Event(player, "client.business.shop24.products.load", JsonConvert.SerializeObject(categories));
            }
            catch (Exception ex) { Logger.WriteError("Load", ex); }
        }

        public ProductShop(int id, BusinessType type) : base(id, type)
        {
            BlipType = 52;
            BlipColor = 4;
            Name = "Магазин 24/7";
            ShapeRadius = 1.25f;
            ShapeHeight = 2;
        }

        public override void InteractionNpc(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData)) return;

                var dialog = new Dialog()
                {
                    ID = 12,
                    Name = "Тётя зина",
                    Description = $"Продавец",
                    Text = $"Добро пожаловать в Магазин 24/7, {player.GetName()}! Хотите что-то преобрести?",
                    Answers = new List<DialogAnswer>() {
                        new DialogAnswer("Да", OnOpen, "open"),
                        new DialogAnswer(Language.GetText(TextType.NoThanks), null, "close")
                    },
                };

                dialog.Open(player, Ped);
            }
            catch (Exception ex) { Logger.WriteError("InteractionNpc", ex); }
        }

        public override void Initialize()
        {
            PedHash = (uint)GTANetworkAPI.PedHash.ShopLowSFY;
            InteractionPedText = "Нажмите чтобы поговорить с Тетей зиной";

            GTAElements();
            CreateBlip(null);
        }

        public void Buy(ENetPlayer player, string category, int itemIndex)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;

                var productData = Shops24.GetCatregories()[BusinessType.Shop24][category][itemIndex];
                var product = Products.Find(x => x.Name == productData.Name);

                if (product is null || productData is null)
                {
                    player.SendError("Не удалось приобрести данный предмет");
                    return;
                }

                int price = productData.Price;

                if (characterData.Cash < price)
                {
                    player.SendError("Недостаточно средств");
                    return;
                }

                ItemId itemId = Enum.Parse<ItemId>(productData.Item);

                var itemData = InvItems.Get(itemId);
                if (itemData is null) return;

                var item = TypedItems.Get(new Item(itemId, productData.Count));

                if (!Inventory.CheckCanAddItem(player, item, -1))
                {
                    player.SendError(Language.GetText(TextType.NoInventoryPlaces));
                    return;
                }

                if (!TakeProduct(productData, product.GetPrice(this)))
                {
                    player.SendError("На складе закончились предметы данного типа");
                    return;
                }

                player.ChangeWallet(-price);

                _ = Inventory.CreateNewItem(player, item);
                player.SendDone($"Вы купили {itemData.Name} за {Helper.FormatPrice(price)}$");

                BuyProductEvent?.Invoke(player, new BuyProductArgs(itemId, price));
            }
            catch (Exception ex) { Logger.WriteError("Buy", ex); }
        }

        private bool TakeProduct(Shops24.Product product, int price)
        {
            ItemId item = Enum.Parse<ItemId>(product.Item);
            bool haveInStore = TakeProduct(product.Count, product.Name, price);
            bool canTake = haveInStore && item == ItemId.LotteryTicket ? Lottery.Instance.TakeTicket() : true;
            return canTake;
        }
    }
}
