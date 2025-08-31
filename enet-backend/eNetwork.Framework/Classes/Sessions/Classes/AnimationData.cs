using System;
using System.Collections.Generic;
using System.Text;
using static eNetwork.Configs.AnimationsConfig;

namespace eNetwork.Framework.Classes.Sessions.Classes
{
    public class AnimationData
    {
        public AnimationConfigItem CurrentAnimation { get; set; } = null;

        public string SoundId { get; set; } = string.Empty;
    }
}
