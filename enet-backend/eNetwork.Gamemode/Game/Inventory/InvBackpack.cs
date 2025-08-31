using eNetwork.Framework;
using eNetwork.Inv;
using eNetwork.Inv.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Game
{
    public class InvBackpack
    {
        private static readonly Logger Logger = new Logger("inventory-backpack");
        public static void SendItems(ENetPlayer player, BackpackItem backpack)
        {
            try
            {
                ClientEvent.Event(player, "client.inventory.setItems", "bag", JsonConvert.SerializeObject(backpack.Items.Select(el=>el.GetItemData())));
            }
            catch (Exception ex) { Logger.WriteError("SendItems", ex); }
        }
        public static async Task LoadItems(BackpackItem backpack)
        {
            backpack.Items.Clear();
            DataTable data = await ENet.Database.ExecuteReadAsync(
                    $"SELECT * FROM `inventory` WHERE `owner_id` = '{backpack.Id}' and `owner_type` = '{(int)ItemOwnerType.Backpack}'");
            foreach (DataRow row in data.Rows)
            {
                var item = new Item(
                        id: long.Parse(row["id"].ToString()),
                        itemid: (ItemId)Convert.ToInt32(row["type"]),
                        count: Convert.ToInt32(row["count"]),
                        slot: Convert.ToInt32(row["slot_id"]),
                        dataStr: Convert.ToString(row["data"]),
                        itemOwnerType: (ItemOwnerType)Convert.ToInt32(row["owner_type"]),
                        ownerId: long.Parse(row["owner_id"].ToString()),
                        isActive: Convert.ToInt32(row["isActive"]));
                item = TypedItems.Get(item);
                backpack.Items.Add(item);
            }
        }
        public static bool GetInventory(int uuid, out Storage items)
        {
            try
            {
                if (!Inventory.GetInventory(uuid, out Storage storage))
                {
                    items = null;
                    return false;
                }

                var bagItem = storage.Items.Find(x => x.Type == ItemId.Bag && x.IsActive && x.Slot == -((int)x.Type));
                if (bagItem is null)
                {
                    items = null;
                    return false;
                }

                items = JsonConvert.DeserializeObject<InvBackpackData>(bagItem.Data).Inventory;
                return true;
            }
            catch(Exception ex) { Logger.WriteError("GetInventory", ex); items = null; return false; }
        }

        public static bool AddItem(BackpackItem backpack, Item item, int slot)
        {
            try
            {
                item.Slot = slot;
                item.OwnerType = ItemOwnerType.Backpack;
                item.OwnerId = backpack.Id;
                item.Save();
                backpack.Items.Add(item);
                return true;
            }
            catch (Exception e) { Logger.WriteError("BackpackAddItem", e); return false; }
        }

        public static void RemoveItem(BackpackItem backpack, int currentSlot)
        {
            try
            {

                var currentItem = backpack.Items.FirstOrDefault(x => x.Slot == currentSlot);
                if (currentItem is null) return;

                backpack.Items.Remove(currentItem);
            }
            catch(Exception ex) { Logger.WriteError("BackpackRemoveItem", ex); }
        }

        public static void RemoveItem(BackpackItem backpack, Item currentItem)
        {
            try
            {
                backpack.Items.Remove(currentItem);
            }
            catch (Exception ex) { Logger.WriteError("RemoveItem", ex); }
        }
        public static Item FindItemById(BackpackItem backpack, int id)
        {
            return backpack.Items.FirstOrDefault(el=>el.Id == id);
        }
        public static Item FindItemBySlot(BackpackItem backpack, int currentSlot)
        {
            return backpack.Items.FirstOrDefault(el => el.Slot == currentSlot);
        }

        public static bool MoveItem(BackpackItem backpack, int currentSlot, int dropSlot)
        {
            try
            {

                var currentItem = FindItemBySlot(backpack, currentSlot);
                if (currentItem is null) return false;

                var dropItem = FindItemBySlot(backpack, dropSlot);
                if (dropItem is null)
                {
                    currentItem.Slot = dropSlot;
                }
                else
                {
                    currentItem.Slot = dropSlot;
                    dropItem.Slot = currentSlot;
                }

                return true;
            }
            catch(Exception ex) { Logger.WriteError("MoveItem", ex); return false; }
        }
        public static int GetMaxSlots(ENetPlayer player)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return -1;
                var backpack = (Inv.Items.BackpackItem)storage.Items.FirstOrDefault(el=>el is Inv.Items.BackpackItem && el.Type == ItemId.Bag && el.IsActive && el.Slot == -((int)el.Type));
                if (backpack == null) return -1;
                return GetMaxSlots(backpack);
            }
            catch (Exception ex) { Logger.WriteError("GetMaxSlots", ex); return 0; }
        }
        public static int GetMaxSlots(BackpackItem backpack)
        {
            try
            {
                return backpack.Slot;
            }
            catch(Exception ex) { Logger.WriteError("GetMaxSlots", ex); return 0; }
        }
    }
}
