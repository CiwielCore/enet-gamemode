using eNetwork.Configs;
using eNetwork.Framework;
using eNetwork.Jobs.Builder.Classes;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Jobs.Builder.Data
{
    internal class Config
    {
        /// <summary>
        /// Позиция взятие коробок
        /// </summary>
        public static List<Position> PointsArray = new List<Position>
        {
            new Position(-129.48448f, -1016.1825f, 26.155222f, 0),
            new Position(-155.17737f, -1020.3012f, 20.156868f, 0),
            new Position(-129.03355f, -1105.9127f, 20.565239f, 0),
            new Position(-154.77936f, -1082.9794f, 20.56525f, 0),
        };

        /// <summary>
        /// Позиция сдачи коробки
        /// </summary>
        public static Position DeliveryPoint = new Position(-119.505684, -1029.7946, 26.197475, 0);

        /// <summary>
        ///  Позиция ПЕДа для работы
        /// </summary>
        public static Position PedPosition = new Position(-169.24393f, -1026.9191f, 27.302148f, 200.0f);

        /// <summary>
        /// Мужская одежда
        /// </summary>
        public static ClothesBuilder[] MaleClothes =
        {
            new(8, 59, 1), // undershirts
            new(4, 55, 1), // Legs
            new(6, 3, 0), // Shoes
            new(3, 0, 0) // Torsos

        };

        /// <summary>
        /// Женская одежда
        /// </summary>
        public static ClothesBuilder[] WomenClothes = {
            new(8, 219, 0), // undershirts
            new(6, 3, 0), // Shoes
            new(4, 58, 1), // Legs
            new(3, 0, 0), // Torsos
        };

        /// <summary>
        /// Сумма зарплаты
        /// </summary>
        public static int Sallary = 200;
    }
}