using System;
using System.Collections.Generic;
using eNetwork.Framework;
using GTANetworkAPI;
using Newtonsoft.Json;
using System.Linq;
using eNetwork.Framework.API.Scenarios.Classes;
using eNetwork.Framework.Classes.Character.Customization;
using System.Data;
using eNetwork.Inv;

namespace eNetwork
{
    public class ENetPlayer : Player
    {
        private static readonly Logger _logger = new Logger("enet-player");
        public ENetPlayer(NetHandle handle) : base(handle) { }

        public string HardwareID { get; set; }
        public string IP { get; set; }
        public string UserName { get; set; }
        public ulong UserId { get; set; }

        public CharacterData CharacterData;
        public Gender Gender;
        public void SetCharacter(CharacterData data)
        {
            CharacterData = data;
            Gender = CharacterData.CustomizationData.Gender;

            NAPI.Task.Run(() => {
                if (data is null) return;

                Name = data.Name;
                SetSharedData("player.cash", data.Cash);
                SetSharedData("player.hungry", data.Indicators.Hungry);
                SetSharedData("player.water", data.Indicators.Water);
            });
        }

        public CharacterData GetCharacter()
        {
            return CharacterData;
        }

        public bool GetCharacter(out CharacterData data)
        {
            if (CharacterData is null)
            {
                data = null;
                return false;
            }
            data = CharacterData;
            return true;
        }

        public bool IsExsist() { return GetCharacter() != null; }

        public AccountData AccountData;
        public void SetAccountData(AccountData data)
        {
            AccountData = data;
        }
        public AccountData GetAccountData() { return AccountData; }
        public bool IsAccountData() { return AccountData != null; }
        public bool GetAccountData(out AccountData data)
        {
            if (GetAccountData() is null)
            {
                data = null;
                return false;
            }

            data = GetAccountData();
            return true;
        }
		
		public SessionData SessionData;
        public void SetSessionData(SessionData data)
        {
            SessionData = data;
        }
        public SessionData GetSessionData() { return SessionData; }
        public bool IsSessionData() { return SessionData != null; }
        public bool GetSessionData(out SessionData data)
        {
            if (GetSessionData() is null)
            {
                data = null;
                return false;
            }

            data = GetSessionData();
            return true;
        }

        public int GetUUID()
        {
            if (GetCharacter() is CharacterData data) return data.UUID;
            return -1;
        }

        public string GetName(bool isFormated = true)
        {
            if (GetCharacter() is CharacterData data)
            {
                if (isFormated)
                    return Helper.FormatName(data.Name);
                else
                    return data.Name;
            }
            return Name;
        }

        private ItemId _handItem = ItemId.Debug;
        public void SetItemHand(ItemId itemId)
        {
            _handItem = itemId;
        }
        public void RemoveItemHand()
        {
            _handItem = ItemId.Debug;
        }
        public ItemId GetItemHand() { return _handItem; }

        public void SavePosition(bool isDiconnect = false)
        {
            NAPI.Task.Run(() =>
            {
                if (CharacterData is null) return;

                CharacterData.LastVector = Position;

                CharacterData.HP = Health;
                CharacterData.Armor = Armor;

                if (isDiconnect)
                    CharacterData.IsSpawned = false;
            });
        }

        public void SetDimension(uint dimension)
        {
            NAPI.Task.Run(() => Dimension = dimension);
        }

        public void SetWaypoint(double x, double y)
        {
            ClientEvent.Event(this, "client.checkpoints.waypoint.set", x, y);
        }

        public bool ChangeWallet(double money, bool notify = true)
        {
            if (CharacterData is null) return false;
            if (CharacterData.Cash + money < 0)
            {
                if (notify)
                    SendError("У вас недостаточно наличных");

                return false;
            }
            if (CharacterData.Cash + money > Double.MaxValue)
            {
                if (notify)
                    SendError("Ваши карманы денег полны");

                return false;
            }

            CharacterData.Cash += money;
            NAPI.Task.Run(() => SetSharedData("player.cash", CharacterData.Cash));

            return true;
        }

