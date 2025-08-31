using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eNetwork.Framework;
using eNetwork.Inv;
using GTANetworkAPI;

namespace eNetwork.Game
{
    public class InvItems
    {
        private static readonly Logger Logger = new Logger("inventory-items");
        #region Items data
        private static List<ItemData> ItemDatas = new List<ItemData>()
        {
            // Clothes *************
            new ItemData(ItemId.Masks, "Маска", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "masks", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Pants, "Штаны", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "pants", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Shoes, "Обувь", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "shoes", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Accessories, "Аксессуар", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "accessories", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Undershirt, "Майка", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "undershirt", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.BodyArmor, "Бронежилет", "Спасет вас от пуль", NAPI.Util.GetHashKey("prop_bodyarmour_06"), "bodyarmor", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Top, "Верхняя одежда", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "top", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Hat, "Головной убор", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "hat", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Glasses, "Очки", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "glasses", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Ears, "Серьги", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "ears", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Watches, "Часы", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "watches", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Bracelets, "Браслеты", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "bracelets", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Bag, "Рюкзак", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "bag", 200, 1, ItemRarity.Common, ItemType.Clothes),
            new ItemData(ItemId.Gloves, "Перчатки", "Можно надеть на себя", NAPI.Util.GetHashKey("prop_cs_box_clothes"), "gloves", 200, 1, ItemRarity.Common, ItemType.Clothes),
            // *********************

            // Food ****************
            new ItemData(ItemId.Burger, "Бургер", "Вы можете съесть этот бургер", NAPI.Util.GetHashKey("prop_cs_burger_01"), "burger", 350, 5, ItemRarity.Common, ItemType.Food),
            new ItemData(ItemId.eCola, "eCola", "Вы можете выпить эту колу", NAPI.Util.GetHashKey("prop_beer_bottle"), "bottle", 350, 5, ItemRarity.Common, ItemType.Food),
            // *********************

            // Tools ***************
            new ItemData(ItemId.PepperSpray, "Перцовый балончик", "С помощью него можно спасаться от злых чмошников", NAPI.Util.GetHashKey("elision_prop_pepperspray"), "pepperspray", 200, 1, ItemRarity.Uncommon, ItemType.Tools),
            // *********************

            // Weapons ***************
            new ItemData(ItemId.Machete, "Мачетте", "Оружие", NAPI.Util.GetHashKey("prop_ld_w_me_machette"), "machete", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Crowbar, "Лом", "Оружие", NAPI.Util.GetHashKey("w_me_crowbar"), "сrowbar", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Hammer, "Молоток", "Оружие", NAPI.Util.GetHashKey("w_me_hammer"), "hammer", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Wrench, "Гаечный ключ", "Оружие", NAPI.Util.GetHashKey("prop_cs_wrench"), "wrench", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Bottle, "Битая бутылка", "Оружие", NAPI.Util.GetHashKey("w_me_bottle"), "bottle", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Dagger, "Кинжал", "Оружие", NAPI.Util.GetHashKey("w_me_dagger"), "dagger", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Hatchet, "Топор", "Оружие", NAPI.Util.GetHashKey("w_me_hatchet"), "hatchet", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Switchblade, "Выкидной нож", "Оружие", NAPI.Util.GetHashKey("w_me_switchblade"), "switchblade", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Battleaxe, "Боевой топор", "Оружие", NAPI.Util.GetHashKey("w_me_battleaxe"), "battleaxe", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Knuckle, "Кастет", "Оружие", NAPI.Util.GetHashKey("w_me_knuckle"), "knuckle", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Bat, "Бита", "Оружие", NAPI.Util.GetHashKey("w_me_bat"), "bat", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Molotov, "Коктейль молотова", "Оружие", NAPI.Util.GetHashKey("w_ex_molotov"), "molotov", 1000, 1, ItemRarity.Uncommon, ItemType.Consumable),
            new ItemData(ItemId.Nightstick, "Дубинка", "Оружие", NAPI.Util.GetHashKey("w_me_nightstick"), "nightstick", 1000, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Flashlight, "Фонарик", "Оружие", NAPI.Util.GetHashKey("w_me_flashlight"), "flashlight", 400, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Assaultrifle, "Штурмовая винтовка", "Оружие", NAPI.Util.GetHashKey("w_ar_assaultrifle"), "assaultrifle", 2500, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Pistol50, "Пистолет .50", "Оружие", NAPI.Util.GetHashKey("w_pi_pistol50"), "pistol50", 900, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Stungun, "Тазер", "Оружие", NAPI.Util.GetHashKey("w_pi_stungun"), "stungun", 750, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Snspistol_mk2, "Карманный пистолет Mk II", "Оружие", NAPI.Util.GetHashKey("w_pi_sns_pistolmk2"), "snspistol_mk2", 900, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Knife, "Нож", "Оружие", NAPI.Util.GetHashKey("w_me_knife_01"), "knife", 300, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Sawnoffshotgun, "Обрез", "Оружие", NAPI.Util.GetHashKey("w_sg_sawnoff"), "sawnoffshotgun", 3000, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Advancedrifle, "Улучшенная винтовка", "Оружие", NAPI.Util.GetHashKey("w_ar_advancedrifle"), "advancedrifle", 3000, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Pistol_mk2, "Пистолет Mk II", "Оружие", NAPI.Util.GetHashKey("w_pi_pistolmk2"), "pistol_mk2", 950, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Pumpshotgun, "Помповый дробовик", "Оружие", NAPI.Util.GetHashKey("w_sg_pumpshotgun"), "pumpshotgun", 3250, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Appistol, "Бронебойный пистолет", "Оружие", NAPI.Util.GetHashKey("w_pi_appistol"), "appistol", 850, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Pumpshotgun_mk2, "Помповый дробовик Mk II", "Оружие", NAPI.Util.GetHashKey("w_sg_pumpshotgunmk2"), "pumpshotgun_mk2", 3500, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Assaultshotgun, "Штурмовой дробовик", "Оружие", NAPI.Util.GetHashKey("w_sg_assaultshotgun"), "assaultshotgun", 2700, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Doublebarrelshotgun, "Двуствольное ружье", "Оружие", NAPI.Util.GetHashKey("w_sg_doublebarrel"), "doublebarrelshotgun", 1450, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Heavyshotgun, "Тяжелый дробовик", "Оружие", NAPI.Util.GetHashKey("w_sg_heavyshotgun"), "heavyshotgun", 2850, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Heavypistol, "Тяжелый пистолет", "Оружие", NAPI.Util.GetHashKey("w_pi_heavypistol"), "heavypistol", 1350, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Vintagepistol, "Винтажный пистолет", "Оружие", NAPI.Util.GetHashKey("w_pi_vintage_pistol"), "vintagepistol", 1100, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Marksmanpistol, "Пистоль", "Оружие", NAPI.Util.GetHashKey("w_pi_singleshot"), "marksmanpistol", 1150, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Revolver, "Револьвер", "Оружие", NAPI.Util.GetHashKey("w_pi_revolver"), "revolver", 1150, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Revolver_mk2, "Револьвер Mk II", "Оружие", NAPI.Util.GetHashKey("w_pi_revolvermk2"), "revolver_mk2", 1150, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Doubleaction, "Самовзводный револьвер", "Оружие", NAPI.Util.GetHashKey("mk2"), "doubleaction", 900, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Smg, "ПП", "Оружие", NAPI.Util.GetHashKey("w_sb_smg"), "smg", 1600, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Smg_mk2, "ПП Mk II", "Оружие", NAPI.Util.GetHashKey("w_sb_smgmk2"), "smg_mk2", 1600, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Combatpdw, "ПОС", "Оружие", NAPI.Util.GetHashKey("w_sb_pdw"), "combatpdw", 1400, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Machinepistol, "Tec-9", "Оружие", NAPI.Util.GetHashKey("w_sb_compactsmg"), "machinepistol", 850, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Minismg, "Мини-ПП", "Оружие", NAPI.Util.GetHashKey("w_sb_minismg"), "minismg", 1350, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Assaultrifle_mk2, "Штурмовая винтовка Mk II", "Оружие", NAPI.Util.GetHashKey("w_ar_assaultriflemk2"), "assaultrifle_mk2", 2750, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Specialcarbine_mk2, "Специальный карабин Mk II", "Оружие", NAPI.Util.GetHashKey("w_ar_specialcarbinemk2"), "specialcarbine_mk2", 2750, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Specialcarbine, "Специальный карабин", "Оружие", NAPI.Util.GetHashKey("w_ar_specialcarbine"), "specialcarbine", 2750, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Carbinerifle_mk2, "Карабиновая винтовка Mk II", "Оружие", NAPI.Util.GetHashKey("w_ar_carbineriflemk2"), "carbinerifle_mk2", 2750, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Carbinerifle, "Карабиновая винтовка", "Оружие", NAPI.Util.GetHashKey("w_ar_carbinerifle"), "carbinerifle", 2750, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Bullpuprifle, "Буллпап", "Оружие", NAPI.Util.GetHashKey("w_ar_bullpuprifle"), "bullpuprifle", 2750, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Bullpuprifle_mk2, "Буллпап Mk II", "Оружие", NAPI.Util.GetHashKey("w_ar_bullpupriflemk2"), "bullpuprifle_mk2", 2750, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Compactrifle, "Укороченная винтовка", "Оружие", NAPI.Util.GetHashKey("w_ar_assaultrifle_smg"), "compactrifle", 1600, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Heavysniper, "Тяжелая снайперская винтовка", "Оружие", NAPI.Util.GetHashKey("w_sr_heavysnipermk2"), "heavysniper", 4900, 1, ItemRarity.Rare, ItemType.Weapon),
            new ItemData(ItemId.Gusenberg, "ПП Гузенберга", "Оружие", NAPI.Util.GetHashKey("w_sb_gusenberg"), "gusenberg", 3100, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Golfclub, "Клюшка для гольфа", "Оружие", NAPI.Util.GetHashKey("w_me_gclub"), "golfclub", 400, 1, ItemRarity.Uncommon, ItemType.Melee),
            new ItemData(ItemId.Heavysniper_mk2, "Тяжелая снайперская винтовка Mk II", "Оружие", NAPI.Util.GetHashKey("w_sr_heavysnipermk2"), "heavysniper_mk2", 5100, 1, ItemRarity.Rare, ItemType.Weapon),
            new ItemData(ItemId.Sniperrifle, "Снайперская винтовка", "Оружие", NAPI.Util.GetHashKey("w_sr_sniperrifle"), "sniperrifle", 4500, 1, ItemRarity.Rare, ItemType.Weapon),
            new ItemData(ItemId.Autoshotgun, "Автоматический дробовик", "Оружие", NAPI.Util.GetHashKey("w_sg_sweeper"), "autoshotgun", 3000, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Mg, "Пулемет", "Оружие", NAPI.Util.GetHashKey("w_mg_mg"), "mg", 3500, 1, ItemRarity.Rare, ItemType.Weapon),
            new ItemData(ItemId.Combatmg, "Тяжёлый пулемёт", "Оружие", NAPI.Util.GetHashKey("w_mg_combatmg"), "combatmg", 3200, 1, ItemRarity.Rare, ItemType.Weapon),
            new ItemData(ItemId.Combatmg_mk2, "Тяжёлый пулемёт Mk II", "Оружие", NAPI.Util.GetHashKey("w_mg_combatmgmk2"), "combatmg_mk2", 3200, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Marksmanrifle_mk2, "Высокоточная винтовка", "Оружие", NAPI.Util.GetHashKey("w_sr_marksmanriflemk2"), "marksmanrifle_mk2", 4400, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Parachute, "Парашют", "Оружие", NAPI.Util.GetHashKey("p_parachute_s_shop"), "parachute", 800, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Assaultsmg, "Штурмовой ПП", "Оружие", NAPI.Util.GetHashKey("w_sb_assaultsmg"), "assaultsmg", 1200, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Navyrevolver, "Флотский револьвер", "Оружие", NAPI.Util.GetHashKey("w_pi_wep2_gun"), "navyrevolver", 1150, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Gadgetpistol, "Пистолет Перико", "Оружие", NAPI.Util.GetHashKey("w_pi_singleshoth4"), "gadgetpistol", 1150, 1, ItemRarity.Mystical, ItemType.Weapon),
            new ItemData(ItemId.Combatshotgun, "Боевой дробовик", "Оружие", NAPI.Util.GetHashKey("w_sg_pumpshotgunh4"), "combatshotgun", 2750, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Militaryrifle, "Военная винтовка", "Оружие", NAPI.Util.GetHashKey("w_ar_bullpuprifleh4"), "militaryrifle", 2750, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Microsmg, "Микро-ПП", "Оружие", NAPI.Util.GetHashKey("w_sb_microsmg"), "microsmg", 1400, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Snowball, "Снежок", "Оружие", NAPI.Util.GetHashKey("w_ex_snowball"), "snowball", 100, 1, ItemRarity.Common, ItemType.Consumable),
            new ItemData(ItemId.Heavyrifle, "Тяжёлая винтовка", "Оружие", NAPI.Util.GetHashKey("w_ar_heavyrifleh"), "heavyrifle", 2650, 1, ItemRarity.Uncommon, ItemType.Weapon),
            new ItemData(ItemId.Musket, "Мушкет", "Оружие", NAPI.Util.GetHashKey("w_ar_musket"), "musket", 1600, 1, ItemRarity.Uncommon, ItemType.Weapon),
            // *********************

            // ammo ****************
            new ItemData(ItemId.Ammo9x19mm, "9x19mm", "Патроны", NAPI.Util.GetHashKey("prop_box_ammo07a"), "ammo9x19mm", 5, 240, ItemRarity.Common, ItemType.Ammo),
            new ItemData(ItemId.Ammo12gaBuckshots, "12ga Buckshots", "Патроны", NAPI.Util.GetHashKey("prop_box_ammo07a"), "ammo12gabuckshots", 5, 120, ItemRarity.Common, ItemType.Ammo),
            new ItemData(ItemId.Ammo762x39mm, "7.62x39mm", "Патроны", NAPI.Util.GetHashKey("prop_box_ammo07a"), "ammo762x39mm", 5, 240, ItemRarity.Common, ItemType.Ammo),
            new ItemData(ItemId.Ammo50bmg, ".50 BMG", "Патроны", NAPI.Util.GetHashKey("prop_box_ammo07a"), "ammo50bmg", 15, 16, ItemRarity.Common, ItemType.Ammo),
            new ItemData(ItemId.Ammo556x45mm, "5.56x45mm", "Патроны", NAPI.Util.GetHashKey("prop_box_ammo07a"), "ammo556x45mm", 5, 240, ItemRarity.Common, ItemType.Ammo),
            new ItemData(ItemId.Ammo12gaRifles, "12ga Rifled", "Патроны", NAPI.Util.GetHashKey("prop_box_ammo07a"), "ammo12garifles", 5, 240, ItemRarity.Common, ItemType.Ammo),
            new ItemData(ItemId.Ammo45Acp, ".45ACP", "Патроны", NAPI.Util.GetHashKey("prop_box_ammo07a"), "ammo45acp", 5, 240, ItemRarity.Common, ItemType.Ammo),
            new ItemData(ItemId.Ammo357Magnum, ".357 Magnum", "Патроны", NAPI.Util.GetHashKey("prop_box_ammo07a"), "ammo357magnum", 5, 120, ItemRarity.Common, ItemType.Ammo),
            // *********************

            // weapon components ***
            new ItemData(ItemId.ExtendedClip, "Увеличенная обойма", "Можно установить на ваше оружие", NAPI.Util.GetHashKey("w_ar_assaultriflemk2_mag1"), "extendedclip", 500, 1, ItemRarity.Rare, ItemType.WeaponComponent),
            new ItemData(ItemId.Grip, "Рукоятка на оружие", "Можно установить на ваше оружие", NAPI.Util.GetHashKey("w_at_sr_barrel_1"), "grip", 500, 1, ItemRarity.Rare, ItemType.WeaponComponent),
            new ItemData(ItemId.HolographicSight, "Голографический прицел", "Можно установить на ваше оружие", NAPI.Util.GetHashKey("w_at_scope_medium"), "holographicsight", 500, 1, ItemRarity.Rare, ItemType.WeaponComponent),
            new ItemData(ItemId.Scope, "Прицел на оружие", "Можно установить на ваше оружие", NAPI.Util.GetHashKey("w_at_scope_medium"), "scope", 500, 1, ItemRarity.Rare, ItemType.WeaponComponent),
            new ItemData(ItemId.Suppressor, "Глушитель на оружие", "Можно установить на ваше оружие", NAPI.Util.GetHashKey("w_at_afgrip_2"), "suppressor", 500, 1, ItemRarity.Rare, ItemType.WeaponComponent),
            new ItemData(ItemId.FlashlightComponent, "Фонарик на оружие", "Можно установить на ваше оружие", NAPI.Util.GetHashKey("w_at_ar_flsh"), "flashlightcomponent", 500, 1, ItemRarity.Rare, ItemType.WeaponComponent),
            // *********************

            // gadgets *************
            new ItemData(ItemId.Phone, "Телефон", "Пригодится для общения с близкими", NAPI.Util.GetHashKey("prop_iphone14"), "phone", 450, 1, ItemRarity.Common, ItemType.Default),
            new ItemData(ItemId.Tablet, "Планшет", "Пригодится для общения с близкими", NAPI.Util.GetHashKey("prop_cs_tablet"), "tablet", 550, 1, ItemRarity.Common, ItemType.Default),
            // *********************

            // health **************
            new ItemData(ItemId.FirstAidKit, "Аптечка", "Пополняет здоровье персонажа на 65 единиц", NAPI.Util.GetHashKey("prop_ld_health_pack"), "firstaidkit", 200, 3, ItemRarity.Common, ItemType.Default),
            // *********************

            new ItemData(ItemId.LotteryTicket, "Лотерейный билет", "Стирайте монетой пленочку и получайте призы, от денег до машины", NAPI.Util.GetHashKey("ng_proc_paper_03a"), "lotteryTicket", 1, 1, ItemRarity.Mystical, ItemType.Default),
            new ItemData(ItemId.IdCard, "ID Карта", "Документ удостоверяющий личность", NAPI.Util.GetHashKey("ng_proc_paper_03a"), "idCard", 1, 1, ItemRarity.Legendary, ItemType.Default),

            new ItemData(ItemId.Lockpick, "Отмычка", "Взломает любую дверь автомобиля", NAPI.Util.GetHashKey("prop_ld_health_pack"), "lockpick", 200, 3, ItemRarity.Common, ItemType.Default),
            new ItemData(ItemId.Programmer, "Программатор", "Взломает все что угодно", NAPI.Util.GetHashKey("prop_ld_health_pack"), "programmer", 1000, 3, ItemRarity.Uncommon, ItemType.Default),

            // fish ****************
            new ItemData(ItemId.Rod, "Удочка", "Можно ловить рыбу", NAPI.Util.GetHashKey("prop_fishing_rod_01"), "rod", 1000, 1, ItemRarity.Uncommon, ItemType.Default),
            new ItemData(ItemId.RodUpgraded, "Улучшенная удочка", "Можно ловить рыбу", NAPI.Util.GetHashKey("prop_fishing_rod_01"), "rod_upgraded", 1000, 1, ItemRarity.Uncommon, ItemType.Default),
            new ItemData(ItemId.RodMk2, "Удочка MK2", "Можно ловить рыбу", NAPI.Util.GetHashKey("prop_fishing_rod_01"), "rod_mk2", 1000, 1, ItemRarity.Uncommon, ItemType.Default),
            new ItemData(ItemId.Bait, "Наживка", "С помощью ее можно ловить рыбу", NAPI.Util.GetHashKey("ng_proc_paintcan02a"), "bait", 10, 15, ItemRarity.Uncommon, ItemType.Default),

            new ItemData(ItemId.Sterlad, "Стерлядь", "Рыба", NAPI.Util.GetHashKey("prop_starfish_01"), "sterlad", 10, 20, ItemRarity.Uncommon, ItemType.Default),
            new ItemData(ItemId.Losos, "Лосось", "Рыба", NAPI.Util.GetHashKey("prop_starfish_01"), "losos", 10, 20, ItemRarity.Uncommon, ItemType.Default),
            new ItemData(ItemId.Osetr, "Осетр", "Рыба", NAPI.Util.GetHashKey("prop_starfish_01"), "osetr", 10, 20, ItemRarity.Uncommon, ItemType.Default),
            new ItemData(ItemId.BlackAmur, "Черный амур", "Рыба", NAPI.Util.GetHashKey("prop_starfish_01"), "black_amur", 10, 20, ItemRarity.Uncommon, ItemType.Default),
            new ItemData(ItemId.Skat, "Скат", "Рыба", NAPI.Util.GetHashKey("prop_starfish_01"), "skat", 10, 20, ItemRarity.Uncommon, ItemType.Default),
            new ItemData(ItemId.Tunec, "Тунец", "Рыба", NAPI.Util.GetHashKey("prop_starfish_01"), "tunec", 10, 20, ItemRarity.Uncommon, ItemType.Default),
            new ItemData(ItemId.Malma, "Мальма", "Рыба", NAPI.Util.GetHashKey("prop_starfish_01"), "malma", 10, 20, ItemRarity.Uncommon, ItemType.Default),
            new ItemData(ItemId.Fugu, "Фугу", "Рыба", NAPI.Util.GetHashKey("prop_starfish_01"), "fugu", 10, 20, ItemRarity.Uncommon, ItemType.Default),
            // *********************
        };

        public static ItemData Get(ItemId id)
        {
            return ItemDatas.Find(x => x.Type == id);
        }

        public static ItemData Get(int id)
        {
            return ItemDatas.Find(x => (int)x.Type == id);
        }

        public static ItemType GetType(ItemId itemId)
        {
            var data = Get(itemId);
            if (data is null) return ItemType.Default;
            else return data.ItemType;
        }
        #endregion
    }
}
