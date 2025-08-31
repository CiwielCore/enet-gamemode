using eNetwork.Game;
using GTANetworkMethods;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Inv
{
    public class Item
    {
        public long Id { get; set; } = -1;
        public ItemId Type { get; set; }
        public int Slot { get; set; } = -1;
        
        public dynamic Data { get; set; }
        public int Count { get; set; }
        /// <summary>
        /// зачем это нужно?
        /// </summary>
        public bool IsActive { get; set; }
        public ItemData ItemData { get; set; }
        /// <summary>
        /// сериализованная доп.информация по классу
        /// </summary>
        public string data { get; set; } = "";
        public ItemOwnerType OwnerType { get; set; } = ItemOwnerType.None;
        public long OwnerId { get; set; } = -1;
        public Item() { }
        public Item(ItemId itemid, int count = 1, string dataStr = "")//bool isActive = false, dynamic data = null, int slot = -1)
        {
            Type = itemid;
            Count = count;
            //IsActive = isActive;
            data = dataStr;
            //Slot = slot;
            ItemData = InvItems.Get(itemid);
        }
        public Item(long id, ItemId itemid, int count, int slot, string dataStr, ItemOwnerType itemOwnerType, long ownerId, int isActive)
        {
            Id = id; 
            Type = itemid; 
            Count = count; 
            Slot = slot;
            ItemData = InvItems.Get(itemid);
            data = dataStr;
            OwnerType = itemOwnerType;
            OwnerId = ownerId;
            IsActive = isActive == 1 ? true : false;
        }
        public Item(Item item)
        {
            this.Id = item.Id;
            this.Type = item.Type;
            this.Slot = item.Slot;
            this.Data = item.Data;
            this.Count = item.Count;
            this.IsActive = item.IsActive;
            this.ItemData = item.ItemData;
            this.data = item.data;
            this.OwnerType = item.OwnerType;
            this.OwnerId = item.OwnerId;
        }
        public virtual object GetItemData()
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
                Data = this.Data,
                ItemType = ItemData.ItemType.ToString(), // InvItems.GetType(this.Type).ToString(),
                IsActive = this.IsActive,
                Rarity = ItemData.Rarity.ToString(),
                Weight = ItemData.Weight,
            };
        }
        public virtual void UpdateParams() { }
        public virtual void RefreshParams() { }
        public virtual void Save()
        {
            if (Id == -1) return;
            _ = ENet.Database.ExecuteAsync($"UPDATE `inventory` SET `owner_type` = '{(int)OwnerType}', `owner_id` = '{(int)OwnerId}', `slot_id` = '{Slot}', `count` = '{Count}', `data` = '{data}', `isActive` = '{(IsActive ? 1 : 0)}' WHERE `id` = '{Id}'");
        }
        public virtual async System.Threading.Tasks.Task SaveAsync()
        {
            if (Id != -1) await ENet.Database.ExecuteAsync($"UPDATE `inventory` SET `owner_type` = '{(int)OwnerType}', `owner_id` = '{(int)OwnerId}', `slot_id` = '{Slot}', `count` = '{Count}', `data` = '{data}', `isActive` = '{(IsActive ? 1 : 0)}' WHERE `id` = '{Id}'");
            return;
        }
        public void Delete()
        {
            _ = ENet.Database.ExecuteAsync($"DELETE FROM `inventory` WHERE `id` = '{Id}'");
        }
    }
}
