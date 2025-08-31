using eNetwork.Game.Banks.Classes;
using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace eNetwork.Game.Banks.Data
{
    public class Config
    {
        /// <summary>
        /// Максимальное количество дней, когда логи будут досупны для чтения
        /// </summary>
        public static readonly int MAX_DAYS_OF_LOGS_ALIVE = 14;

        /// <summary>
        /// Максимальное количество логов, которое будет выводится в интерфейс
        /// </summary>
        public static readonly int MAX_COUNT_OF_LOGS_IN_HISTORY = 50;

        /// <summary>
        /// Список всех банков
        /// </summary>
        public static readonly List<BankPoint> BANK_POINTS = new List<BankPoint>()
        {
            new BankPoint(new Position(0, 0, 0, heading: 0), 0, new Vector3(149.83676, -1040.7944, 29.374088)),
            new BankPoint(new Position(0, 0, 0, heading: 0), 0, new Vector3(-350.6245, -49.7105, 47.92259)),
            new BankPoint(new Position(0, 0, 0, heading: 0), 0, new Vector3(314.35547, -278.81268, 53.050797)),
            new BankPoint(new Position(0, 0, 0, heading: 0), 0, new Vector3(-112.36371, 6469.186, 30.506704)),
            new BankPoint(new Position(0, 0, 0, heading: 0), 0, new Vector3(-1212.516, -330.61328, 36.667046)),
            new BankPoint(new Position(0, 0, 0, heading: 0), 0, new Vector3(1174.9863, 2706.7532, 36.97404)),
            new BankPoint(new Position(0, 0, 0, heading: 0), 0, new Vector3(248.19638, 222.85657, 105.16672)),
        };
    }
}
