using eNetwork.Framework;
using System;
using System.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using MySqlConnector;
using System.Threading.Tasks;
using System.Linq;
using eNetwork.Game.Weapons;
using eNetwork.Inv;
using static eNetwork.Configs.Weapons;

namespace eNetwork.Game
{
    public static class Inventory
    {
        private static readonly Logger Logger = new Logger("inventory-manager");
        private static readonly string DBName = "inventory";
        private static readonly int _maxInventoryWeight = 20 * 1000;
        private static Dictionary<int, Storage> _inventories = new Dictionary<int, Storage>();


        /// <summary>
        /// устаревшее
        /// </summary>
        public static void Initialize()
        {
            return;
            try
            {
                //DataTable result = ENet.Database.ExecuteRead($"SELECT * FROM `{DBName}`");
                //if (result != null && result.Rows.Count > 0)
                //{
                //    foreach (DataRow row in result.Rows)
                //    {
                //        int uuid = Convert.ToInt32(row["uuid"]);
                //        List<Item> list = JsonConvert.DeserializeObject<List<Item>>(row["items"].ToString());

                //        _inventories.TryAdd(uuid, list);
                //    }
                //}

                //Logger.WriteInfo($"Загружено {_inventories.Count} инвентарей у игроков");
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }
        public static async Task<Storage> LoadInventory(int uuid)
        {
            var data = await ENet.Database.ExecuteReadAsync($"SELECT * FROM `inventory` WHERE `owner_id` = '{uuid}' and `owner_type` = '{(int)ItemOwnerType.Player}'");
            var playerStorage = new Storage();
            foreach(DataRow row in data.Rows)
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
                playerStorage.Items.Add(item);
            }
            if (_inventories.ContainsKey(uuid)) _inventories[uuid] = playerStorage;
            else _inventories.Add(uuid, playerStorage);
            return playerStorage;
        }
        //public static bool CreateInventory(int uuid)
        //{
        //    try
        //    {
                
        //        if (_inventories.TryGetValue(uuid, out List<Item> items)) return false;

        //        List<Item> list = new List<Item>();
        //        _inventories.TryAdd(uuid , list);
        //        ENet.Database.Execute($"INSERT INTO `{DBName}` (`uuid`,`items`) VALUES({uuid}, '{JsonConvert.SerializeObject(list)}')");
        //        return true;
        //    }
        //    catch(Exception ex) { Logger.WriteError("CreateInventory", ex); return false; }
        //}

        public static async Task Save(int uuid)
        {
            try
            {
                if (uuid <= 0 || !GetInventory(uuid, out Storage storage)) return;

                foreach(var item in storage.Items)
                {
                    await item.SaveAsync();
                }

                //MySqlCommand sqlCommand = new MySqlCommand($"UPDATE `{DBName}` SET `items`=@ITEMS WHERE `uuid`=@UUID");
                //sqlCommand.Parameters.AddWithValue("@ITEMS", JsonConvert.SerializeObject(items));
                //sqlCommand.Parameters.AddWithValue("@UUID", uuid);

                //await ENet.Database.ExecuteAsync(sqlCommand);
            }
            catch (Exception ex) { Logger.WriteError("Save", ex); }
        }

        public static bool GetInventory(this ENetPlayer player, out Storage storage)
        {
            try
            {
                storage = null;
                if (!player.GetCharacter(out var characterData)) return false;

                return GetInventory(characterData.UUID, out storage);
            }
            catch(Exception ex) { Logger.WriteError("GetInventory", ex); storage = null;  return false;  }
        }

