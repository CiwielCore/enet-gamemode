using eNetwork.Framework.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Businesses.Products
{
    public class Showrooms
    {
        private static readonly Dictionary<BusinessType, List<Product>> _products = new Dictionary<BusinessType, List<Product>>()
        {
            { BusinessType.Showroom, new List<Product>()
                {
                    new Product("g63k", 1000, maxCount: 100),
                    new Product("2022kiak5gt", 99999, maxCount: 100),
                    new Product("m8gc", 123, maxCount: 100),
                }
            },
        };

        public static Product GetProduct(BusinessType type, string model)
        {
            if (!_products.TryGetValue(type, out List<Product> list)) return null;
            return list.Find(x => x.Model == model);
        }

        public static List<Product> GetProducts(BusinessType type)
        {
            if (!_products.TryGetValue(type, out List<Product> list)) return null;
            return list;
        }

        public class Product
        {
            public string Model { get; set; }
            public int Price { get; set; }
            public int MaxCount { get; set; } = 0;
            public Product(string model, int price, int maxCount = 0)
            {
                Model = model;
                Price = price;
                MaxCount = maxCount;
            }
        }
    }
}
