using eNetwork.Configs;
using eNetwork.Framework.Classes.Faction;
using eNetwork.Framework.Classes.Faction.Ranks;
using System.Collections.Generic;
using Vector3 = GTANetworkAPI.Vector3;

namespace eNetwork.Factions.Classes.StateFactions
{
    public class StateFactions : Framework.Classes.Faction.Factions
    {
        public StateFactions(string factionName, FactionType factionType, List<RanksData> ranks, List<ENetVehicle> factionVehicles, Vector3 position, int factionMoney, List<Weapons.WeaponData> craftData, List<ENetPlayer> membersInFaction) : base(factionName, factionType, ranks, factionVehicles, position, factionMoney, craftData, membersInFaction)
        {
        }
    }
}