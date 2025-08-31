using NeptuneEvo.Jobs.Fishing.Classes;
using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Newtonsoft.Json;
using System.Linq;
using eNetwork.Jobs.Fishing.Classes;
using eNetwork.Jobs.Fishing.Store;
using eNetwork.Jobs.Fishing.Store.Classes;
using eNetwork.Game;
using eNetwork.Inv;

namespace eNetwork.Jobs.Fishing.Data
{
    public class Config
    {
        public static void Initialize()
        {
            STORES.ForEach(b => 
            { 
                b.GTAElements(); 
                b.Update(); 
            });

            SPOTS_DATA.ForEach(s => s.GTAElements());

            FISH_ITEMS_DATA = FISH_ITEMS_DATA.OrderBy(x => x.Chance).Reverse().ToList();
        }

        /// <summary>
        /// Данные о удочках
        /// 
        /// wear - Износ
        /// minTime - Минимальное время ожидания улова
        /// maxTime - Максимальное время ожидания улова
        /// </summary>
        public static readonly Dictionary<ItemId, RodData> RODS_DATA = new Dictionary<ItemId, RodData>()
        {
            { ItemId.Rod, new RodData(wear: 1, minTime: 10, maxTime: 20) },
            { ItemId.RodUpgraded, new RodData(wear: .5, minTime: 5, maxTime: 15) },
            { ItemId.RodMk2, new RodData(wear: .25, minTime: 3, maxTime: 5) },
        };

        /// <summary>
        /// Данные о всех рыбах
        /// 
        /// minLvl - Мниимальный уровень рыбака, чтобы выловить эту рыбку
        /// chanсe - Шанс улова
        /// minPrice - Минимальная цена за рыбу
        /// maxPrice - Максимальная цена за рыбу
        /// rods - список удочек, которыми можно поймать рыбу
        /// </summary>
        public static List<FishItemData> FISH_ITEMS_DATA = new List<FishItemData>()
        {
             new FishItemData(ItemId.Sterlad, minLvl: 0, chance: 50, minPrice: 36, maxPrice: 47),
             new FishItemData(ItemId.Losos, minLvl: 0, chance: 50, minPrice: 49, maxPrice: 64),
             new FishItemData(ItemId.Osetr, minLvl: 0, chance: 50, minPrice: 88, maxPrice: 116),
             new FishItemData(ItemId.BlackAmur, minLvl: 4, chance: 50, minPrice: 176, maxPrice: 231),
             new FishItemData(ItemId.Skat, minLvl: 4, chance: 50, minPrice: 200, maxPrice: 263),
             new FishItemData(ItemId.Tunec, minLvl: 4, chance: 50, minPrice: 560, maxPrice: 735),
             new FishItemData(ItemId.Malma, minLvl: 4, chance: 50, minPrice: 840, maxPrice: 1103),
             new FishItemData(ItemId.Fugu, minLvl: 6, chance: 10, minPrice: 2000, maxPrice: 2625, rods: new List<ItemId>() { ItemId.RodMk2 }, isDonate: false),
        };

        /// <summary>
        /// Список всех точек рыболовли
        /// </summary>
        public static readonly List<FishSpot> SPOTS_DATA = new List<FishSpot>()
        {
            new FishSpot("Зона рыбалки", 50, new Vector3(-1829.282, -1212.4371, 13.017266), 
                fish: new List<ItemId>() { ItemId.Sterlad, ItemId.Losos, ItemId.Osetr, ItemId.BlackAmur, ItemId.Skat, ItemId.Tunec, ItemId.Malma, ItemId.Fugu }
            ),
        };

        /// <summary>
        /// Список
        /// </summary>
        public static readonly Dictionary<ItemId, int> FISHING_STORE_ITEMS = new Dictionary<ItemId, int>()
        {
            { ItemId.Rod, 1000 },
            { ItemId.RodUpgraded, 25000 },
            { ItemId.RodMk2, 40000 },
            { ItemId.Bait, 10 },
        };

        // Список о лвлах
        public static readonly Dictionary<int, int> EXP_LVL_DATA = new Dictionary<int, int>()
        {
            { 1, 250 },     // 250 - количетсво рыб, чтобы подняться с 1 уровная до 2
            { 2, 250 },     // 250 - количетсво рыб, чтобы подняться с 2 уровная до 3
            { 3, 250 },     // 250 - количетсво рыб, чтобы подняться с 3 уровная до 4
            { 4, 250 },     // 250 - количетсво рыб, чтобы подняться с 4 уровная до 5
            { 5, 250 },     // 250 - количетсво рыб, чтобы подняться с 5 уровная до 6
        };

