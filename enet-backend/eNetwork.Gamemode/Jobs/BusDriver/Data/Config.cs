using eNetwork.Jobs.BusDriver.Classes;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Jobs.BusDriver.Data
{
    public class Config
    {
        public static readonly int PAYMENT = 500;

        public static readonly int WAIT_CHECKPOINT_TIME = 10;

        public static readonly List<BusParking> PARKINGS = new List<BusParking>()
        {
            new BusParking()
            {
                // Уникальный индификатор
                Id = "pillbox",

                // Позиция NPC
                Position = new Position(437.68503, -624.30835, 28.70, 90),
                
                // Точки для спавна автобусов
                SpawnPositions = new List<Position>()
                {
                    new Position(430.93634, -605.0827, 29.832283, 85)
                },

                // Точки автобусников
                Checkpoints = new List<BusCheckpoint> 
                { 
                    new BusCheckpoint(new Vector3(402.86197, -706.6017, 28.132648)),
                    new BusCheckpoint(new Vector3(403.25027, -803.7999, 28.12456)),
                    new BusCheckpoint(new Vector3(498.86307, -770.0307, 23.60636)),
                    new BusCheckpoint(new Vector3(484.7233, -673.0054, 24.78012)),
                }
            }
        };
    }
}
