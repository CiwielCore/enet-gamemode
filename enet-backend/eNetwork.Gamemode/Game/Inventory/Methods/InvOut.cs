using eNetwork.Factions;
using eNetwork.Factions.Classes;
using eNetwork.Framework;
using eNetwork.Framework.Classes;
using eNetwork.Framework.Enums;
using eNetwork.Game.HiddingBox;
using eNetwork.Houses;
using eNetwork.Inv;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace eNetwork.Game
{
    public static class InvOut
    {
        private static readonly Logger Logger = new Logger("inv-out");
        public static void OpenOut(this ENetPlayer player, List<Item> items, InvOutType outType, int extraData, int maxWeight)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)
                    || (player.GetData("outInventory.type", out InvOutType _oldOutType) && _oldOutType != outType && player.GetData("outInventory.extra", out int _oldExtra) && _oldExtra != extraData)) return;

                string outName = GetOutName(outType);

                player.SetData("outInventory.type", outType);
                player.SetData("outInventory.extra", extraData);
                player.SetData("outInventory.maxWeight", maxWeight);

                switch (outType)
                {
                    case InvOutType.FactionStock:
                        FactionId factionId = (FactionId)Enum.GetValues(typeof(FactionId)).GetValue(extraData);
                        //Faction faction = Factionnager.GetFaction(factionId);
                        //if (faction is null) return;

                        //faction.Stock.EnterStock(player);
                        break;

                    case InvOutType.HomeStorage:
                        var house = HousesManager.GetHouse(extraData);
                        if (house is null) return;

                        house.Storage.Enter(player);
                        break;

                    case InvOutType.HiddenBox:
                        var hiddenBox = HiddingBoxManager.GetHiddenBox(extraData);
                        if (hiddenBox is null) return;

                        hiddenBox.Enter(player);
                        break;
                }

                ClientEvent.Event(player, "client.outInventory.open", outName, maxWeight);
                SendItems(player, items);
            }
            catch (Exception ex) { Logger.WriteError("OpenOut", ex); }
        }

        public static void SendItems(ENetPlayer player, List<Item> items)
        {
            try
            {
                if (player is null) return;
                var list = new List<object>();
                items.ForEach(item => { list.Add(InvInterface.GetItemData(item)); });

                ClientEvent.Event(player, "client.outInventory.setItems", JsonConvert.SerializeObject(list));
            }
            catch (Exception ex) { Logger.WriteError("SendItems", ex); }
        }

        [CustomEvent("server.outInventory.close")]
        public static void Close(ENetPlayer player)
        {
            try
            {
                if (!player.IsTimeouted("outInventory.close", 1) || !player.GetCharacter(out var characterData) || !player.GetInventory(out Storage storage) || !player.GetData("outInventory.type", out InvOutType outType) || !player.GetData("outInventory.extra", out int extraData) || !player.GetData("outInventory.maxWeight", out int maxWeight)) return;

                switch (outType)
                {
                    case InvOutType.FactionStock:
                        FactionId factionId = (FactionId)Enum.GetValues(typeof(FactionId)).GetValue(extraData);
                        //Faction faction = Factionnager.GetFaction(factionId);
                        //if (faction is null) return;

                        //faction.Stock.LeaveStock(player);
                        break;

                    case InvOutType.HomeStorage:
                        var house = HousesManager.GetHouse(extraData);
                        if (house is null) return;

                        house.Storage.Leave(player);
                        break;

                    case InvOutType.HiddenBox:
                        var hiddenBox = HiddingBoxManager.GetHiddenBox(extraData);
                        if (hiddenBox is null) return;

                        hiddenBox.Leave(player);
                        break;
                }

                player.ResetData("outInventory.type");
                player.ResetData("outInventory.extra");
                player.ResetData("outInventory.maxWeight");

                ClientEvent.Event(player, "client.outInventory.close");
            }
            catch (Exception ex) { Logger.WriteError("Close", ex); }
        }

        [CustomEvent("server.outInventory.put")]
        private static void OnPut(ENetPlayer player, int index)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)
                    || !player.GetInventory(out Storage inventory)
                    || !player.GetData("outInventory.type", out InvOutType outType)
                    || !player.GetData("outInventory.extra", out int extraData)
                    || !player.GetData("outInventory.maxWeight", out int maxWeight))
                    return;

                if (!player.IsTimeouted("outInventory.put", 2))
                {
                    player.SendError("Не так часто!");
                    return;
                }

                var item = inventory.Items.ElementAt(index);
                if (item is null) return;

                if (item.Slot <= -100)
                {
                    player.SendError("Уберите предмет из быстрого слота");
                    return;
                }

                if (item.IsActive)
                {
                    player.SendError("Уберите предмет из рук");
                    return;
                }

                var itemData = InvItems.Get(item.Type);
                if (itemData is null || (itemData.ItemType == ItemType.Clothes && item.IsActive)) return;

                switch (outType)
                {
                    //case InvOutType.FactionStock:
                    //    FactionId factionId = (FactionId)Enum.GetValues(typeof(FactionId)).GetValue(extraData);
                    //    Faction faction = Factionnager.GetFaction(factionId);
                    //    if (faction is null) return;

                    //    if (faction.Stock.TryAdd(player, item))
                    //    {
                    //        Inventory.RemoveItem(player, item);
                    //        player.SendDone("Вы положили предмет на склад");
                    //    }
                    //    return;

                    case InvOutType.HomeStorage:
                        var house = HousesManager.GetHouse(extraData);
                        if (house is null) return;

                        if (house.Storage.TryAdd(player, item))
                        {
                            Inventory.RemoveItem(player, item);
                            player.SendDone("Вы положили предмет в шкаф");
                        }
                        return;

                    case InvOutType.HiddenBox:
                        var hiddenBox = HiddingBoxManager.GetHiddenBox(extraData);
                        if (hiddenBox is null) return;

                        if (hiddenBox.TryAdd(player, item))
                        {
                            Inventory.RemoveItem(player, item);
                            player.SendDone("Вы положили предмет в тайник");
                        }
                        break;
                }
            }
            catch (Exception ex) { Logger.WriteError("OnPut", ex); }
        }

        [CustomEvent("server.outInventory.take")]
        private static void OnTake(ENetPlayer player, int index)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || !player.GetInventory(out Storage inventory) || !player.GetData("outInventory.type", out InvOutType outType) || !player.GetData("outInventory.extra", out int extraData) || !player.GetData("outInventory.maxWeight", out int maxWeight)) return;

                if (!player.IsTimeouted("outInventory.take", 2))
                {
                    player.SendError("Не так часто!");
                    return;
                }

                switch (outType)
                {
                    //case InvOutType.FactionStock:
                    //    FactionId factionId = (FactionId)Enum.GetValues(typeof(FactionId)).GetValue(extraData);
                    //    Faction faction = Factionnager.GetFaction(factionId);
                    //    if (faction is null) return;

                    //    var item = faction.Stock.GetItem(index);
                    //    if (item is null) return;

                    //    var itemData = InvItems.Get(item.Type);
                    //    if (itemData is null) return;

                    //    if (!Inventory.CheckCanAddItem(player, item, -1))
                    //    {
                    //        player.SendError(Language.GetText(TextType.NoInventoryPlaces));
                    //        return;
                    //    }

                    //    faction.Stock.RemoveItem(item);
                    //    player.SendDone($"Вы взяли со склада {itemData.Name} x{item.Count}");
                    //    return;
                    case InvOutType.HomeStorage:
                        var house = HousesManager.GetHouse(extraData);
                        if (house is null) return;

                        Item item = house.Storage.GetItem(index);
                        if (item is null) return;

                        var itemData = InvItems.Get(item.Type);
                        if (itemData is null) return;

                        if (!Inventory.CheckCanAddItem(player, item, -1))
                        {
                            player.SendError(Language.GetText(TextType.NoInventoryPlaces));
                            return;
                        }

                        //if (!Inventory.TryAdd(player, item, -1))
                        //{
                        //    player.SendError(Language.GetText(TextType.NoInventoryPlaces));
                        //    return;
                        //}

                        house.Storage.RemoveItem(item);
                        //player.SendDone($"Вы взяли со шкафа {itemData.Name} x{item.Count}");
                        return;

                    case InvOutType.HiddenBox:
                        var hiddenBox = HiddingBoxManager.GetHiddenBox(extraData);
                        if (hiddenBox is null) return;

                        //if (!Inventory.CheckCanAddItem(player, item, -1))
                        //{
                        //    player.SendError(Language.GetText(TextType.NoInventoryPlaces));
                        //    return;
                        //}

                        //ItemData itemData = InvItems.Get(item.Type);
                        //if (itemData is null) return;

                        //if (!Inventory.TryAdd(player, item, -1))
                        //{
                        //    player.SendError(Language.GetText(TextType.NoInventoryPlaces));
                        //    return;
                        //}

                        //hiddenBox.RemoveItem(item);
                        //player.SendDone($"Вы взяли с тайника {itemData.Name} x{item.Count}");
                        break;
                }
            }
            catch (Exception ex) { Logger.WriteError("OnTake", ex); }
        }

        public static string GetOutName(InvOutType outType)
        {
            switch (outType)
            {
                case InvOutType.Vehicle: return "Багажник";
                case InvOutType.FactionStock: return "Склад организации";
                case InvOutType.HomeStorage: return "Шкаф";
                case InvOutType.HiddenBox: return "Тайник";
                default: return "undefined";
            }
        }
    }

    public enum InvOutType
    {
        Vehicle,
        FactionStock,
        HomeStorage,
        HiddenBox
    }
}