        public static bool GetInventory(int uuid, out Storage storage)
        {
            try
            {
                if (uuid < 0) 
                { 
                    storage = null;
                    return false; 
                }

                //if (!_inventories.ContainsKey(uuid))
                //    CreateInventory(uuid);

                if (!_inventories.TryGetValue(uuid, out storage)) return false;
                return true;
            }
            catch (Exception ex) { Logger.WriteError("GetInventory", ex); storage = null; return false; }
        }
        public static bool CheckCanAddItem(ENetPlayer player, Item item, int slot = -1)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return false;
                var newSlot = TryGetSlot(storage, item, InventoryType.Player, slot);
                return newSlot != -1;
            }
            catch (Exception ex) { Logger.WriteError("CheckCanAddItem", ex); return false; }
        }
        public static bool AddItem(ENetPlayer player, Item item)
        {
            if (!GetInventory(player.GetUUID(), out Storage storage)) return false;
            var newSlot = TryGetSlot(storage, item, InventoryType.Player, -1);

            if (newSlot != -1)
            {
                item.Slot = newSlot;
                item.OwnerType = ItemOwnerType.Player;
                item.OwnerId = player.GetUUID();
                item.Save();
                storage.Items.Add(item);
                InvInterface.AddItem(player, "main", item, newSlot);

                if (InvItems.GetType(item.Type) == ItemType.Ammo)
                    WeaponController.UpdateCurrentWeapon(player, item.Type);
            }

            InvInterface.SendItems(player);

            return newSlot != -1;
        }
        public static async Task<bool> CreateNewItem(ENetPlayer player, Item item, int slot = -1)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return false;
                var newSlot = TryGetSlot(storage, item, InventoryType.Player, slot);

                if (newSlot != -1)
                {
                    item.Slot = newSlot;
                    var newItemId = await ENet.Database.ExecuteAsyncInt(
                        $"INSERT INTO `inventory` (`owner_type`, `owner_id`, `slot_id`, `type`, `count`, `data`) VALUES ('{(int)item.OwnerType}', '{item.OwnerId}', '{item.Slot}', '{(int)item.Type}', '{item.Count}', '{item.data}')");
                    item.Id = newItemId;
                    storage.Items.Add(item);
                    InvInterface.AddItem(player, "main", item, newSlot);

                    if (InvItems.GetType(item.Type) == ItemType.Ammo)
                        WeaponController.UpdateCurrentWeapon(player, item.Type);
                }

                InvInterface.SendItems(player);

                return newSlot != -1;
            }
            catch(Exception ex) { Logger.WriteError("TryAdd", ex); return false; }
        }

        public static int TryGetSlot(Storage storage, Item item, InventoryType type, int slot = -1)
        {
            try
            {
                if (slot >= InvInterface.GetMaxSlots(type)) return -1;

                int weight = GetWeight(item);
                if (GetWeight(storage) + weight > _maxInventoryWeight) return -1;

                if (slot == -1)
                {
                    slot = GetFreeSlot(storage, InventoryType.Player);
                    if (slot == -1) return -1;
                }
                else
                {
                    if (FindItemBySlot(storage, slot) != null) return -1;
                }

                return slot;
            }
            catch(Exception ex) { Logger.WriteError("TryAdd", ex); return -1; }
        }

        public static int GetWeight(Storage storage)
        {
            try
            {
                return storage.Items.Sum(el => el.Count * el.ItemData.Weight);
                //int weight = 0;
                //storage.ForEach(item =>
                //{
                //    var itemData = InvItems.Get(item.Type);
                //    if (itemData != null && (itemData.ItemType != ItemType.Clothes || !item.IsActive))
                //    {
                //        weight += itemData.Weight * item.Count;
                //    }
                //});

                //return weight;
            }
            catch(Exception ex) { Logger.WriteError("GetWeight", ex); return 0; }
        }

        public static int GetWeight(Item item)
        {
            return item.Count* item.ItemData.Weight;
            //int weight = 0;
            //var itemData = InvItems.Get(item.Type);
            //if (itemData != null)
            //    weight += itemData.Weight * item.Count;

            //return weight;
        }

        public static bool MoveSlot(Storage storage, int itemId, int toSlot)
        {
            try
            {
                var item = FindItemById(storage, itemId);
                if (item is null) return false;

                var inSlotItem = FindItemBySlot(storage, toSlot);
                if (inSlotItem != null) return false;

                item.Slot = toSlot;
                return true;
            }
            catch(Exception ex) { Logger.WriteError("MoveSlot", ex); return false; }
        }

        public static Item GetFastslotItem(ENetPlayer player, int slot)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return null;
                return FindItemBySlot(storage, (slot + 100) * -1);
            }
            catch(Exception ex) { Logger.WriteError("GetFastslotItem", ex); return null; }
        }

        public static int GetFreeSlot(Storage storage, InventoryType type)
        {
            try
            {
                for (int i = 0; i < InvInterface.GetMaxSlots(type); i++)
                {
                    var item = FindItemBySlot(storage, i);
                    if (item != null) continue;

                    return i;
                }

                return -1;
            }
            catch(Exception ex) { Logger.WriteError("GetFreeSlot", ex); return -1; }
        }

        public static Item FindItemById(Storage storage, long id)
        {
            return storage.Items.Find(x => x.Id == id);
        }
        public static Item FindItemBySlot(Storage storage, int slot)
        {
            return storage.Items.Find(x => x.Slot == slot);
        }

        public static Item FindItem(Storage storage, ItemId type)
        {
            return storage.Items.Find(x => x.Type == type);
        }

        public static int GetCount(Storage storage, ItemId itemId)
        {
            return storage.Items.Count(el => el.Type == itemId);
            //int count = 0;
            //storage.Items.ForEach((x) => { if (x.Type == itemId) count += x.Count; });
            //return count;
        }

        public static Item FindItem(ENetPlayer player, long id)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return null;
                return FindItemById(storage, id);
            }
            catch(Exception ex) { Logger.WriteError("FindItem", ex); return null; }
        }

        public static Item FindItem(ENetPlayer player, ItemId id)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return null;
                return FindItem(storage, id);
            }
            catch(Exception ex) { Logger.WriteError("FindItem", ex); return null; }
        }

        public static bool RemoveItem(ENetPlayer player, int slot)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return false;

                var item = FindItemBySlot(storage, slot);
                if (item is null) return false;

                storage.Items.Remove(item);
                InvInterface.SendItems(player);
                return true;
            }
            catch(Exception ex) { Logger.WriteError("RemoveItem", ex); return false; }
        }

        public static bool RemoveItem(ENetPlayer player, ItemId itemId, int count)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return false;
                var result = RemoveItem(storage, itemId, count);
                InvInterface.SendItems(player);

                return result;
            }
            catch (Exception ex) { Logger.WriteError("RemoveItem", ex); return false; }
        }

        public static bool RemoveItem(Storage storage, ItemId itemId, int count)
        {
            try
            {
                var item = FindItem(storage, itemId);
                if (item is null) return false;

                item.Count -= count;
                if (item.Count <= 0)
                    storage.Items.Remove(item);

                return true;
            }
            catch (Exception ex) { Logger.WriteError("RemoveItem", ex); return false; }
        }

        public static bool RemoveItem(ENetPlayer player, Item item, bool delete = false)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return false;

                item.Delete();
                storage.Items.Remove(item);

                InvInterface.SendItems(player);
                return true;
            }
            catch (Exception ex) { Logger.WriteError("RemoveItem", ex); return false; }
        }

        public static bool RemoveItem(ENetPlayer player, Item item, int count)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return false;

                if (item.Count < count) return false;
                if (item.Count == count)
                    storage.Items.Remove(item);
                else
                    item.Count -= count;

                InvInterface.SendItems(player);
                return true;
            }
            catch (Exception ex) { Logger.WriteError("RemoveItem", ex); return false; }
        }

        public static void OnDeath(ENetPlayer player)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return;
                var droppedItems = storage.Items.Where(el=>el.Slot < -100);
                foreach (var item in droppedItems)
                {
                    var itemType = InvItems.GetType(item.Type);
                    if (itemType == ItemType.Weapon)
                    {
                        if(item is Inv.Items.WeaponItem weaponItem)
                        {
                            
                            //InvWeaponData _weaponData = JsonConvert.DeserializeObject<InvWeaponData>(item.Data);
                            if (weaponItem.Components.Suppressor)
                            {
                                weaponItem.Components.Suppressor = false;
                                InvGround.Drop(player.Position, new Item(ItemId.Suppressor, 1), player.Dimension);
                            }
                            if (weaponItem.Components.Scope)
                            {
                                weaponItem.Components.Scope = false;
                                InvGround.Drop(player.Position, new Item(ItemId.Scope, 1), player.Dimension);
                            }
                            if (weaponItem.Components.HolographicSight)
                            {
                                weaponItem.Components.HolographicSight = false;
                                InvGround.Drop(player.Position, new Item(ItemId.HolographicSight, 1), player.Dimension);
                            }
                            if (weaponItem.Components.Clip)
                            {
                                weaponItem.Components.Clip = false;
                                InvGround.Drop(player.Position, new Item(ItemId.ExtendedClip, 1), player.Dimension);
                            }
                            if (weaponItem.Components.Flashlight)
                            {
                                weaponItem.Components.Flashlight = false;
                                InvGround.Drop(player.Position, new Item(ItemId.FlashlightComponent, 1), player.Dimension);
                            }
                            if (weaponItem.Components.Grip)
                            {
                                weaponItem.Components.Grip = false;
                                InvGround.Drop(player.Position, new Item(ItemId.Grip, 1), player.Dimension);
                            }
                            weaponItem.RefreshParams();
                        }
                        WeaponAttachment.RemoveWeaponFromFastSlot(player, item.Type, (item.Slot + 100) * -1);
                    }
                    InvGround.Drop(player, item);

                }

                player.RemoveItemHand();
            }
            catch(Exception ex) { Logger.WriteError("OnDeath", ex); }
        }

        public static void LoadFastSlot(ENetPlayer player)
        {
            try
            {
                if (!GetInventory(player.GetUUID(), out Storage storage)) return;
                foreach(var item in storage.Items)
                {
                    if (item.Slot < -100)
                    {
                        if(item is Inv.Items.WeaponItem weaponItem)
                        {
                            WeaponAttachment.SetWeaponInFastSlot(
                                player, 
                                item.Type, 
                                weaponItem.Components,
                                (item.Slot + 100) * -1
                                );
                        }
                        //var itemType = InvItems.GetType(item.Type);
                        //if (itemType == ItemType.Weapon || itemType == ItemType.Melee)
                        //    WeaponAttachment.SetWeaponInFastSlot(player, item.Type, JsonConvert.DeserializeObject<InvWeaponData>(item.Data).WeaponComponents, (item.Slot + 100) * -1);
                    }
                }
            }
            catch(Exception ex) { Logger.WriteError("LoadFastSlot", ex); }
        }
    }

    public enum InventoryType
    {
        Player,
        Vehicle,
        Backpack
    }
}
