using eNetwork.Framework;
using eNetwork.Inv;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Configs
{
    public class Weapons
    {
        private static readonly Logger _logger = new Logger("weapons-config");
        private static readonly Dictionary<ItemId, WeaponData> _weaponData = new Dictionary<ItemId, WeaponData>()
        {
            { ItemId.Machete, new WeaponData(WeaponHash.Machete, 0, ItemId.Debug) },
            { ItemId.Crowbar, new WeaponData(WeaponHash.Crowbar, 0, ItemId.Debug) },
            { ItemId.Hammer, new WeaponData(WeaponHash.Hammer, 0, ItemId.Debug) },
            { ItemId.Wrench, new WeaponData(WeaponHash.Wrench, 0, ItemId.Debug) },
            { ItemId.Bottle, new WeaponData(WeaponHash.Bottle, 0, ItemId.Debug) },
            { ItemId.Dagger, new WeaponData(WeaponHash.Dagger, 0, ItemId.Debug) },
            { ItemId.Hatchet, new WeaponData(WeaponHash.Hatchet, 0, ItemId.Debug) },
            { ItemId.Switchblade, new WeaponData(WeaponHash.Switchblade, 0, ItemId.Debug) },
            { ItemId.Battleaxe, new WeaponData(WeaponHash.Battleaxe, 0, ItemId.Debug) },
            { ItemId.Knuckle, new WeaponData(WeaponHash.Knuckle, 0, ItemId.Debug) },
            { ItemId.Bat, new WeaponData(WeaponHash.Bat, 0, ItemId.Debug) },
            { ItemId.Molotov, new WeaponData(WeaponHash.Molotov, 0, ItemId.Debug) },
            { ItemId.Snowball, new WeaponData(WeaponHash.Snowball, 0, ItemId.Debug) },
            { ItemId.Nightstick, new WeaponData(WeaponHash.Nightstick, 0, ItemId.Debug) },
            { ItemId.Flashlight, new WeaponData(WeaponHash.Flashlight, 0, ItemId.Debug) },
            { ItemId.Knife, new WeaponData(WeaponHash.Knife, 0, ItemId.Debug) },
            { ItemId.Golfclub, new WeaponData(WeaponHash.Golfclub, 0, ItemId.Debug) },
            { ItemId.Stungun, new WeaponData(WeaponHash.Stungun, 0, ItemId.Debug) },

            { ItemId.Assaultrifle, new WeaponData(WeaponHash.Assaultrifle, 30, ItemId.Ammo762x39mm) },
            { ItemId.Pistol50, new WeaponData(WeaponHash.Pistol50, 9, ItemId.Ammo9x19mm) },
            { ItemId.Snspistol_mk2, new WeaponData(WeaponHash.Snspistol_mk2, 6, ItemId.Ammo9x19mm) },
            { ItemId.Sawnoffshotgun, new WeaponData(WeaponHash.Sawnoffshotgun, 8, ItemId.Ammo12gaBuckshots) },
            { ItemId.Advancedrifle, new WeaponData(WeaponHash.Advancedrifle, 30, ItemId.Ammo762x39mm) },
            { ItemId.Pistol_mk2, new WeaponData(WeaponHash.Pistol_mk2, 12, ItemId.Ammo9x19mm) },
            { ItemId.Pumpshotgun, new WeaponData(WeaponHash.Pumpshotgun, 8, ItemId.Ammo12gaBuckshots) },
            { ItemId.Appistol, new WeaponData(WeaponHash.Appistol, 18, ItemId.Ammo9x19mm) },
            { ItemId.Pumpshotgun_mk2, new WeaponData(WeaponHash.Pumpshotgun_mk2, 8, ItemId.Ammo12gaBuckshots) },
            { ItemId.Assaultshotgun, new WeaponData(WeaponHash.Assaultshotgun, 8, ItemId.Ammo12gaRifles) },
            { ItemId.Doublebarrelshotgun, new WeaponData(WeaponHash.Dbshotgun, 2, ItemId.Ammo12gaBuckshots) },
            { ItemId.Heavyshotgun, new WeaponData(WeaponHash.Heavyshotgun, 6, ItemId.Ammo12gaRifles) },
            { ItemId.Heavypistol, new WeaponData(WeaponHash.Heavypistol, 18, ItemId.Ammo9x19mm) },
            { ItemId.Vintagepistol, new WeaponData(WeaponHash.Vintagepistol, 7, ItemId.Ammo9x19mm) },
            { ItemId.Marksmanpistol, new WeaponData(WeaponHash.Marksmanpistol, 1, ItemId.Ammo9x19mm) },
            { ItemId.Revolver, new WeaponData(WeaponHash.Revolver, 6, ItemId.Ammo357Magnum) },
            { ItemId.Revolver_mk2, new WeaponData(WeaponHash.Revolver_mk2, 6, ItemId.Ammo357Magnum) },
            { ItemId.Doubleaction, new WeaponData(WeaponHash.Doubleaction, 6, ItemId.Ammo357Magnum) },
            { ItemId.Smg, new WeaponData(WeaponHash.Smg, 30, ItemId.Ammo45Acp) },
            { ItemId.Smg_mk2, new WeaponData(WeaponHash.Smg_mk2, 30, ItemId.Ammo45Acp) },
            { ItemId.Combatpdw, new WeaponData(WeaponHash.Combatpdw, 30, ItemId.Ammo45Acp) },
            { ItemId.Machinepistol, new WeaponData(WeaponHash.Machinepistol, 12, ItemId.Ammo9x19mm) },
            { ItemId.Minismg, new WeaponData(WeaponHash.Minismg, 20, ItemId.Ammo45Acp) },
            { ItemId.Assaultrifle_mk2, new WeaponData(WeaponHash.Assaultrifle_mk2, 30, ItemId.Ammo762x39mm) },
            { ItemId.Specialcarbine_mk2, new WeaponData(WeaponHash.Specialcarbine_mk2, 30, ItemId.Ammo556x45mm) },
            { ItemId.Specialcarbine, new WeaponData(WeaponHash.Specialcarbine, 30, ItemId.Ammo556x45mm) },
            { ItemId.Carbinerifle_mk2, new WeaponData(WeaponHash.Carbinerifle_mk2, 30, ItemId.Ammo556x45mm) },
            { ItemId.Carbinerifle, new WeaponData(WeaponHash.Carbinerifle, 30, ItemId.Ammo556x45mm) },
            { ItemId.Bullpuprifle, new WeaponData(WeaponHash.Bullpuprifle, 30, ItemId.Ammo556x45mm) },
            { ItemId.Bullpuprifle_mk2, new WeaponData(WeaponHash.Bullpuprifle_mk2, 30, ItemId.Ammo556x45mm) },
            { ItemId.Compactrifle, new WeaponData(WeaponHash.Compactrifle, 20, ItemId.Ammo762x39mm) },
            { ItemId.Heavysniper, new WeaponData(WeaponHash.Heavysniper, 6, ItemId.Ammo50bmg) },
            { ItemId.Gusenberg, new WeaponData(WeaponHash.Gusenberg, 30, ItemId.Ammo45Acp) },
            { ItemId.Heavysniper_mk2, new WeaponData(WeaponHash.Heavysniper_mk2, 6, ItemId.Ammo50bmg) },
            { ItemId.Sniperrifle, new WeaponData(WeaponHash.Sniperrifle, 10, ItemId.Ammo50bmg) },
            { ItemId.Autoshotgun, new WeaponData(WeaponHash.Autoshotgun, 8, ItemId.Ammo12gaBuckshots) },
            { ItemId.Mg, new WeaponData(WeaponHash.Mg, 54, ItemId.Ammo556x45mm) },
            { ItemId.Combatmg, new WeaponData(WeaponHash.Combatmg, 100, ItemId.Ammo556x45mm) },
            { ItemId.Combatmg_mk2, new WeaponData(WeaponHash.Combatmg_mk2, 100, ItemId.Ammo556x45mm) },
            { ItemId.Marksmanrifle_mk2, new WeaponData(WeaponHash.Marksmanrifle_mk2, 8, ItemId.Ammo762x39mm) },
            { ItemId.Assaultsmg, new WeaponData(WeaponHash.Assaultsmg, 30, ItemId.Ammo45Acp) },
            { ItemId.Navyrevolver, new WeaponData(WeaponHash.NavyRevolver, 6, ItemId.Ammo357Magnum) },
            { ItemId.Gadgetpistol, new WeaponData(0x57A4368C, 6, ItemId.Ammo357Magnum) },
            { ItemId.Combatshotgun, new WeaponData(0x5A96BA4, 8, ItemId.Ammo12gaBuckshots) },
            { ItemId.Militaryrifle, new WeaponData(0x9D1F17E6, 30, ItemId.Ammo762x39mm) },
            { ItemId.Microsmg, new WeaponData(WeaponHash.Microsmg, 20, ItemId.Ammo45Acp) },
            { ItemId.Heavyrifle, new WeaponData(0xC78D71B4, 30, ItemId.Ammo762x39mm) },
            { ItemId.Musket, new WeaponData(WeaponHash.Musket, 1, ItemId.Ammo12gaBuckshots) },
        };

        private static readonly Dictionary<string, List<string>> _weaponComponents = new Dictionary<string, List<string>>()
        {
            { "Suppressor", new List<string>() {
                "COMPONENT_AT_PI_SUPP",
                "COMPONENT_AT_SR_SUPP",
                "COMPONENT_AT_AR_SUPP",
                "COMPONENT_AT_AR_SUPP_02",
                "COMPONENT_AT_PI_SUPP_02",
            }},

            { "Grip", new List<string>() {
                "COMPONENT_AT_AR_AFGRIP",
                "COMPONENT_AT_AR_AFGRIP_02"
            }},

            { "Flashlight", new List<string>() {
                "COMPONENT_AT_PI_FLSH",
                "COMPONENT_AT_PI_FLSH_03",
                "COMPONENT_AT_PI_FLSH_02",
                "COMPONENT_AT_AR_FLSH"
            }},

            { "HolographicSight", new List<string>() {
                "COMPONENT_AT_SIGHTS",
                "COMPONENT_AT_SIGHTS_SMG"
            }},

            { "Scope", new List<string>() {
                "COMPONENT_AT_SCOPE_MACRO_MK2",
                "COMPONENT_AT_PI_RAIL_02",
                "COMPONENT_AT_SCOPE_MACRO",
                "COMPONENT_AT_SCOPE_MACRO_02",
                "COMPONENT_AT_SCOPE_MACRO_02_MK2",
                "COMPONENT_AT_SCOPE_MACRO_02_SMG_MK2",
                "COMPONENT_AT_SCOPE_SMALL",
                "COMPONENT_AT_SCOPE_MEDIUM"
            }},

            { "DefaultClips", new List<string>() {
                "COMPONENT_PISTOL_CLIP_01",
                "COMPONENT_COMBATPISTOL_CLIP_01",
                "COMPONENT_APPISTOL_CLIP_01",
                "COMPONENT_PISTOL50_CLIP_01",
                "COMPONENT_REVOLVER_CLIP_01",
                "COMPONENT_SNSPISTOL_CLIP_01",
                "COMPONENT_HEAVYPISTOL_CLIP_01",
                "COMPONENT_REVOLVER_MK2_CLIP_01",
                "COMPONENT_SNSPISTOL_MK2_CLIP_01",
                "COMPONENT_PISTOL_MK2_CLIP_01",
                "COMPONENT_VINTAGEPISTOL_CLIP_01",
                "COMPONENT_CERAMICPISTOL_CLIP_01",
                "COMPONENT_MICROSMG_CLIP_01",
                "COMPONENT_SMG_CLIP_01",
                "COMPONENT_ASSAULTSMG_CLIP_01",
                "COMPONENT_MINISMG_CLIP_01",
                "COMPONENT_SMG_MK2_CLIP_01",
                "COMPONENT_MACHINEPISTOL_CLIP_01",
                "COMPONENT_COMBATPDW_CLIP_01",
                "COMPONENT_ASSAULTSHOTGUN_CLIP_01",
                "COMPONENT_PUMPSHOTGUN_MK2_CLIP_01",
                "COMPONENT_HEAVYSHOTGUN_CLIP_01",
                "COMPONENT_ASSAULTRIFLE_CLIP_01",
                "COMPONENT_CARBINERIFLE_CLIP_01",
                "COMPONENT_ADVANCEDRIFLE_CLIP_01",
                "COMPONENT_SPECIALCARBINE_CLIP_01",
                "COMPONENT_BULLPUPRIFLE_CLIP_01",
                "COMPONENT_BULLPUPRIFLE_MK2_CLIP_01",
                "COMPONENT_SPECIALCARBINE_MK2_CLIP_01",
                "COMPONENT_ASSAULTRIFLE_MK2_CLIP_01",
                "COMPONENT_CARBINERIFLE_MK2_CLIP_01",
                "COMPONENT_COMPACTRIFLE_CLIP_01",
                "COMPONENT_MILITARYRIFLE_CLIP_01",
                "COMPONENT_MG_CLIP_01",
                "COMPONENT_COMBATMG_CLIP_01",
                "COMPONENT_COMBATMG_MK2_CLIP_01",
                "COMPONENT_GUSENBERG_CLIP_01",
                "COMPONENT_SNIPERRIFLE_CLIP_01",
                "COMPONENT_HEAVYSNIPER_CLIP_01",
                "COMPONENT_MARKSMANRIFLE_MK2_CLIP_01",
                "COMPONENT_HEAVYSNIPER_MK2_CLIP_01",
                "COMPONENT_MARKSMANRIFLE_CLIP_01",
                "COMPONENT_GRENADELAUNCHER_CLIP_01",
            }},

            { "Clip", new List<string>() {
                "COMPONENT_PISTOL_CLIP_02",
                "COMPONENT_COMBATPISTOL_CLIP_02",
                "COMPONENT_APPISTOL_CLIP_02",
                "COMPONENT_PISTOL50_CLIP_02",
                "COMPONENT_SNSPISTOL_CLIP_02",
                "COMPONENT_HEAVYPISTOL_CLIP_02",
                "COMPONENT_SNSPISTOL_MK2_CLIP_02",
                "COMPONENT_PISTOL_MK2_CLIP_02",
                "COMPONENT_VINTAGEPISTOL_CLIP_02",
                "COMPONENT_MICROSMG_CLIP_02",
                "COMPONENT_ASSAULTSMG_CLIP_02",
                "COMPONENT_MINISMG_CLIP_02",
                "COMPONENT_SMG_MK2_CLIP_02",
                "COMPONENT_MACHINEPISTOL_CLIP_02",
                "COMPONENT_COMBATPDW_CLIP_02",
                "COMPONENT_ASSAULTSHOTGUN_CLIP_02",
                "COMPONENT_HEAVYSHOTGUN_CLIP_02",
                "COMPONENT_ASSAULTRIFLE_CLIP_02",
                "COMPONENT_CARBINERIFLE_CLIP_02",
                "COMPONENT_ADVANCEDRIFLE_CLIP_02",
                "COMPONENT_SPECIALCARBINE_CLIP_02",
                "COMPONENT_BULLPUPRIFLE_CLIP_02",
                "COMPONENT_BULLPUPRIFLE_MK2_CLIP_02",
                "COMPONENT_SPECIALCARBINE_MK2_CLIP_02",
                "COMPONENT_ASSAULTRIFLE_MK2_CLIP_02",
                "COMPONENT_CARBINERIFLE_MK2_CLIP_02",
                "COMPONENT_COMPACTRIFLE_CLIP_02",
                "COMPONENT_MILITARYRIFLE_CLIP_02",
                "COMPONENT_MG_CLIP_02",
                "COMPONENT_COMBATMG_CLIP_02",
                "COMPONENT_COMBATMG_MK2_CLIP_02",
                "COMPONENT_GUSENBERG_CLIP_02",
                "COMPONENT_MARKSMANRIFLE_MK2_CLIP_02",
                "COMPONENT_HEAVYSNIPER_MK2_CLIP_02",
                "COMPONENT_MARKSMANRIFLE_CLIP_02"
            }},
        };

        public static void LoadConfig(ENetPlayer player)
        {
            ClientEvent.Event(player, "client.weaponComponents.load", JsonConvert.SerializeObject(_weaponComponents));
        }   

        public static WeaponData GetWeaponData(ItemId weaponId)
        {
            if (_weaponData.TryGetValue(weaponId, out WeaponData data)) return data;
            return null;
        }

        public class WeaponData 
        { 
            public uint Hash { get; set; }
            public int MaxAmmo { get; set; }
            public ItemId AmmoId { get; set; }

            public WeaponData(uint hash, int maxAmmo, ItemId ammoId)
            {
                Hash = hash;
                MaxAmmo = maxAmmo;
                AmmoId = ammoId;
            }

            public WeaponData(WeaponHash hash, int maxAmmo, ItemId ammoId)
            {
                Hash = (uint)hash;
                MaxAmmo = maxAmmo;
                AmmoId = ammoId;
            }
        }
    }
}
