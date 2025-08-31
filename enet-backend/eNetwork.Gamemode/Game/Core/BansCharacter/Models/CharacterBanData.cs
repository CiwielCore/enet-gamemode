using System;

namespace eNetwork.Game.Core.BansCharacter.Models
{
    public class CharacterBanData
    {
        public int UUID { get; set; }
        public string Admin { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public DateTime Ended { get; set; }
    }
}
