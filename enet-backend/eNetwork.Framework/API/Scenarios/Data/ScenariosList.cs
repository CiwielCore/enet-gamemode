using eNetwork.Framework.API.Scenarios.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.Scenarios.Data
{
    public class ScenariosList
    {
        public static readonly Dictionary<ScenarioType, ScenarioData> Data = new Dictionary<ScenarioType, ScenarioData>()
        {
            { ScenarioType.TakeItem, new ScenarioData("random@domestic", "pickup_low", 47, isLooped: false, isClientSync: false, stopDuration: .85) },
            { ScenarioType.DropItem, new ScenarioData("anim@heists@narcotics@trash", "drop_side", 49, isLooped: false, isClientSync: false, stopDuration: .7) },
            { ScenarioType.VehicleKey, new ScenarioData("anim@mp_player_intmenu@key_fob@", "fob_click", 49, isLooped: false, isClientSync: false) },
            { ScenarioType.HelthAidKit, new ScenarioData("anim@amb@office@boardroom@crew@female@var_b@base@", "idle_a", 49, isLooped: false, isClientSync: false) },
            { ScenarioType.TakePhone, new ScenarioData("cellphone@str", "cellphone_text_press_a", 49, isLooped: false, isClientSync: true, stopOnEnd: false) },
            { ScenarioType.TakeIpad, new ScenarioData("amb@code_human_in_bus_passenger_idles@female@tablet@idle_a", "idle_a", 49, isLooped: false, isClientSync: false, stopOnEnd: false) }
        };
    }
}
