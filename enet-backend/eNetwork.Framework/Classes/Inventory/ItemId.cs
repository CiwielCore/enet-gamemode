using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Inv
{
    public enum ItemId
    {
        Debug = 0,

        // Clothes =======
        Masks = 1,
        Pants = 2,
        Shoes = 3,
        Accessories = 4,
        Undershirt = 5,
        BodyArmor = 6,
        Top = 7,
        Hat = 8,
        Glasses = 9,
        Ears = 10,
        Watches = 11,
        Bracelets = 12,
        Bag = 13,
        Gloves = 14,
        // ===============

        // Wepaons =======
        Machete = 15,
        Crowbar = 16,
        Hammer = 17,
        Wrench = 18,
        Bottle = 19,
        Dagger = 20,
        Hatchet = 21,
        Switchblade = 22,
        Battleaxe = 23,
        Knuckle = 24,
        Bat = 25,
        Molotov = 26,
        Nightstick = 27,
        Flashlight = 28,
        Assaultrifle = 29,
        Pistol50 = 30,
        Stungun = 31,
        Snspistol_mk2 = 32,
        Knife = 33,
        Sawnoffshotgun = 34,
        Advancedrifle = 35,
        Pistol_mk2 = 36,
        Pumpshotgun = 37,
        Appistol = 38,
        Pumpshotgun_mk2 = 39,
        Assaultshotgun = 40,
        Doublebarrelshotgun = 41,
        Heavyshotgun = 42,
        Heavypistol = 43,
        Vintagepistol = 44,
        Marksmanpistol = 45,
        Revolver = 46,
        Revolver_mk2 = 47,
        Doubleaction = 48,
        Smg = 49,
        Smg_mk2 = 50,
        Combatpdw = 51,
        Machinepistol = 52,
        Minismg = 53,
        Assaultrifle_mk2 = 54,
        Specialcarbine_mk2 = 55,
        Specialcarbine = 56,
        Carbinerifle_mk2 = 57,
        Carbinerifle = 58,
        Bullpuprifle = 59,
        Bullpuprifle_mk2 = 60,
        Compactrifle = 61,
        Heavysniper = 62,
        Gusenberg = 63,
        Golfclub = 64,
        Heavysniper_mk2 = 65,
        Sniperrifle = 66,
        Autoshotgun = 67,
        Mg = 68,
        Combatmg = 69,
        Combatmg_mk2 = 70,
        Marksmanrifle_mk2 = 71,
        Parachute = 72,
        Assaultsmg = 73,
        Navyrevolver = 74,
        Gadgetpistol = 75,
        Combatshotgun = 76,
        Militaryrifle = 77,
        Microsmg = 78,
        Snowball = 79,
        Heavyrifle = 80,
        Musket = 81,
        // ===============

        // Ammo ==========
        Ammo9x19mm = 82,
        Ammo12gaBuckshots = 83,
        Ammo762x39mm = 84,
        Ammo50bmg = 85,
        Ammo556x45mm = 86,
        Ammo12gaRifles = 87,
        Ammo45Acp = 88,
        Ammo357Magnum = 89,
        // ===============

        // Weapons components
        ExtendedClip = 90,
        Grip = 91,
        HolographicSight = 92,
        Scope = 93,
        Suppressor = 94,
        FlashlightComponent = 95,
        // ===============

        // Gadgets =======
        Phone = 96,
        Tablet = 97,
        Simcard = 98,
        // ===============

        // Health ========
        FirstAidKit = 99,
        Bandages = 100,
        // ===============

        // Food ==========
        Burger = 101,
        Chips = 102,
        Dount = 103,
        Apple = 104,
        Orange = 105,
        Banana = 106,
        Pizza = 107,
        ChocolateMeteorite = 108,
        SweetCorn = 109, //swtcorn
        Salad = 110,
        Pancakes = 111,
        Hotdog = 112,
        Pie = 113,
        Potate = 114,
        Cabbage = 115,
        Pumpkin = 116,
        Corn = 117,
        Millet = 118,

        Pisswasser = 119,
        Sprunk = 120,
        eCola = 121,
        Milk = 122,
        Cocktail = 123,
        Water = 124,
        Coffe = 125,
        // ===============

        // Smokes ========
        EmptyPackRedwood = 126,
        PackOfRedwood = 127,
        Vape = 128,
        LiquidVape = 129,
        VaporizerVape = 130,
        Hqd = 131,
        Hookah = 132,
        // ===============

        // Drugs =========
        Methamphetamine = 133,
        Cocaine = 134,
        Marijuana = 135,
        // ===============

        // Other =========
        IdCard = 136,
        DmvLicense = 137,
        BankCard = 138,
        MedLic = 139,
        StateLic = 140,
        LotteryTicket = 141,
        PlayingDice = 142,
        NumberPlate = 143,
        // ===============

        // Tools =========
        Lighter = 144,
        Camera = 145,
        Microphone = 146,
        Rose = 147,
        Umbrella = 148,
        TeddyBear = 149,
        Braizer = 150,
        Binoculars = 151,
        Guitar = 152,
        GasolineCanister = 153,
        Tent = 154,
        Bonfire = 155,
        Sack = 156,
        Buckle = 157,
        Rope = 158,

        WoodenRod = 159,
        SilverRod = 160,
        GoldenRod = 161,

        FireworkSmall = 162,
        FireworkMedium = 163,
        FireworkLarge = 164,

        Dragy = 165,
        MetalDetector = 166,
        shavingMachine = 167,

        PepperSpray = 168,
        // ===============

        // Hunitng =======
        RabbitMeat = 169,
        RabbitMeatCook = 170,

        DeerMeat = 171,
        DeerMeatCock = 172,

        BoarMeat = 173,
        BoarMeatCock = 174,

        CoyoteMeat = 175,
        CoyoteMeatCock = 176,

        MiltonMeat = 177,
        MiltonMeatCock = 178,

        PantherMeat = 179,
        PantherMeatCock = 180,

        BearMeat = 181,
        BearMeatCock = 182,
        // ===============

        Bong = 183,

        Lockpick = 184,
        Programmer = 185,

        // ===============
        Rod = 186,
        RodUpgraded = 187,
        RodMk2 = 188,
        Bait = 189,

        Sterlad = 190,
        Losos = 191,
        Osetr = 192,
        BlackAmur = 193,
        Skat = 194,
        Tunec = 195,
        Malma = 196,
        Fugu = 197
        // ===============
    }
}
