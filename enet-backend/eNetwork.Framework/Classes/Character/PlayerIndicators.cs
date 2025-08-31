using eNetwork.Framework;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork
{
    public class PlayerIndicators
    {
        private static readonly Logger _logger = new Logger("player-indicators");
        public double Hungry { get; set; } = 100;
        public double Water { get; set; } = 100;

        public void SetWater(ENetPlayer player, double water)
        {
            if (Water + water > 100)
                Water = 100;
            else if (Water + water < 0)
                Water = 0;
            else
                Water += water;

            player.SetSharedData("player.water", Water);
        }

        public void SetHungry(ENetPlayer player, double hungry)
        {
            if (Hungry + hungry > 100)
                Hungry = 100;
            else if (Hungry + hungry < 0)
                Hungry = 0;
            else
                Hungry += hungry;

            player.SetSharedData("player.hungry", Hungry);
        }

        public void Interval(ENetPlayer player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    if (!player.GetCharacter(out var characterData) || characterData.Status > PlayerRank.Media || player.Health <= 0) return;

                    double modifier = 1;
                    if (player.IsInVehicle)
                        modifier = .5;

                    double waterMinus = .25;
                    if (Hungry <= 5)
                        waterMinus = .5;
                    else
                        waterMinus = .25;

                    SetWater(player, -waterMinus * modifier);

                    double hungryMinus = .1;
                    if (Water <= 5)
                        hungryMinus = .25;
                    else
                        hungryMinus = .1;

                    SetHungry(player, -hungryMinus * modifier);

                    if (DateTime.Now.Minute % 5 == 0)
                    {
                        if (Hungry == 0)
                        {
                            player.SendWarning("Вы проголодались, вам нужно поесть!");
                        }
                        else if (Water == 0)
                        {
                            player.SendWarning("Вы истощились, вам нужно попить воды!");
                        }
                        else
                        {
                            player.SendWarning("Вы слишком истощились, вам нужно поесть!");
                        }
                    }
                });
            }
            catch(Exception ex) { _logger.WriteError("Interval", ex); }
        }
    }
}
