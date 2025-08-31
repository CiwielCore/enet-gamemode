using eNetwork.Factions;
using eNetwork.Factions.Classes;
using eNetwork.Framework;
using eNetwork.Framework.Classes;
using eNetwork.Framework.Enums;
using eNetwork.Inv;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Game
{
    public class WeaponController
    {
        private static readonly Logger Logger = new Logger("weapon-controller");

        public static int GetAmmo(ENetPlayer player, ItemId itemId)
        {
            try
            {
                if (!Inventory.GetInventory(player.GetUUID(), out var items)) return 0;
                Configs.Weapons.WeaponData weaponConfig = Configs.Weapons.GetWeaponData(itemId);
                if (weaponConfig is null) return 0;

                return Inventory.GetCount(items, weaponConfig.AmmoId);
            }
            catch (Exception ex) { Logger.WriteError("GetAmmo", ex); return 0; }
        }

        public static string GenerateSerial(FactionId id, int index)
        {
            try
            {
                string seraial = "";

                //if (id != FactionId.None)
                //    //seraial += Factionnager.GetFactionTag(id);
                //else
                //            seraial += $"A{index}-";

                //for (int i = 0; i < 12; i++)
                //    seraial += $"{ENet.Random.Next(0, 9)}";

                return seraial;
            }
            catch (Exception ex) { Logger.WriteError("GenerateSerial", ex); return ""; }
        }
        public static Item GetCurrentWeapon(Storage storage)
        {
            try
            {
                return storage.Items.Find(x => (InvItems.GetType(x.Type) == ItemType.Weapon || InvItems.GetType(x.Type) == ItemType.Melee || InvItems.GetType(x.Type) == ItemType.Consumable) && x.IsActive);
            }
            catch (Exception ex) { Logger.WriteError("GetCurrentWeapon", ex); return null; }
        }

        public static void UpdateCurrentWeapon(ENetPlayer player, ItemId ammoId)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData) || !Inventory.GetInventory(player.GetUUID(), out var items)) return;
                var currentWeapon = GetCurrentWeapon(items);
                if (currentWeapon is null) return;

                Configs.Weapons.WeaponData weaponConfig = Configs.Weapons.GetWeaponData(currentWeapon.Type);
                if (weaponConfig is null || weaponConfig.AmmoId != ammoId) return;

                var ammo = GetAmmo(player, currentWeapon.Type);
                ClientEvent.Event(player, "client.weapon.update", weaponConfig.Hash, ammo);
            }
            catch (Exception ex) { Logger.WriteError("UpdateCurrentWeapon", ex); }
        }

        public static void OnDeath(ENetPlayer player)
        {
            try
            {
                if (!Inventory.GetInventory(player.GetUUID(), out var items)) return;

                var currentWeapon = GetCurrentWeapon(items);
                if (currentWeapon is null) return;

                Configs.Weapons.WeaponData weaponConfig = Configs.Weapons.GetWeaponData(currentWeapon.Type);
                if (weaponConfig is null) return;

                var ammoItem = Inventory.FindItem(items, weaponConfig.AmmoId);
                if (ammoItem != null)
                    InvGround.Drop(player, ammoItem);

                RemoveWeapon(player);

                InvGround.Drop(player, currentWeapon);
            }
            catch (Exception ex) { Logger.WriteError("OnDeath", ex); }
        }

        public static void GiveWeapon(ENetPlayer player, ItemId itemId, WeaponComponentsData componentsData, bool isRemove = false)
        {
            try
            {
                Configs.Weapons.WeaponData weaponConfig = Configs.Weapons.GetWeaponData(itemId);
                if (weaponConfig is null) return;

                var ammoCount = GetAmmo(player, itemId);

                player.SetSharedData("weapon.hash", weaponConfig.Hash);
                player.SetSharedData("weapon.components", JsonConvert.SerializeObject(componentsData));

                ClientEvent.Event(player, "client.weapon.give", weaponConfig.Hash, ammoCount, isRemove);
            }
            catch (Exception ex) { Logger.WriteError("GiveWeapon", ex); }
        }

        public static void RemoveWeapon(ENetPlayer player)
        {
            try
            {
                player.ResetSharedData("weapon.hash");
                player.ResetSharedData("weapon.components");

                ClientEvent.Event(player, "client.weapon.remove");
            }
            catch (Exception ex) { Logger.WriteError("RemoveWeapon", ex); }
        }

        [CustomEvent("server.weapon.update")]
        private static void OnUpdate(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out CharacterData characterData) || !Inventory.GetInventory(player.GetUUID(), out var items)) return;
                var currentWeapon = GetCurrentWeapon(items);
                if (currentWeapon is null) return;

                if(currentWeapon is Inv.Items.WeaponItem weaponItem)
                {
                    Configs.Weapons.WeaponData weaponConfig = Configs.Weapons.GetWeaponData(currentWeapon.Type);
                    //if (weaponConfig is null)
                    //{
                    //    Configs.Weapons.WeaponData weaponConfig = Configs.Weapons.GetWeaponData(currentWeapon.Type);
                //    if (weaponConfig is null)
                //    {
                //        ClientEvent.Event(player, "client.weapon.remove");
                    //        return;
                    //    }

                    Inventory.RemoveItem(player, weaponConfig.AmmoId, 1);

                    weaponItem.Wear -= 0.0025f;
                    weaponItem.RefreshParams();

                    var ammoCount = Inventory.GetCount(items, weaponConfig.AmmoId);
                    if (ammoCount <= 0)
                    {
                        player.RemoveItemHand();
                        InvUse.ItemUsing(player, "main", currentWeapon, -1, false);
                    }
                }

                
            }
            catch (Exception ex) { Logger.WriteError("OnUpdate", ex); }
        }
    }
}