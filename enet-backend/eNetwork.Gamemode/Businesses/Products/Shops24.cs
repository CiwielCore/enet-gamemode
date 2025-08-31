using eNetwork.Configs;
using eNetwork.Framework.Enums;
using eNetwork.Framework;
using eNetwork.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eNetwork.Inv;

namespace eNetwork.Businesses.Products
{
     class Shops24
    {
        private static readonly Logger Logger = new Logger("shops24-products");

        private static Dictionary<BusinessType, List<Product>> _products = new Dictionary<BusinessType, List<Product>>();
        private static readonly Dictionary<BusinessType, Dictionary<string, List<Product>>> _categories = new Dictionary<BusinessType, Dictionary<string, List<Product>>>()
        {
            { BusinessType.Shop24, new Dictionary<string, List<Product>>() {
                    { "Продукты", new List<Product>() {
                         new Product(ItemId.eCola, 1000, maxCount: 100),
                         new Product(ItemId.Burger, 1000, maxCount: 100),
                    }},
                    { "Электроника", new List<Product>() {
                         new Product(ItemId.Phone, 1000, maxCount: 100),
                    }},
                    { "Инструменты", new List<Product>() {
                        new Product(ItemId.Phone, 1000, maxCount: 100),
                    }},
                    { "Прочее", new List<Product>() {
                        new Product(ItemId.LotteryTicket, 100, maxCount: 100)
                    }}
                }
            },
        };

        public static void Initialize()
        {
            try
            {
                foreach (var item in _categories)
                {
                    if (!_products.ContainsKey(item.Key))
                        _products.Add(item.Key, new List<Product>());

                    item.Value.ToList().ForEach((categories) =>
                        categories.Value.ForEach((product) => _products[item.Key].Add(product)));
                }
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public static Product GetProduct(BusinessType type, string item)
        {
            if (!_products.TryGetValue(type, out List<Product> list)) return null;
            return list.Find(x => x.Item.ToString() == item);
        }

        public static List<Product> GetProducts(BusinessType type)
        {
            if (!_products.TryGetValue(type, out List<Product> list)) return null;
            return list;
        }

        public static Dictionary<BusinessType, Dictionary<string, List<Product>>> GetCatregories()
        {
            return _categories;
        }

        public class Product
        {
            public string Item { get; set; }
            public int Price { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
            public string Picture { get; set; }
            public int MaxCount { get; set; }

            public Product(ItemId item, int price, int count = 1, int maxCount = 0)
            {
                Item = item.ToString();
                Price = price;
                Count = count;

                var itemData = InvItems.Get(item);
                if (itemData != null)
                {
                    Name = itemData.Name;
                    Picture = itemData.Picture;
                }
                MaxCount = maxCount;
            }
        }
    }
}
