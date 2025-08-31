using eNetwork.Configs;
using eNetwork.Framework.Classes.Faction.Ranks;
using eNetwork.Inv;
using GTANetworkAPI;
using System.Collections.Generic;

namespace eNetwork.Framework.Classes.Faction
{
    public class Factions
    {
        public Factions(string factionName, FactionType factionType, List<RanksData> ranks, List<ENetVehicle> factionVehicles,
            Vector3 position, int factionMoney, List<Weapons.WeaponData> craftData, List<ENetPlayer> membersInFaction)
        {
            FactionName = factionName;
            FactionType = factionType;
            Ranks = ranks;
            FactionVehicles = factionVehicles;
            Position = position;
            FactionMoney = factionMoney;
            CraftData = craftData;
            MembersInFaction = membersInFaction;
        }

        public List<Weapons.WeaponData> CraftData { get; private set; }
        public int FactionMoney { get; private set; }
        public string FactionName { get; }
        public FactionType FactionType { get; }
        public List<ENetVehicle> FactionVehicles { get; private set; }
        public int MaterialsCounts { get; private set; }
        public Vector3 Position { get; }
        public List<RanksData> Ranks { get; private set; }
        public List<ENetPlayer> MembersInFaction { get; private set; }
    }

    /// <summary>
    /// Дочерний класс VehicleData для фракций.
    /// </summary>
    public class FactionVehiclesData : VehicleData
    {
        public int AccessLvl { get; private set; } = 1;

        public FactionVehiclesData(int iD, VehicleOwner owner, string numberPlate, string model, int health,
                                   float fuel, float mile, VehicleCustomization components, List<Item> items, Position position, float dirt,
                                   string parkedPlace, int accessLvl) : base(iD, owner, numberPlate, model, health, fuel, mile, components, items,
                                   position, dirt, parkedPlace)
        {
            AccessLvl = accessLvl;
        }

        public void ChangeAccessLvl(ENetPlayer player, int newLevelAccess)
        {
            if (newLevelAccess < 0)
            {
                ENet.Chat.SendMessage(player, "Значение недопустимо");
                return;
            }

            int oldAccessLevel = AccessLvl;
            AccessLvl = newLevelAccess;
            player.SendDone($"Вы успешно изменили доступ к {Model} [{NumberPlate}] c {oldAccessLevel} на {newLevelAccess}");
        }
    }
}