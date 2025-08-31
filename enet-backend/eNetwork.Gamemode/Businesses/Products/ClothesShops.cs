using eNetwork.Configs;
using eNetwork.Framework.Enums;
using eNetwork.Framework;
using eNetwork.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using eNetwork.Inv;

namespace eNetwork.Businesses.Products
{
     class ClothesShops
     {
        private static readonly Logger Logger = new Logger("clothes-products");

        private static Dictionary<BusinessType, List<Product>> _products = new Dictionary<BusinessType, List<Product>>();
        private static readonly Dictionary<BusinessType, List<Product>> _categories = new Dictionary<BusinessType, List<Product>>()
        {
            { BusinessType.ClothesShop, new List<Product>() {
                    new Product(ItemId.Top, "Мужской топ 1", 1000, new InvClothesData(Gender.Male, 2, 0), maxCount: 100),
                    new Product(ItemId.Undershirt, "Мужской андертоп 1", 1200, new InvClothesData(Gender.Male, 5, 0), maxCount: 100),
                    new Product(ItemId.Undershirt, "Женский андертоп 1", 1200, new InvClothesData(Gender.Female, 11, 0), maxCount: 100),
                }
            },
        };

        private static readonly Dictionary<int, CharPos> _positions = new Dictionary<int, CharPos>() // int = business id
        {
            { 31, new CharPos(new Position(122.98451, -221.08817, 54.437874, -20.752108), new Position(124.067665, -218.09131, 55, 158.82431)) }
        };

        public static void Initialize()
        {
            try
            {
                foreach (var item in _categories)
                {
                    if (!_products.ContainsKey(item.Key))
                        _products.Add(item.Key, new List<Product>());

                    item.Value.ToList().ForEach((product) =>
                        _products[item.Key].Add(product));
                }
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }
        public static CharPos GetPositions(int id)
        {
            return _positions[id];
        }

        public static Product GetProduct(BusinessType type, string item)
        {
            if (!_products.TryGetValue(type, out List<Product> list)) return null;
            return list.Find(x => x.Item.ToString() == item);
        }

        public static List<Product> GetProducts(BusinessType type)
        {
            if (!_categories.TryGetValue(type, out List<Product> list)) return null;
            return list;
        }

        public static Dictionary<BusinessType, List<Product>> GetCatregories()
        {
            return _categories;
        }

        public class CharPos
        {
            public Position PlayerPos { get; set; }
            public Position CameraPos { get; set; }

            public CharPos(Position playerPos, Position cameraPos)
            {
                PlayerPos = playerPos;
                CameraPos = cameraPos;
            }
        }
        public class Product
        {
            public ItemId Item { get; set; }
            public int Price { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
            public string Picture { get; set; }
            public InvClothesData InvClothesData { get; set; }
            public int MaxCount { get; set; }

            public Product(ItemId item, string name, int price, InvClothesData invClothesData, int count = 1, int maxCount = 0)
            {
                Item = item;
                Price = price;
                Count = count; 
                Name = name;
                InvClothesData = invClothesData;
                MaxCount = maxCount;

                var itemData = InvItems.Get(item);
                if (itemData != null)
                {
                    Picture = itemData.Picture;
                }
            }
        }
    }
}
