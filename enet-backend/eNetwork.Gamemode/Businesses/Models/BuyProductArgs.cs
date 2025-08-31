using eNetwork.Inv;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Businesses.Models
{
    public class BuyProductArgs : EventArgs
    {
        public ItemId Item { get; }
        public int Price { get; }

        public BuyProductArgs(ItemId itemId, int price)
        {
            Item = itemId;
            Price = price;
        }
    }
}
