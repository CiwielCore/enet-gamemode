using eNetwork.Framework;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Synchronization
{
    public class ScenariosSync
    {
        private static readonly Logger Logger = new Logger("scenario-sync");

        [CustomEvent("server.scenarios.stop")]
        public static void OnEnd(ENetPlayer player)
        {
            try
            {
                if (!player.IsTimeouted("scenarios.stop", 1)) return;

                var scenarioData = player.GetPlayedScenario();
                if (scenarioData is null) return;

                player.StopScenario();
                switch(scenarioData.Type)
                {
                    case ScenarioType.VehicleKey:
                        player.AddAttachment("carkey", remove: true);
                        break;
                    case ScenarioType.HelthAidKit:
                        if (player.HasData("item.health.data"))
                        {
                            int healthCount = player.GetData<int>("item.health.data");
                            int playerHealth = 0;
                            if (player.Health + healthCount > 100)
                                playerHealth = player.Health = 100;
                            else
                                playerHealth = player.Health + healthCount;

                            player.Health = playerHealth;

                            player.SendDone($"Здоровье пополнено до {playerHealth} процентов!");

                            player.ResetData("item.health.data");
                            player.AddAttachment("health_pack", remove: true);
                            player.RemoveItemHand();
                        }
                        break;
                }

                player.SmoothResetAnim();
            }
            catch(Exception ex) { Logger.WriteError("OnEnd", ex); }
        }
    }
}
