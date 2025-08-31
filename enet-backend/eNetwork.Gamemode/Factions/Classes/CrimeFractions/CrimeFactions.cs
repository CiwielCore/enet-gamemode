using eNetwork.Configs;
using GTANetworkAPI;
using System.Collections.Generic;
using eNetwork.Framework.Classes.Faction;
using eNetwork.Framework.Classes.Faction.Ranks;

namespace eNetwork.Factions.Classes.CrimeFractions
{
    public class CrimeFactions : eNetwork.Framework.Classes.Faction.Factions
    {
        public CrimeFactions(string factionName, FactionType factionType, List<RanksData> ranks, List<ENetVehicle> factionVehicles, Vector3 position, int factionMoney, List<Weapons.WeaponData> craftData, List<ENetPlayer> membersInFaction) : base(factionName, factionType, ranks, factionVehicles, position, factionMoney, craftData, membersInFaction)
        {
        }
    }
}