        public bool PlayScenario(ScenarioType scenarioType)
        {
            ScenarioData scenarioData = ENet.Scenarios.GetScenario(scenarioType);
            if (scenarioData is null) return false;

            scenarioData.Type = scenarioType;
            SetSharedData("player.scenario", JsonConvert.SerializeObject(scenarioData));
            return true;
        }

        public ScenarioData GetPlayedScenario()
        {
            if (!GetSharedData("player.scenario", out object data) || data is bool) return null;
            ScenarioData scenarioData = JsonConvert.DeserializeObject<ScenarioData>(data.ToString());
            return scenarioData;
        }

        public void SmoothResetAnim()
        {
            PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
        }

        public void StopScenario()
        {
            SetSharedData("player.scenario", false);
        }

        public void AddAttachment(dynamic attachment, bool remove = false)
        {
            try
            {
                if (!HasData("Attachments"))
                    SetData("Attachments", new List<uint>());

                List<uint> currentAttachments = GetData<List<uint>>("Attachments");

                uint attachmentHash = 0;

                if (attachment.GetType() == typeof(string))
                    attachmentHash = NAPI.Util.GetHashKey(attachment);
                else
                    attachmentHash = Convert.ToUInt32(attachment);

                if (attachmentHash == 0)
                {
                    Console.WriteLine($"Attachment hash couldn't be found for {attachment}");
                    return;
                }

                if (currentAttachments.IndexOf(attachmentHash) == -1) // if current attachment hasn't already been added
                {
                    if (!remove) // if it needs to be added
                    {
                        currentAttachments.Add(attachmentHash);
                    }
                }
                else if (remove) // if it was found and needs to be removed
                {
                    currentAttachments.Remove(attachmentHash);
                }

                // send updated data to clientside
                SetSharedData("attachmentsData", String.Join("|", currentAttachments.Select(a => Base36Extensions.ToBase36(a)).ToArray()));
            }
            catch (Exception e) { _logger.WriteError("AddAttachments", e); }
        }

        public bool HasAttachment(dynamic attachment)
        {
            uint attachmentHash = 0;
            List<uint> currentAttachments = new List<uint>();
            try
            {
                if (!HasData("Attachments"))
                    return false;

                currentAttachments = GetData<List<uint>>("Attachments");

                if (attachment.GetType() == typeof(string))
                    attachmentHash = NAPI.Util.GetHashKey(attachment);
                else
                    attachmentHash = Convert.ToUInt32(attachment);

                if (attachmentHash == 0)
                {
                    Console.WriteLine($"Attachment hash couldn't be found for {attachment}");
                    return false;
                }
            }
            catch (Exception e) { _logger.WriteError("HasAttachment", e); }
            return currentAttachments.IndexOf(attachmentHash) != -1;
        }

        public void ClearAttachments()
        {
            try
            {
                if (!HasData("Attachments"))
                    return;

                List<uint> currentAttachments = GetData<List<uint>>("Attachments");

                if (currentAttachments.Count > 0)
                {
                    for (int i = currentAttachments.Count - 1; i >= 0; i--)
                    {
                        AddAttachment(currentAttachments[i], true);
                    }
                }

                ResetSharedData("attachmentsData");
                SetData("Attachments", new List<uint>());
            }
            catch (Exception e) { _logger.WriteError("ClearAttachments", e); }
        }

        public bool IsTimeouted(string nameTimeout, int time)
        {
            nameTimeout = nameTimeout + "_TIMEOUT";
            if (!HasData(nameTimeout)) { SetData(nameTimeout, DateTime.Now); return true; }
            else { if (DateTime.Now > GetData<DateTime>(nameTimeout).AddSeconds(time)) { SetData(nameTimeout, DateTime.Now); return true; } }
            return false;
        }
        public void ExtKick(string reason)
        {
            ClientEvent.Event(this, "client.kick", reason);
            _logger.WriteInfo("Kicked: " + reason);
            Kick(reason);
        }

