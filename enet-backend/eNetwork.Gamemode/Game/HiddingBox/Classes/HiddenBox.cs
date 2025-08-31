using eNetwork.Framework;
using eNetwork.Game.HiddingBox.Data;
using eNetwork.Houses;
using eNetwork.Inv;
using GTANetworkAPI;
using GTANetworkInternals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Game.HiddingBox.Classes
{
    public class HiddenBox
    {
        private static readonly Logger Logger = new Logger(nameof(HiddenBox));

        public int Id { get; set; }
        public HiddenBoxType Type { get; set; }
        public Position Position { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
        public bool IsLocked { get; set; } = true;
        public DateTime Cooldown { get; set; } = DateTime.Now;

        public List<ENetPlayer> PlayersInBox { get; set; } = new List<ENetPlayer>();

        public HiddenBox(HiddenBoxType type, Position position)
        {
            Type = type;
            Position = position;                                                                                        
        }

        public HiddenBoxSettings Settings { get; set; }

        private ENetColShape _colShape { get; set; }
        private GTANetworkAPI.Object _object { get; set; }
        public void GTAElements()
        {
            Settings = Config.HIDDEN_BOX_SETTINGS[Type].Clone();

            _colShape = ENet.ColShape.CreateCylinderColShape(Position.GetVector3(), 2, 2, 0, ColShapeType.HiddenBox);
            _colShape.OnEntityEnterColShape += (s, e) => e.SetData("hiddenBox", this);
            _colShape.OnEntityExitColShape += (s, e) => e.ResetData("hiddenBox");
            _colShape.SetInteractionText(Settings.Name);

            _object = NAPI.Object.CreateObject(Settings.Model, Position.GetVector3(), new Vector3(0, 0, Position.GetHeading()), 255, 0);

            NAPI.TextLabel.CreateTextLabel($"Тайник #{Id}", Position.GetVector3(), 10f, 5f, 4, new Color(255, 255, 255, 255), false, 0);

            UpdateCooldown();
        }

        public void Interaction(ENetPlayer player)
        {
            try
            {
                if (!player.GetSessionData(out var sessionData)) return;

                if (IsLocked)
                {
                    if (sessionData.TimersData.HiddenBox != null)
                    {
                        player.SendError("Вы уже взламываете тайник...");
                        return;
                    }

                    sessionData.TimersData.HiddenBox = Timers.Start(Config.TIME_TO_UNLOCK_BOX * 1000, () =>
                    {
                        try
                        {
                            if (sessionData.TimersData.HiddenBox != null)
                            {
                                Timers.Stop(sessionData.TimersData.HiddenBox);
                                sessionData.TimersData.HiddenBox = null;
                            }

                            if (Items.Count == 0)
                            {
                                player.SendError("Тайник пуст...");
                                return;
                            }

                            IsLocked = false;
                            player.OpenOut(Items, InvOutType.HiddenBox, Id, Config.BOX_WEIGHT);
                        }
                        catch(Exception ex) { Logger.WriteError("TryOpen.Timer", ex); }
                    });

                    player.SendInfo("Начался взлом");
                }
                else
                {
                    player.OpenOut(Items, InvOutType.HiddenBox, Id, Config.BOX_WEIGHT);
                }
            }
            catch(Exception ex) { Logger.WriteError("TryOpen", ex); }
        }

        public void Worker()
        {
            try
            {
                if (Cooldown > DateTime.Now) return;

                foreach(var player in PlayersInBox)
                {
                    InvOut.Close(player);
                }

                PlayersInBox.Clear();

                IsLocked = true;
                var itemId = Settings.Items[ENet.Random.Next(0, Settings.Items.Count)];

                var item = new Item(itemId, 1);
                Items.Add(item);

                UpdateCooldown();
            }
            catch(Exception ex) { Logger.WriteError("Worker", ex); }
        }

        // Storage
        public void Enter(ENetPlayer player)
        {
            try
            {
                if (!PlayersInBox.Contains(player))
                    PlayersInBox.Add(player);
            }
            catch (Exception ex) { Logger.WriteError("Enter", ex); }
        }

        public void Leave(ENetPlayer player)
        {
            try
            {
                if (PlayersInBox.Contains(player))
                    PlayersInBox.Remove(player);
            }
            catch (Exception ex) { Logger.WriteError("Leave", ex); }
        }

        public bool TryAdd(ENetPlayer player, Item item)
        {
            try
            {
                var add = AddItem(item);
                if (!add)
                {
                    player.SendError("Недостаточно места на складе");
                    return false;
                }

                return true;
            }
            catch (Exception ex) { Logger.WriteError("TryAdd", ex); return false; }
        }

        public bool AddItem(Item item)
        {
            try
            {
                var itemData = InvItems.Get(item.Type);
                if (itemData is null) return false;

                int weight = itemData.Weight * item.Count;

                if (GetWeight() + weight > Config.BOX_WEIGHT) return false;

                item.IsActive = false;
                item.Slot = -1;
                Items.Add(item);

                PlayersInBox.ForEach((player) => InvOut.SendItems(player, Items));
                return true;
            }
            catch (Exception ex) { Logger.WriteError("AddItem", ex); return false; }
        }

        public Item GetItem(int index)
        {
            try
            {
                if (index < 0 || index >= Items.Count) return null;

                var item = Items.ElementAt(index);
                if (item is null) return null;

                return item;
            }
            catch (Exception ex) { Logger.WriteError("GetItem", ex); return null; }
        }

        public bool RemoveItem(Item item, bool remove = true)
        {
            try
            {
                if (!Items.Contains(item)) return false;

                if (remove)
                {
                    Items.Remove(item);
                    PlayersInBox.ForEach((player) => InvOut.SendItems(player, Items));
                }

                return true;
            }
            catch (Exception ex) { Logger.WriteError("RemoveItem", ex); return false; }
        }

        public bool RemoveItem(int index, bool remove = true)
        {
            try
            {
                var item = GetItem(index);
                if (item is null) return false;

                return RemoveItem(item, remove);
            }
            catch (Exception ex) { Logger.WriteError("RemoveItem", ex); return false; }
        }

        public int GetWeight()
        {
            int weight = 0;
            Items.ForEach(item =>
            {
                var itemData = InvItems.Get(item.Type);
                if (itemData != null)
                    weight += itemData.Weight * item.Count;
            });

            return weight;
        }

        public void UpdateCooldown()
        {
            Cooldown = DateTime.Now.AddMinutes(ENet.Random.Next(Config.MIN_TIME_TO_REFRESH, Config.MAX_TIME_TO_REFRESH));
        }
    }
}
