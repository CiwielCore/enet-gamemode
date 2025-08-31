using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Inv
{
    public class Storage
    {
        public List<Item> Items { get; set; } = new List<Item>();
        public float MaxWeight = 20 * 1000;
        public Storage() { }
    }
}