        public void SendDone(string text, int time = 3000)
        {
            ClientEvent.Event(this, "client.notification.done", text, time);
        }

        public void SendError(string text, int time = 3000)
        {
            ClientEvent.Event(this, "client.notification.error", text, time);
        }

        public void SendWarning(string text, int time = 3000)
        {
            ClientEvent.Event(this, "client.notification.warn", text, time);
        }

        public void SendInfo(string text, int time = 3000)
        {
            ClientEvent.Event(this, "client.notification.info", text, time);
        }

        public bool GetData<T>(string key, out T result)
        {
            if (!HasData(key))
            {
                result = default(T);
                return false;
            }

            result = GetData<T>(key);
            return true;
        }

        public bool GetSharedData<T>(string key, out T result)
        {
            if (!HasSharedData(key))
            {
                result = default(T);
                return false;
            }

            result = GetSharedData<T>(key);
            return true;
        }

        public void Freeze(bool state = false)
        {
            SetSharedData("IS_FREEZED", state);
        }

        public void Invisibility(bool state = false)
        {
            SetSharedData("IS_INVISIBILITY", state);
        }

        public void Godmode(bool state = false)
        {
            SetSharedData("IS_GODMODE", state);
        }

        public bool ApplyCustomization()
        {
            try
            {
                if (!GetCharacter(out CharacterData data)) return false;

                CustomizationData customizationData = data.CustomizationData;
                Gender gender = customizationData.Gender;

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (gender == Gender.Male) SetSkin(PedHash.FreemodeMale01);
                        else SetSkin(PedHash.FreemodeFemale01);

                        SetClothes(2, customizationData.Hair.Hair, 0);
                        NAPI.Player.SetPlayerHairColor(this, (byte)customizationData.Hair.Color, (byte)customizationData.Hair.HighlightColor);

                        int torsoVariation = 15;

                        bool undershirtIsTop = false;
                        int undershirtVariation = customizationData.Clothes.Undershit.DrawableId;
                        if (customizationData.Clothes.Top.DrawableId == Configs.CustomizationConfig.EmptySlots[gender][ItemId.Top][0])
                        {
                            if (customizationData.Clothes.Undershit.DrawableId == Configs.CustomizationConfig.EmptySlots[gender][ItemId.Undershirt][0])
                            {
                                if (Configs.CustomizationConfig.Tops[gender].TryGetValue(customizationData.Clothes.Top.DrawableId, out Configs.CustomizationConfig.Top topData))
                                {
                                    torsoVariation = topData.Torso;
                                }
                            }
                            else
                            {
                                if (Configs.CustomizationConfig.Undershirts[gender][customizationData.Clothes.Torso.DrawableId].TryGetValue(customizationData.Clothes.Undershit.DrawableId, out Configs.CustomizationConfig.Undershirt undershirtData))
                                {
                                    if (undershirtData.Top != -1)
                                    {
                                        SetClothes(11, undershirtData.Top, customizationData.Clothes.Undershit.TextureId);
                                        SetClothes(3, Configs.CustomizationConfig.EmptySlots[gender][ItemId.Undershirt][0], 0);
                                        undershirtIsTop = true;

                                        if (Configs.CustomizationConfig.Tops[gender].TryGetValue(undershirtData.Top, out Configs.CustomizationConfig.Top topData))
                                        {
                                            torsoVariation = topData.Torso;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Configs.CustomizationConfig.Undershirts[gender][customizationData.Clothes.Torso.DrawableId].TryGetValue(customizationData.Clothes.Undershit.DrawableId, out Configs.CustomizationConfig.Undershirt undershirtData))
                            {
                                if (Configs.CustomizationConfig.Tops[gender].TryGetValue(customizationData.Clothes.Top.DrawableId, out Configs.CustomizationConfig.Top mainTopData))
                                {
                                    if (mainTopData.IsUnderCropped && undershirtData.Cropped != -1)
                                    {
                                        undershirtVariation = undershirtData.Cropped;
                                    }
                                }
                            }
                        }

                        SetClothes(3, customizationData.Clothes.Torso.DrawableId, 0);
                        SetClothes(4, customizationData.Clothes.Leg.DrawableId, customizationData.Clothes.Leg.TextureId);
                        SetClothes(5, customizationData.Clothes.Bag.DrawableId, customizationData.Clothes.Bag.TextureId);
                        SetClothes(6, customizationData.Clothes.Feet.DrawableId, customizationData.Clothes.Feet.TextureId);
                        SetClothes(7, customizationData.Clothes.Accessory.DrawableId, customizationData.Clothes.Accessory.TextureId);
                        SetClothes(9, customizationData.Clothes.Bodyarmor.DrawableId, customizationData.Clothes.Bodyarmor.TextureId);
                        SetClothes(10, customizationData.Clothes.Decals.DrawableId, customizationData.Clothes.Decals.TextureId);

                        if (!undershirtIsTop)
                        {
                            SetClothes(11, customizationData.Clothes.Top.DrawableId, customizationData.Clothes.Top.TextureId);
                            SetClothes(8, undershirtVariation, customizationData.Clothes.Undershit.TextureId);
                        }

                        foreach (var tattooList in customizationData.Tattoos.Values)
                        {
                            foreach (PlayerTattooData playerTattoo in tattooList)
                            {
                                if (playerTattoo is null) continue;
                                var decoration = new Decoration()
                                {
                                    Collection = NAPI.Util.GetHashKey(playerTattoo.Collection),
                                    Overlay = NAPI.Util.GetHashKey(playerTattoo.Overlay)
                                };

                                SetDecoration(decoration);
                            }
                        }
                        
                        SetSharedData("player.tattoos", JsonConvert.SerializeObject(customizationData.Tattoos));

                        var headBlend = new HeadBlend()
                        {
                            ShapeFirst = (byte)customizationData.parentData.Mother,
                            ShapeSecond = (byte)customizationData.parentData.Father,
                            ShapeThird = 0,

                            SkinFirst = (byte)customizationData.parentData.Mother,
                            SkinSecond = (byte)customizationData.parentData.Father,
                            SkinThird = 0,

                            ShapeMix = customizationData.parentData.Similarity,
                            SkinMix = customizationData.parentData.SkinSimilarity,
                            ThirdMix = 0.0f,
                        };

                        NAPI.Player.SetPlayerHeadBlend(this, headBlend);

                        for (int i = 0; i < customizationData.Features.Length; i++)
                            NAPI.Player.SetPlayerFaceFeature(this, i, customizationData.Features[i]);

                        for (int i = 0; i < customizationData.Appearance.Length; i++)
                        {
                            var headOverlay = new HeadOverlay();
                            headOverlay.Index = (byte)customizationData.Appearance[i].Value;
                            headOverlay.Opacity = (byte)customizationData.Appearance[i].Opacity;
                            if (i == 1) headOverlay.Color = (byte)customizationData.otherCustomize.BreadColor;
                            else if (i == 3) headOverlay.Color = 1;
                            else if (i == 2) headOverlay.Color = (byte)customizationData.otherCustomize.EyeBrowColor;
                            else if (i == 5) headOverlay.Color = (byte)customizationData.otherCustomize.BlushColor;
                            else if (i == 8) headOverlay.Color = (byte)customizationData.otherCustomize.LipstickColor;
                            else if (i == 10) headOverlay.Color = (byte)customizationData.otherCustomize.ChestHairColor;
                            headOverlay.SecondaryColor = 100;

                            NAPI.Player.SetPlayerHeadOverlay(this, i, headOverlay);
                        }
                        NAPI.Player.SetPlayerEyeColor(this, (byte)customizationData.otherCustomize.EyeColor);
                    }
                    catch (Exception ex) { _logger.WriteError("ApplyCustomization.Task", ex); }
                });

                return true;
            }
            catch (Exception ex) { _logger.WriteError("ApplyCustomization", ex); return false; }
        }
    }
}
