using eNetwork.Configs;
using eNetwork.Framework;
using eNetwork.Inv;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Weapons
{
    public class WeaponAttachment
    {
        private static Logger Logger = new Logger("weapon-attachment");

        private static List<AttachWepaonCoordinates> _attachmentWepaonCoordinates = new List<AttachWepaonCoordinates>()
        {
            new AttachWepaonCoordinates(WeaponAttachmentType.PistolAttachment, new Vector3(0.02, 0.06, 0.1), new Vector3(-100.0, 0.0, 0.0), 51826),
            new AttachWepaonCoordinates(WeaponAttachmentType.SMGAttachment, new Vector3(0.08, 0.03, -0.1), new Vector3(-80.77, 0.0, 0.0), 58271),
            new AttachWepaonCoordinates(WeaponAttachmentType.ShotgunAttachment, new Vector3(-0.1, -0.15, 0.11), new Vector3(-180.0, 0.0, 0.0), 24818),
            new AttachWepaonCoordinates(WeaponAttachmentType.RifleAttachment, new Vector3(-0.1, -0.15, -0.13), new Vector3(0.0, 0.0, 3.5), 24818),
            new AttachWepaonCoordinates(WeaponAttachmentType.SniperAttachment, new Vector3(-0.1, -0.15, 0.01), new Vector3(0.0, 0.0, 3.5), 24818),
        };

        private static Dictionary<ItemId, WeaponAttachmentType> _weaponAttachments = new Dictionary<ItemId, WeaponAttachmentType>()
        {
            { ItemId.Assaultrifle, WeaponAttachmentType.RifleAttachment },
            { ItemId.Pistol50, WeaponAttachmentType.PistolAttachment },
            { ItemId.Stungun, WeaponAttachmentType.PistolAttachment },
            { ItemId.Snspistol_mk2, WeaponAttachmentType.PistolAttachment },
            { ItemId.Sawnoffshotgun, WeaponAttachmentType.ShotgunAttachment },
            { ItemId.Advancedrifle, WeaponAttachmentType.RifleAttachment },
            { ItemId.Pistol_mk2, WeaponAttachmentType.PistolAttachment },
            { ItemId.Pumpshotgun, WeaponAttachmentType.ShotgunAttachment },
            { ItemId.Appistol, WeaponAttachmentType.PistolAttachment },
            { ItemId.Pumpshotgun_mk2, WeaponAttachmentType.ShotgunAttachment },
            { ItemId.Assaultshotgun, WeaponAttachmentType.ShotgunAttachment },
            { ItemId.Doublebarrelshotgun, WeaponAttachmentType.ShotgunAttachment },
            { ItemId.Heavyshotgun, WeaponAttachmentType.ShotgunAttachment },
            { ItemId.Heavypistol, WeaponAttachmentType.PistolAttachment },
            { ItemId.Vintagepistol, WeaponAttachmentType.PistolAttachment },
            { ItemId.Marksmanpistol, WeaponAttachmentType.PistolAttachment },
            { ItemId.Revolver, WeaponAttachmentType.PistolAttachment },
            { ItemId.Revolver_mk2, WeaponAttachmentType.PistolAttachment },
            { ItemId.Doubleaction, WeaponAttachmentType.PistolAttachment },
            { ItemId.Smg, WeaponAttachmentType.SMGAttachment },
            { ItemId.Smg_mk2, WeaponAttachmentType.SMGAttachment },
            { ItemId.Combatpdw, WeaponAttachmentType.SMGAttachment },
            { ItemId.Machinepistol, WeaponAttachmentType.PistolAttachment },
            { ItemId.Minismg, WeaponAttachmentType.SMGAttachment },
            { ItemId.Assaultrifle_mk2, WeaponAttachmentType.RifleAttachment },
            { ItemId.Specialcarbine_mk2, WeaponAttachmentType.RifleAttachment },
            { ItemId.Specialcarbine, WeaponAttachmentType.RifleAttachment },
            { ItemId.Carbinerifle_mk2, WeaponAttachmentType.RifleAttachment },
            { ItemId.Carbinerifle, WeaponAttachmentType.RifleAttachment },
            { ItemId.Bullpuprifle, WeaponAttachmentType.RifleAttachment },
            { ItemId.Bullpuprifle_mk2, WeaponAttachmentType.RifleAttachment },
            { ItemId.Compactrifle, WeaponAttachmentType.SMGAttachment },
            { ItemId.Heavysniper, WeaponAttachmentType.SniperAttachment },
            { ItemId.Gusenberg, WeaponAttachmentType.RifleAttachment },
            { ItemId.Heavysniper_mk2, WeaponAttachmentType.SniperAttachment },
            { ItemId.Sniperrifle, WeaponAttachmentType.SniperAttachment },
            { ItemId.Autoshotgun, WeaponAttachmentType.ShotgunAttachment },
            { ItemId.Mg, WeaponAttachmentType.RifleAttachment },
            { ItemId.Combatmg, WeaponAttachmentType.RifleAttachment },
            { ItemId.Combatmg_mk2, WeaponAttachmentType.RifleAttachment },
            { ItemId.Marksmanrifle_mk2, WeaponAttachmentType.SniperAttachment },
            { ItemId.Assaultsmg, WeaponAttachmentType.SMGAttachment },
            { ItemId.Navyrevolver, WeaponAttachmentType.PistolAttachment },
            { ItemId.Gadgetpistol, WeaponAttachmentType.PistolAttachment },
            { ItemId.Combatshotgun, WeaponAttachmentType.ShotgunAttachment },
            { ItemId.Militaryrifle, WeaponAttachmentType.RifleAttachment },
            { ItemId.Microsmg, WeaponAttachmentType.SMGAttachment },
            { ItemId.Heavyrifle, WeaponAttachmentType.RifleAttachment },
            { ItemId.Musket, WeaponAttachmentType.ShotgunAttachment },
        };

        private static WeaponAttachmentType GetWeaponAttachmentTypeByItemType(ItemId itemId)
        {
            return _weaponAttachments[itemId];
        }

        public static void SetWeaponInFastSlot(ENetPlayer player, ItemId itemId, WeaponComponentsData componentsData, int fastslot)
        {
            Attach(player, itemId, componentsData, fastslot);
        }

        public static void RemoveWeaponFromFastSlot(ENetPlayer player, ItemId itemId, int dropSlot)
        {
            Detach(player, dropSlot);
        }

        private static void Attach(ENetPlayer player, ItemId itemId, WeaponComponentsData componentsData, int slot)
        {
            try
            {
                var itemData = InvItems.Get(itemId);
                if (itemData is null) return;

                var weaponData = Configs.Weapons.GetWeaponData(itemData.Type);
                if (weaponData is null) return;

                WeaponAttachmentType weaponAttachmentType = GetWeaponAttachmentTypeByItemType(itemId);

                var coords = AttachWepaonCoordinates.GetAttachWepaonCoordinates(weaponAttachmentType);

                var attObj = new AttachedObject(
                    slot, 
                    weaponData.Hash,
                    coords.Bone,
                    coords.Position, 
                    coords.Rotation, 
                    componentsData
                );
                List<AttachedObject> attList = new List<AttachedObject>();
                if (!player.HasSharedData("attachedWeapons"))
                {
                    attList.Add(attObj);
                }
                else
                {
                    attList = JsonConvert.DeserializeObject<List<AttachedObject>>(player.GetSharedData<string>("attachedWeapons"));
                    attList.Add(attObj);
                }
                player.SetSharedData("attachedWeapons", JsonConvert.SerializeObject(attList));
            }
            catch (Exception ex) { Logger.WriteError("Attach", ex); }
        }

        private static void Detach(ENetPlayer player, int slot)
        {
            try
            {
                if (!player.HasSharedData("attachedWeapons")) return;
                var attList = JsonConvert.DeserializeObject<List<AttachedObject>>(player.GetSharedData<string>("attachedWeapons"));
                attList.Remove(attList.Find(x => x.Slot == slot));

                //if (attList.Count == 0) ClientEvent.EventInRange(player.Position, 250, "client.weapons.detach.all", player.Value);
                player.SetSharedData("attachedWeapons", JsonConvert.SerializeObject(attList));
            }
            catch (Exception ex) { Logger.WriteError("Detach", ex); }
        }

        #region модуль аттача
        private class AttachedObject
        {
            public int Slot { get; set; }
            public uint Model { get; set; }
            public int Bone { get; set; }
            public Vector3 PosOffset { get; set; }
            public Vector3 RotOffset { get; set; }
            public WeaponComponentsData ComponentsData { get; set; }

            public AttachedObject(int slot, uint model, int bone, Vector3 pos, Vector3 rot, WeaponComponentsData componentsData)
            {
                Slot = slot;
                Model = model;
                Bone = bone;
                PosOffset = pos;
                RotOffset = rot;
                ComponentsData = componentsData;
            }
        }
        #endregion
        #region модуль координатов аттача
        private class AttachWepaonCoordinates
        {
            public WeaponAttachmentType WeaponAttachmentType { get; set; }
            public Vector3 Position { get; set; }
            public Vector3 Rotation { get; set; }
            public int Bone { get; set; }
            public AttachWepaonCoordinates(WeaponAttachmentType weaponAttachmentType, Vector3 position, Vector3 rotation, int bone)
            {
                WeaponAttachmentType = weaponAttachmentType;
                Position = position;
                Rotation = rotation;
                Bone = bone;
            }

            public static AttachWepaonCoordinates GetAttachWepaonCoordinates(WeaponAttachmentType weaponAttachmentType)
            {
                return _attachmentWepaonCoordinates.Find(x => x.WeaponAttachmentType == weaponAttachmentType);
            }
        }
        #endregion
        private enum WeaponAttachmentType
        {
            PistolAttachment = 1,
            SMGAttachment = 2,
            ShotgunAttachment = 3,
            RifleAttachment = 4,
            SniperAttachment = 5,
        }
    }
}
