using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eNetwork.Framework;
using eNetwork.Framework.Enums;
using eNetwork.Game;
using eNetwork.GameUI;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace eNetwork.Businesses
{
    public class BarberShop : Business
    {
        private static readonly Logger Logger = new Logger("barber-shop");

        [CustomEvent("server.business.barberShop.buy")]
        private static void Event_Buy(ENetPlayer player, int type, int texture, int color, int hcolor, string payType)
        {
            try
            {
                if (!player.IsTimeouted("event.biz.barber.buy", 1) || !player.GetData<BarberShop>("CURRENT_BUSINESS", out BarberShop barberShop)) return;
                barberShop.Buy(player, type, texture, color, hcolor, payType);
            }
            catch (Exception ex) { Logger.WriteError("Event_SelectModel", ex); }
        }

        [CustomEvent("server.business.barberShop.close")]
        private static void Event_Close(ENetPlayer player)
        {
            try
            {
                if (!player.IsTimeouted("event.biz.barber.close", 1) || !player.GetData<BarberShop>("CURRENT_BUSINESS", out BarberShop barberShop)) return;
                barberShop.Close(player);
            }
            catch (Exception ex) { Logger.WriteError("Event_SelectModel", ex); }
        }

        private static async void OnOpen(ENetPlayer player, params object[] arguments)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData data) || !player.IsTimeouted("events.biz.barber.open", 1) || !player.GetData("CURRENT_PED_BUSINESS", out BarberShop barberShop) || !player.HasData("BUSINESS_POSITION_INDEX")) return;

                int index = player.GetData<int>("BUSINESS_POSITION_INDEX");
                Vector3 position = barberShop.Positions.ElementAt(index);

                if (position is null) return;

                int barber = GetBarberType(player.Position);
                if (barber == -1)
                {
                    player.SendError("Данный барбершоп не работает");
                    return;
                }

                Transition.Open(player, "Садим вас в кресло");

                await Task.Delay(600);

                Dialogs.Close(player, 0);

                player.SetDimension(DimensionHandler.RequestPrivateDimension(player));
                player.SetData("CURRENT_BUSINESS", barberShop);

                NAPI.Task.Run(() => data.ExteriosPosition = player.Position);

                BarberShopType barberShopType = GetTypByPositionBarber(barberShop.Positions[0]);
                string barbershopName = barberShopType == BarberShopType.HerrKutz ? "Herr Kutz" : barberShopType == BarberShopType.BobMulet ? "Bob Mulet" : "Undefined Name";

                ClientEvent.Event(player, "client.business.barberShop.open", barbershopName, barber, JsonConvert.SerializeObject(GetCategoryByGender(data.CustomizationData.Gender)), barberShop.Markup);
            }
            catch (Exception ex) { Logger.WriteError("InteractionBiz", ex); }
        }

        public BarberShop(int id, BusinessType type) : base(id, type)
        {
            BlipType = 71;
            BlipColor = 4;
            Name = "Барбершоп";
            ShapeRadius = 2;
            ShapeHeight = 2;
        }

        public override void InteractionNpc(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData)) return;

                BarberShopType barberShopType = GetTypByPositionBarber(Positions[0]);
                string barbershopName = barberShopType == BarberShopType.HerrKutz ? "Herr Kutz" : barberShopType == BarberShopType.BobMulet ? "Bob Mulet" : "Барбершоп";

                var dialog = new Dialog()
                {
                    ID = (int)DialogType.Showroom,
                    Name = barberShopType == BarberShopType.BobMulet ? "Боб Мулет" : "Келли Берч",
                    Description = $"Парикмахер",
                    Text = $"Добро пожаловать в {barbershopName}, {player.GetName()}! Какую прическу будем делать сегодня?",
                    Answers = new List<DialogAnswer>() {
                        new DialogAnswer("Хочу воспользоваться вашими услугами", OnOpen, "open"),
                        new DialogAnswer(Language.GetText(TextType.NotNowDay), null, "close")
                    },
                };

                dialog.Open(player, Ped);
            }
            catch (Exception ex) { Logger.WriteError("InteractionNpc", ex); }
        }

        public override void Initialize()
        {
            BarberShopType barberShopType = GetTypByPositionBarber(Positions[0]);
            InteractionPedText = "Нажмите чтобы поговорить с " + (barberShopType == BarberShopType.BobMulet ? "Боб Мулет" : "Келли Берч");

            PedHash = GetPedHash(Positions[0]);
            GTAElements();

            CreateBlip(null);
        }

        public void Buy(ENetPlayer player, int type, int texture, int color, int hcolor, string payType)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;

                var item = GetBarberArrayByType(type).FirstOrDefault(x => x.Texture == texture);
                if (item == null) { player.SendError("Ошибка покупки"); return; }

                var prod = Products.Find(p => p.Name == "Расходники");
                int price = prod.GetPrice(this);

                if (payType == "Cash")
                {
                    if (characterData.Cash < price)
                    {
                        player.SendError("Недостаточно средств");
                        return;
                    }
                }
                else
                {
                    // TODO: Card Payment
                }

                if (!TakeProduct(1, "Расходники", price))
                {
                    player.SendError("Этот барбер-шоп не может оказать эту услугу в данный момент");
                    return;
                }

                player.ApplyCustomization();

                switch (type)
                {
                    case 0:
                    case 6:
                        characterData.CustomizationData.Hair = new HairData(texture, color, hcolor);
                        break;
                    case 1:
                        characterData.CustomizationData.Appearance[2].Value = texture;
                        characterData.CustomizationData.otherCustomize.EyeBrowColor = color;
                        break;
                    case 2:
                        characterData.CustomizationData.Appearance[1].Value = texture;
                        characterData.CustomizationData.otherCustomize.BreadColor = color;
                        break;
                    case 3:
                        characterData.CustomizationData.Appearance[10].Value = texture;
                        characterData.CustomizationData.otherCustomize.ChestHairColor = color;
                        break;
                    case 4:
                        characterData.CustomizationData.otherCustomize.EyeColor = texture;
                        break;
                    case 5:
                        characterData.CustomizationData.Appearance[4].Value = texture;
                        break;
                    case 7:
                        characterData.CustomizationData.Appearance[8].Value = texture;
                        characterData.CustomizationData.otherCustomize.LipstickColor = color;
                        break;
                    case 8:
                        characterData.CustomizationData.Appearance[5].Value = texture;
                        characterData.CustomizationData.otherCustomize.BlushColor = color;
                        break;
                }

                ClientEvent.Event(player, "client.business.barberShop.animation");

                if (payType == "Cash")
                {
                    player.ChangeWallet(-price);
                }
                else
                {
                    // TODO: Card Payment
                }

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player != null)
                            player.ApplyCustomization();
                    }
                    catch (Exception ex) { Logger.WriteError("Buy.Task", ex); }
                }, 2000);
            }
            catch (Exception ex) { Logger.WriteError("Buy", ex); }
        }

        public void Close(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;
                player.Position = characterData.ExteriosPosition;
                characterData.ExteriosPosition = null;
                player.ResetData("CURRENT_BUSINESS");

                player.Dimension = 0;
                player.ApplyCustomization();

                ClientEvent.Event(player, "client.business.barberShop.reset");
            }
            catch (Exception ex) { Logger.WriteError("Close", ex); }
        }

        #region Получение типа NPC
        private static uint GetPedHash(Vector3 position)
        {
            BarberShopType barber = GetTypByPositionBarber(position);
            switch (barber)
            {
                case BarberShopType.BobMulet:
                    return 0x418DFF92;
                case BarberShopType.HerrKutz:
                    return 0x163B875B;

                default: return 0;
            }
        }

        private static BarberShopType GetTypByPositionBarber(Vector3 pos)
        {
            if (pos.DistanceTo(new Vector3(-817.9705, -184.5802, 36.56892)) < 10) return BarberShopType.BobMulet;
            if (pos.DistanceTo(new Vector3(139.6911, -1706.982, 28.29159)) < 10) return BarberShopType.HerrKutz;
            if (pos.DistanceTo(new Vector3(-1280.183, -1118.613, 5.99159)) < 10) return BarberShopType.HerrKutz;
            if (pos.DistanceTo(new Vector3(1931.628, 3733.236, 31.84159)) < 10) return BarberShopType.HerrKutz;
            if (pos.DistanceTo(new Vector3(1214.553, -474.7919, 65.20159)) < 10) return BarberShopType.HerrKutz;
            if (pos.DistanceTo(new Vector3(-35.2074, -154.372, 56.07159)) < 10) return BarberShopType.HerrKutz;
            if (pos.DistanceTo(new Vector3(-277.3488, 6225.342, 30.69159)) < 10) return BarberShopType.HerrKutz;
            return BarberShopType.None;
        }

        private static int GetBarberType(Vector3 pos)
        {
            if (pos.DistanceTo(new Vector3(-817.9705, -184.5802, 36.56892)) < 10) return 0;
            if (pos.DistanceTo(new Vector3(139.6911, -1706.982, 28.29159)) < 10) return 1;
            if (pos.DistanceTo(new Vector3(-1280.183, -1118.613, 5.99159)) < 10) return 2;
            if (pos.DistanceTo(new Vector3(1931.628, 3733.236, 31.84159)) < 10) return 3;
            if (pos.DistanceTo(new Vector3(1214.553, -474.7919, 65.20159)) < 10) return 4;
            if (pos.DistanceTo(new Vector3(-35.2074, -154.372, 56.07159)) < 10) return 5;
            if (pos.DistanceTo(new Vector3(-277.3488, 6225.342, 30.69159)) < 10) return 6;
            return -1;
        }
        #endregion

        #region Списки
        public static void LoadConfig(ENetPlayer player)
        {
            try
            {
                for (int i = 0; i < 9; i++)
                {
                    ClientEvent.Event(player, "client.business.barberShop.config", i, JsonConvert.SerializeObject(GetBarberArrayByType(i)));
                }
            }
            catch (Exception ex) { Logger.WriteError("LoadConfig", ex); }
        }
        public static List<BarberItem> GetBarberArrayByType(int type)
        {
            switch (type)
            {
                case 0: return MaleHairs;
                case 1: return eyeBrows;
                case 2: return Bread;
                case 3: return chestHair;
                case 4: return eyeColors;
                case 5: return MakeUP;

                case 6: return FemaleHairs;
                case 7: return Lipstick;
                case 8: return Blush;

                default: return null;
            }
        }

        public static List<BarberCategory> GetCategoryByGender(Gender gender)
        {
            if (gender == Gender.Male)
                return MaleCategories;
            else
                return FemaleCategories;
        }

        #region Категории
        private static List<BarberCategory> MaleCategories = new List<BarberCategory>()
        {
            new BarberCategory("Прически", 0),
            new BarberCategory("Брови", 1),
            new BarberCategory("Борода", 2),
            new BarberCategory("Волосы на груди", 3),
            new BarberCategory("Линзы", 4),
            new BarberCategory("Макияж", 5),
        };

        private static List<BarberCategory> FemaleCategories = new List<BarberCategory>()
        {
            new BarberCategory("Прически", 6),
            new BarberCategory("Брови", 1),
            new BarberCategory("Линзы", 4),
            new BarberCategory("Макияж", 5),
            new BarberCategory("Помада", 7),
            new BarberCategory("Тени", 8),
        };
        #endregion

        private static List<BarberItem> Blush = new List<BarberItem>() //Тени
        {
            new BarberItem("Нет", 255, 100),
            new BarberItem("Полный", 0, 100),
            new BarberItem("Под углом", 1, 100),
            new BarberItem("Округлый", 2, 100),
            new BarberItem("Горизонтальный", 3, 100),
            new BarberItem("На скулах", 4, 100),
            new BarberItem("Красотка", 5, 100),
            new BarberItem("В стиле 80-х", 6, 100),
        };

        private static List<BarberItem> Lipstick = new List<BarberItem>() //Губы
        {
            new BarberItem("Нет", 255, 100),
            new BarberItem("Цветные матовые", 0, 100),
            new BarberItem("Цветные блестящие", 0, 100),
            new BarberItem("Контур, матовые", 0, 100),
            new BarberItem("Контур, блестящие", 0, 100),
        };

        private static List<BarberItem> MakeUP = new List<BarberItem>() //Макияж
        {
            new BarberItem("Нет", 255, 100),
            new BarberItem("Дымчато-черный", 0, 100),
            new BarberItem("Бронзовый", 1, 100),
            new BarberItem("Мягкий серый", 2, 100),
            new BarberItem("Ретро-гламур", 3, 100),
            new BarberItem("Естественный", 4, 100),
        };

        private static List<BarberItem> eyeColors = new List<BarberItem>() //Глаза
        {
            new BarberItem("Зеленый", 0, 100),
            new BarberItem("Изумрудный", 1, 100),
            new BarberItem("Светло-голубой", 2, 100),
            new BarberItem("Океанский синий", 3, 100),
            new BarberItem("Светло-Коричневый", 4, 100),
        };

        private static List<BarberItem> eyeBrows = new List<BarberItem>() //Брови
        {
            new BarberItem("Без бровей", 255, 100),
            new BarberItem("Сбалансирован", 0, 100),
            new BarberItem("Модный", 1, 100),
            new BarberItem("Клеопатра", 2, 100),
            new BarberItem("Насмешливый", 3, 100),
        };

        private static List<BarberItem> chestHair = new List<BarberItem>() //Волосы на теле
        {
            new BarberItem("Бритая", 255, 100),
            new BarberItem("Естественные", 0, 100),
            new BarberItem("Полоска", 1, 100),
            new BarberItem("Тонкое дерево", 2, 100),
        };

        private static List<BarberItem> Bread = new List<BarberItem>() //Борода
        {
            new BarberItem("Без бороды", 255, 100),
            new BarberItem("Легкая щетина", 0, 100),
            new BarberItem("Бальбо", 1, 100),
        };

        private static List<BarberItem> MaleHairs = new List<BarberItem>() //Волосы муж
        {
            new BarberItem("Под ноль", 0, 1000),
            new BarberItem("Коротко", 1, 2000),
            new BarberItem("Ястреб", 2, 19000),
        };

        private static List<BarberItem> FemaleHairs = new List<BarberItem>() //Волосы жен
        {
            new BarberItem("Под ноль", 0, 1200),
            new BarberItem("Коротко", 1, 4000),
            new BarberItem("Слои", 2, 12000),
        };

        public class BarberItem
        {
            public string Name { get; set; }
            public int Texture { get; set; }
            public int Price { get; set; }
            public BarberItem(string name, int texture, int price)
            {
                Name = name;
                Texture = texture;
                Price = price;
            }
        }

        public class BarberCategory
        {
            public string Name { get; set; }
            public int Type { get; set; }
            public BarberCategory(string name, int type)
            {
                Name = name;
                Type = type;
            }
        }
        #endregion

        private enum BarberShopType
        {
            None,
            BobMulet,
            HerrKutz
        }
    }
}
