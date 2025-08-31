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
using System.Threading.Tasks;

namespace eNetwork.Businesses
{
    public class Ammunation : Business
    {
        private static readonly Logger Logger = new Logger("ammunation");

        private static async void OnOpen(ENetPlayer player, params object[] arguments)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData data) || !player.IsTimeouted("events.biz.ammunation.open", 1) || !player.GetData("CURRENT_PED_BUSINESS", out Ammunation ammunation) || !player.HasData("BUSINESS_POSITION_INDEX")) return;

                int index = player.GetData<int>("BUSINESS_POSITION_INDEX");
                Vector3 position = ammunation.Positions.ElementAt(index);

                if (position is null) return;

                Transition.Open(player, "Достаем снаряжение со склада");
                player.SetData("CURRENT_BUSINESS", ammunation);
                await Task.Delay(600);

                Dialogs.Close(player, 0);
                ClientEvent.Event(player, "client.business.ammunation.open", ammunation.Type.ToString(), 0, ammunation.Markup);
            }
            catch (Exception ex) { Logger.WriteError("Open", ex); }
        }

        [CustomEvent("server.business.ammunation.close")]
        private static void Event_Close(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData data) || !player.IsTimeouted("events.biz.ammunation.close", 1)) return;

                Transition.Close(player);
                player.ResetData("CURRENT_BUSINESS");
            }
            catch (Exception ex) { Logger.WriteError("Close", ex); }
        }

        [CustomEvent("server.business.ammunation.buy")]
        private static void Event_Buy(ENetPlayer player, string ItemId, int tintIndex, string payType)
        {
            try
            {
                if (!player.IsTimeouted("event.biz.ammunation.buy", 1) || !player.GetData<Ammunation>("CURRENT_BUSINESS", out Ammunation ammunation)) return;
                ammunation.Buy(player, ItemId, tintIndex, payType);
            }
            catch (Exception ex) { Logger.WriteError("Event_SelectModel", ex); }
        }

        public static void LoadConfig(ENetPlayer player)
        {
            try
            {
                var categories = Businesses.Products.Ammunations.GetCatregories();
                ClientEvent.Event(player, "client.business.ammunation.products.load", JsonConvert.SerializeObject(categories));
            }
            catch (Exception ex) { Logger.WriteError("Load", ex); }
        }

        public Ammunation(int id, BusinessType type) : base(id, type)
        {
            BlipType = 110;
            BlipColor = 4;
            Name = "Амуниция";
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
                    ID = (int)DialogType.Showroom,
                    Name = "Евгений Пригожин",
                    Description = $"Оружейный мастер",
                    Text = $"Добро пожаловать в Аммуницию, {player.GetName()}! Будем собирать сегодня новый комплект снаряжения?",
                    Answers = new List<DialogAnswer>() {
                        new DialogAnswer("Да, хочу посмотреть!", OnOpen, "open"),
                        new DialogAnswer(Language.GetText(TextType.NoThanks), null, "close")
                    },
                };

                dialog.Open(player, Ped);
            }
            catch (Exception ex) { Logger.WriteError("InteractionNpc", ex); }
        }

        public override void Initialize()
        {
            PedHash = (uint)GTANetworkAPI.PedHash.Ammucity01SMY;
            InteractionPedText = "Нажмите чтобы поговорить с Пригожиным";

            GTAElements();
            CreateBlip(null);
        }

        public void Buy(ENetPlayer player, string _itemId, int tintIndex, string payType)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;

                var product = Products.Find(x => x.Name == _itemId);

                var productData = Ammunations.GetProduct(Type, _itemId);
                if (product is null || productData is null)
                {
                    player.SendError("Не удалось приобрести данный предмет");
                    return;
                }

                int price = tintIndex == 0 ? product.GetPrice(this) : Convert.ToInt32(Math.Floor(product.GetPrice(this) * 2.5));

                if (payType == "Card")
                {
                    // TODO: Card Payment
                }
                else
                {
                    if (characterData.Cash < price)
                    {
                        player.SendError("Недостаточно средств");
                        return;
                    }
                }

                ItemId itemId = Enum.Parse<ItemId>(_itemId);
                var itemData = InvItems.Get(itemId);
                if (itemData is null) return;

                int gunshopId = BusinessManager.GetIndexBussinesOfType(Type, ID);

                //dynamic _data = null;
                //if (itemData.ItemType == ItemType.Weapon)
                //    _data = JsonConvert.SerializeObject(new InvWeaponData(100, WeaponController.GenerateSerial(FactionId.None, gunshopId + 1), new WeaponComponentsData() { TintIndex = tintIndex }));
                string _data = "";
                if (itemData.ItemType == ItemType.Weapon)
                    _data = JsonConvert.SerializeObject(
                        new { 
                            Endurance = 100, 
                            Serial = WeaponController.GenerateSerial(FactionId.None, gunshopId + 1), 
                            Components = new WeaponComponentsData() { TintIndex = tintIndex } 
                        }
                    );
                
                //_data = JsonConvert.SerializeObject(new InvWeaponData(100, WeaponController.GenerateSerial(FactionId.None, gunshopId + 1), new WeaponComponentsData() { TintIndex = tintIndex }));

                var item = TypedItems.Get(new Item(itemId, productData.Count, dataStr: _data));  //new Item(itemId, productData.Count, dataStr: _data);

                if (!Inventory.CheckCanAddItem(player, item, -1))
                {
                    player.SendError(Language.GetText(TextType.NoInventoryPlaces));
                    return;
                }

                if (!TakeProduct(productData.Count, _itemId, price))
                {
                    player.SendError("На складе закончились предметы данного типа");
                    return;
                }

                if (payType == "Card")
                {
                    // TODO: Card Payment
                }
                else
                {
                    player.ChangeWallet(-price);
                }

                _ = Inventory.CreateNewItem(player, item);
                player.SendDone($"Вы купили {itemData.Name} за {Helper.FormatPrice(price)}$");
            }
            catch (Exception ex) { Logger.WriteError("Buy", ex); }
        }
    }
}
