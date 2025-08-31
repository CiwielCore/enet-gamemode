using eNetwork.Businesses.List;
using eNetwork.Businesses.Products;
using eNetwork.Framework;
using eNetwork.Framework.Enums;
using eNetwork.Game;
using eNetwork.GameUI;
using eNetwork.Inv;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eNetwork.Businesses
{
    public class ClothesShop : Business
    {
        private static readonly Logger Logger = new Logger("clothes-shop");

        private async void OnOpen(ENetPlayer player, params object[] arguments)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData data) || !player.GetData("CURRENT_PED_BUSINESS", out ClothesShop clothesShop) || !player.HasData("BUSINESS_POSITION_INDEX")) return;

                int index = player.GetData<int>("BUSINESS_POSITION_INDEX");
                Vector3 position = clothesShop.Positions.ElementAt(index);

                if (position is null) return;

                Transition.Open(player, "Достаем гуччи луи прада прада");
                player.SetData("CURRENT_BUSINESS", clothesShop);
                await Task.Delay(600);

                Transition.Close(player);

                Dialogs.Close(player, 0);
                player.SetDimension(9999);
                ClientEvent.Event(player, "client.business.clothesShop.open", JsonConvert.SerializeObject(ClothesShops.GetPositions(ID)), clothesShop.Markup);
            }
            catch (Exception ex) { Logger.WriteError("Open", ex); }
        }

        [CustomEvent("server.business.clothesShop.close")]
        private static void Event_Close(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData data)) return;

                Transition.Close(player);
                player.ResetData("CURRENT_BUSINESS");
                player.SetDimension(0);
                player.ApplyCustomization();
            }
            catch (Exception ex) { Logger.WriteError("Close", ex); }
        }

        [CustomEvent("server.business.clothesShop.buy")]
        private static void Event_Buy(ENetPlayer player, int itemId, int texture)
        {
            try
            {
                if (!player.GetData("CURRENT_BUSINESS", out ClothesShop clothesShop)) return;

                clothesShop.Buy(player, itemId, texture);
            }
            catch (Exception ex) { Logger.WriteError("Event_SelectModel", ex); }
        }

        public static void LoadConfig(ENetPlayer player)
        {
            try
            {
                var categories = ClothesShops.GetCatregories()[BusinessType.ClothesShop];
                ClientEvent.Event(player, "client.business.clothesShop.products.load", JsonConvert.SerializeObject(categories));
            }
            catch (Exception ex) { Logger.WriteError("Load", ex); }
        }

        public ClothesShop(int id, BusinessType type) : base(id, type)
        {
            BlipType = 73;
            BlipColor = 4;
            Name = "Магазин одежды";
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
                    Name = "Консультант",
                    Description = $"Продавец",
                    Text = $"Добро пожаловать в Магазин одежды, {player.GetName()}! Хотите что-то приобрести?",
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
            PedHash = (uint)GTANetworkAPI.PedHash.ShopMidSFY;
            InteractionPedText = "Нажмите чтобы поговорить с консультантом";

            GTAElements();
            CreateBlip(null);
        }

        public void Buy(ENetPlayer player, int itemIndex, int texture)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;

                var productData = ClothesShops.GetCatregories()[BusinessType.ClothesShop][itemIndex];
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

                ItemId itemId = productData.Item;

                //var itemData = InvItems.Get(itemId);
                //if (itemData is null) return;

                productData.InvClothesData.Texture = texture;
                //Item item = new Item(itemId, productData.Count, data: JsonConvert.SerializeObject(productData.InvClothesData));


                // TO DO

                var item = TypedItems.Get(new Item(itemId, productData.Count, JsonConvert.SerializeObject(new { ClotheId = 1 })));
                // new Inv.Items.ClotheItem(itemId, productData.InvClothesData, productData.Count);

                if (!Inventory.CheckCanAddItem(player, item, -1))
                {
                    player.SendError(Language.GetText(TextType.NoInventoryPlaces));
                    return;
                }

                if (!TakeProduct(productData.Count, productData.Name, price))
                {
                    player.SendError("На складе закончились предметы данного типа");
                    return;
                }

                player.ChangeWallet(-price);

                _ = Inventory.CreateNewItem(player, item);
                player.SendDone($"Вы купили {productData.Name} за {Helper.FormatPrice(price)}$");

                Game.Quests.QuestTasksHandler.Instance.AddProgressToTask(player, Game.Quests.QuestTaskId.BuyClothes);
            }
            catch (Exception ex) { Logger.WriteError("Buy", ex); }
        }
    }
}
