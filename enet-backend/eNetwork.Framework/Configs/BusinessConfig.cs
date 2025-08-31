using System;
using System.Collections.Generic;
using System.Text;
using eNetwork.Framework;
using eNetwork.Framework.Enums;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace eNetwork.Configs
{
    public class BusinessConfig
    {
        private static readonly Logger _logger = new Logger("business-config");

        #region Carroom
        public static readonly int TestdriveTime = 180;
        public static readonly Dictionary<int, ShowroomConfig> ShowroomPositions = new Dictionary<int, ShowroomConfig>()
        {
            { (int)BusinessType.Showroom,
                new ShowroomConfig()
                {
                    Testdrive = new Vector3(-23.468401, -1093.9418, 27.390215),
                    TestdriveHeading = -18,

                    Vehicle = new Vector3(-37.202522, -1093.4559, 27.386396),
                    Heading = -174f,

                    SpawnPositions = new List<Position>()
                    {
                        new Position(-40.055866, -1116.7544, 26.754816, -20),
                        new Position(-43.678726, -1117.2269, 26.755037, -20),
                        new Position(-47.19814, -1117.7194, 26.754517, -20),
                    }
                }
            },
        };
        public class ShowroomConfig
        {
            public Vector3 Testdrive;
            public float TestdriveHeading;
            public Vector3 Vehicle;
            public float Heading;

            [JsonIgnore]
            public List<Position> SpawnPositions;
        }

        public static readonly Dictionary<string, ShowroomColor> Colors = new Dictionary<string, ShowroomColor>()
        {
            { "Черный", new ShowroomColor(0, 0, 0, 0) },
            { "Серый", new ShowroomColor(100, 100, 100, 6) },
            { "Белый", new ShowroomColor(255, 255, 255, 111) },
            { "Красный", new ShowroomColor(255, 0, 0, 27) },
            { "Оранжевый", new ShowroomColor(252, 127, 3, 38) },
            { "Желтый", new ShowroomColor(252, 248, 3, 89) },
            { "Зеленый", new ShowroomColor(119, 252, 3, 53) },
            { "Голубой", new ShowroomColor(3, 177, 252, 70) },
            { "Синий", new ShowroomColor(3, 11, 252, 64) },
            { "Фиолетовый", new ShowroomColor(119, 3, 252, 145) },
        };

        public class ShowroomColor
        {
            public int Red;
            public int Green;
            public int Blue;
            public int ColorId;

            public ShowroomColor(int red, int green, int blue, int colorId)
            {
                Red = red;
                Green = green;
                Blue = blue;
                ColorId = colorId;
            }
        }
        #endregion

        public static void Load(ENetPlayer player)
        {
            try
            {
                ClientEvent.Event(player, "client.business.showroom.load", JsonConvert.SerializeObject(ShowroomPositions), JsonConvert.SerializeObject(Colors));
            }
            catch(Exception ex) { _logger.WriteError("Load", ex); }
        }
    }
}
