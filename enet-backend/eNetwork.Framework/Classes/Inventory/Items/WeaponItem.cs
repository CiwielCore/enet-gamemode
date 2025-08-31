using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Reflection.Metadata.BlobBuilder;

namespace eNetwork.Inv.Items
{
    public class WeaponItem : Item
    {
        private float wear { get; set; } = 100f;
        private string serial { get; set; } = "XXXXXX";
        private WeaponComponentsData components { get; set; } = new WeaponComponentsData();
        public float Wear
        {
            get { return wear; }
            set
            {
                wear = value;
                this.data = JsonConvert.SerializeObject(new { Wear, Serial, Components });
            }
        }
        public string Serial
        {
            get { return serial; }
            set
            {
                serial = value;
                this.data = JsonConvert.SerializeObject(new { Wear, Serial, Components });
            }
        }
        public WeaponComponentsData Components
        {
            get
            {
                return components;
            }
            set
            {
                components = value;
                this.data = JsonConvert.SerializeObject(new { Wear, Serial, Components });
            }
        }
        public WeaponItem() { }
        public WeaponItem(Item item) : base(item)
        {
            UpdateParams();
        }
        public override void UpdateParams()
        {
            if (this.data.Length == 0)
            {
                this.data = JsonConvert.SerializeObject(new { Wear, Serial, Components });
                //какое то бы сохранение сделать
                if (this.Id != -1) _ = ENet.Database.ExecuteAsync($"UPDATE `inventory` SET `data` = '{data}' WHERE `id` = '{this.Id}'");
            }
            else
            {
                WeaponItem props = JsonConvert.DeserializeObject<WeaponItem>(this.data);
                this.wear = props.wear;
                this.serial = props.serial;
                this.components = props.components;
            }
        }
        public override void RefreshParams()
        {
            this.data = JsonConvert.SerializeObject(new { Wear, Serial, Components });
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
                Data = new { Wear, Serial },
                ItemType = ItemData.ItemType.ToString(), // InvItems.GetType(this.Type).ToString(),
                IsActive = this.IsActive,
                Rarity = ItemData.Rarity.ToString(),
                Weight = ItemData.Weight,
            };
        }
    }
}