        public static List<FishingMarket> STORES = new List<FishingMarket>()
        {
            new FishingMarket("Рыболовный магазин", position: new Vector3(-2293.3367, 369.56128, 174.60161), heading: 24, blipSprite: 0, blipColor: 0, (uint)PedHash.Bevhills01AMM, 
                new Dictionary<string, List<MarketItem>>()
                {
                    { "fishingCategoryRod", new List<MarketItem>() {
                        new MarketItem("Удочка", ItemId.Rod, minPrice: 500, maxPrice: 500, value: 1),
                        new MarketItem("Удочка улучш.", ItemId.RodUpgraded, minPrice: 10000, maxPrice: 10000, value: 1),
                        new MarketItem("Удочка Mk2", ItemId.RodMk2, minPrice: 40000, maxPrice: 40000, value: 1),
                    }},
                    { "fishingCategoryBait", new List<MarketItem>() {
                        new MarketItem("Наживка", ItemId.Bait, 100, 100, 1),
                    }},
                }
            ),

            new FishingMarket("Скупщик рыбы", position: new Vector3(-1270.824, -1453.4025, 4.5562973), heading: -58, blipSprite: 0, blipColor: 0, (uint)PedHash.Barry, new Dictionary<string, List<MarketItem>>()
            {
                { "fishingCategoryFish", new List<MarketItem>() {
                    new MarketItem(InvItems.Get(FISH_ITEMS_DATA[0].ItemId).Name, FISH_ITEMS_DATA[0].ItemId, FISH_ITEMS_DATA[0].MinPrice, FISH_ITEMS_DATA[0].MaxPrice, 1, true, true, FISH_ITEMS_DATA[0].IsDonate),
                    new MarketItem(InvItems.Get(FISH_ITEMS_DATA[1].ItemId).Name, FISH_ITEMS_DATA[1].ItemId, FISH_ITEMS_DATA[1].MinPrice, FISH_ITEMS_DATA[1].MaxPrice, 1, true, true, FISH_ITEMS_DATA[1].IsDonate),
                    new MarketItem(InvItems.Get(FISH_ITEMS_DATA[2].ItemId).Name, FISH_ITEMS_DATA[2].ItemId, FISH_ITEMS_DATA[2].MinPrice, FISH_ITEMS_DATA[2].MaxPrice, 1, true, true, FISH_ITEMS_DATA[2].IsDonate),
                    new MarketItem(InvItems.Get(FISH_ITEMS_DATA[3].ItemId).Name, FISH_ITEMS_DATA[3].ItemId, FISH_ITEMS_DATA[3].MinPrice, FISH_ITEMS_DATA[3].MaxPrice, 1, true, true, FISH_ITEMS_DATA[3].IsDonate),
                    new MarketItem(InvItems.Get(FISH_ITEMS_DATA[4].ItemId).Name, FISH_ITEMS_DATA[4].ItemId, FISH_ITEMS_DATA[4].MinPrice, FISH_ITEMS_DATA[4].MaxPrice, 1, true, true, FISH_ITEMS_DATA[4].IsDonate),
                    new MarketItem(InvItems.Get(FISH_ITEMS_DATA[5].ItemId).Name, FISH_ITEMS_DATA[5].ItemId, FISH_ITEMS_DATA[5].MinPrice, FISH_ITEMS_DATA[5].MaxPrice, 1, true, true, FISH_ITEMS_DATA[5].IsDonate),
                    new MarketItem(InvItems.Get(FISH_ITEMS_DATA[6].ItemId).Name, FISH_ITEMS_DATA[6].ItemId, FISH_ITEMS_DATA[6].MinPrice, FISH_ITEMS_DATA[6].MaxPrice, 1, true, true, FISH_ITEMS_DATA[6].IsDonate),
                    new MarketItem(InvItems.Get(FISH_ITEMS_DATA[7].ItemId).Name, FISH_ITEMS_DATA[7].ItemId, FISH_ITEMS_DATA[7].MinPrice, FISH_ITEMS_DATA[7].MaxPrice, 1, true, true, FISH_ITEMS_DATA[7].IsDonate),
                }},
            })
        };
    }
}
