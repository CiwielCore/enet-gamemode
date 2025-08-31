using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Businesses
{
    public class BizProduct
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
        public int MaxCount { get; set; }
        public bool Disable { get; set; }
        public int CountSell { get; set; } = 0;
        public BizProduct(string name, int price, int count, int maxCount, bool disable = false)
        {
            Name = name; Price = price; Count = count;
            MaxCount = maxCount;
            Disable = disable;
        }

        public int GetPrice(Business business)
        {
            return business.GetPrice(Price);
        }
    }
}
