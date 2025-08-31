using eNetwork.Inv;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.HiddingBox.Classes
{
    public class HiddenBoxSettings
    {
        public string Name { get; set; }    
        public uint Model { get; set; }
        public List<ItemId> Items { get; set; }
    }
}
