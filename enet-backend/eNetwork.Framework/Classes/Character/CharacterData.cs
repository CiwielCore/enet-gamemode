using eNetwork.Framework.Classes.Faction;
using eNetwork.Framework.Enums;
using GTANetworkAPI;
using System;

namespace eNetwork.Framework
{
    public class CharacterData
    {
        public int UUID { get; set; }
        public string Name { get; set; }
        public double Cash { get; set; } = 0;
        public int BankID { get; set; } = -1;
        public int HP { get; set; } = 100;
        public int Armor { get; set; } = 0;
        public int Lvl { get; set; } = 0;
        public int Exp { get; set; } = 0;
        public bool IsCuffed { get; set; } = false;
        public DateTime BirthDay { get; set; } = DateTime.Now;
        public PlayerRank Status { get; set; } = PlayerRank.Player;

        public int FactionId { get; set; } = -1;

        public Factions Faction { get; set; }

        public int FactionRank { get; set; } = 0;

        public bool isLeader { get; set; } = false;

        public JobId JobId { get; set; } = JobId.None;

        public AdminData AdminData { get; set; } = new AdminData();
        public int Warn { get; set; } = 0;

        public CharacterStats Stats { get; set; } = new CharacterStats();
        public CustomizationData CustomizationData { get; set; } = new CustomizationData();
        public PlayerIndicators Indicators { get; set; } = new PlayerIndicators();
        public Vector3 LastVector { get; set; } = new Vector3();

        public bool IsSpawned { get; set; } = false;
        public Vector3 ExteriosPosition { get; set; } = null;
    }

    public enum PlayerRank
    {
        Player,
        Media,
        Helper,
        JuniorAdmin,
        Admin,
        SeniorAdmin,
        Curator,
        CheifCurator,
        PRManager,
        DepCheifAdmin,
        CheifAdmin,
        Owner
    }
}