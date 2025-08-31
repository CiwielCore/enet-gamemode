using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Inv.Items
{
    public class BackpackItem : ClotheItem
    {
        public int Slots
        {
            get
            {
                //закладываю логику где undershirt для рюкзаков - это кол-во слотов
                
                return ClotheModel.UndershirtId;
            }
        }
        public float Weight
        {
            get
            {
                //torse - это макс.вес
                return ClotheModel.TorseId;
            }
        }
        public List<Item> Items { get; set; } = new List<Item>();
        public BackpackItem() { }
        public BackpackItem(Item item) : base(item)
        {
            UpdateParams();
        }
        public override object GetItemData()
        {
            return new
            {
                Id = Id,
                Type = this.Type,
                Slot = this.Slot,
                Name = ItemData != null ? ItemData.Name : "undefined",
                Description = ItemData != null ? ItemData.Description : "undefined",
                Picture = ItemData != null ? ItemData.Picture : "null",
                Count = this.Count,
                Data = new { Slots },
                ItemType = ItemData.ItemType.ToString(), // InvItems.GetType(this.Type).ToString(),
                IsActive = this.IsActive,
                Rarity = ItemData.Rarity.ToString(),
                Weight = ItemData.Weight,
            };
        }
    }
}
