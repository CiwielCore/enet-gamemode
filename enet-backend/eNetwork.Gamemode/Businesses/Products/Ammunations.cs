using eNetwork.Configs;
using eNetwork.Framework;
using eNetwork.Framework.Enums;
using eNetwork.Game;
using eNetwork.Inv;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Businesses.Products
{
    public class Ammunations
    {
        private static readonly Logger Logger = new Logger("ammunation-products");

        private static Dictionary<BusinessType, List<Product>> _products = new Dictionary<BusinessType, List<Product>>();
        private static readonly Dictionary<BusinessType, Dictionary<string, List<Product>>> _categories = new Dictionary<BusinessType, Dictionary<string, List<Product>>>()
        {
            { BusinessType.AmmuNation, new Dictionary<string, List<Product>>() {
                    { "pistols", new List<Product>() {
                         new Product(ItemId.Heavypistol, 1000, maxCount: 100),
                         new Product(ItemId.Pistol50, 1000, maxCount: 100),
                         new Product(ItemId.Vintagepistol, 1000, maxCount: 100),
                         new Product(ItemId.Appistol, 1000, maxCount: 100),
                    }},
                    { "smg", new List<Product>() {
                        new Product(ItemId.Machinepistol, 1000, maxCount : 100),
                        new Product(ItemId.Microsmg, 1000, maxCount : 100),
                    }},
                    { "shotguns", new List<Product>() {
                        new Product(ItemId.Doublebarrelshotgun, 1000, maxCount : 100),
                        new Product(ItemId.Sawnoffshotgun, 1000, maxCount : 100),
                        new Product(ItemId.Pumpshotgun, 1000, maxCount : 100),
                        new Product(ItemId.Assaultshotgun, 1000, maxCount : 100),
                    }},
                    { "rifles", new List<Product>() {
                        new Product(ItemId.Assaultrifle, 1000, maxCount : 100),
                        new Product(ItemId.Bullpuprifle, 1000, maxCount : 100),
                        new Product(ItemId.Carbinerifle, 1000, maxCount : 100),
                    }},
                    { "equipemnt", new List<Product>() {
                        new Product(ItemId.Ammo9x19mm, 100, 30, maxCount: 100),
                        new Product(ItemId.Ammo762x39mm, 100, 30, maxCount: 100),
                        new Product(ItemId.Ammo556x45mm, 100, 30, maxCount : 100),
                        new Product(ItemId.Ammo45Acp, 100, 30, maxCount : 100),
                        new Product(ItemId.Ammo12gaBuckshots, 100, 30, maxCount : 100),
                        new Product(ItemId.Ammo12gaRifles, 100, 30, maxCount : 100),
                        new Product(ItemId.Ammo357Magnum, 100, 30, maxCount : 100),
                        new Product(ItemId.Ammo50bmg, 100, 5, maxCount : 100),
                        new Product(ItemId.BodyArmor, 1200, 1, maxCount : 100)
                    }},
                }
            },
        };

        public static void Initialize()
        {
            try
            {
                foreach(var item in _categories)
                {
                    if (!_products.ContainsKey(item.Key))
                        _products.Add(item.Key, new List<Product>());

                    item.Value.ToList().ForEach((categories) => 
                        categories.Value.ForEach((product) => _products[item.Key].Add(product)));
                }
            }
            catch(Exception ex) { Logger.WriteError("Initialize", ex); }
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
            public uint Model { get; set; } = 0;
            public string Name { get; set; }
            public int Count { get; set; }
            public int MaxCount { get; set; }

            public Product(ItemId item, int price, int count = 1, int maxCount = 0)
            {
                Item = item.ToString();
                Price = price;
                Count = count;

                var weaponData = Weapons.GetWeaponData(item);
                if (weaponData != null)
                    Model = weaponData.Hash;

                var itemData = InvItems.Get(item);
                if (itemData != null)
                {
                    Name = itemData.Name;
                    if (Model == 0)
                        Model = itemData.Model;
                }
                MaxCount = maxCount;
            }
        }
    }
}
