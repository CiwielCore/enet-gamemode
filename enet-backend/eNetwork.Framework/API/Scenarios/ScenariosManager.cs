using eNetwork.Framework.API.Scenarios.Classes;
using eNetwork.Framework.API.Scenarios.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework.API.Scenarios
{
    public class ScenariosManager
    {
        public ScenariosManager() { }

        public ScenarioData GetScenario(ScenarioType scenarioType)
        {
            ScenariosList.Data.TryGetValue(scenarioType, out ScenarioData scenarioData);
            return scenarioData;
        }
    }
}
