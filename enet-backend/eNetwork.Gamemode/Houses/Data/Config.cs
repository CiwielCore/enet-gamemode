using eNetwork.Houses.Garage;
using eNetwork.Houses.Interior;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Houses.Data
{
    public class Config
    {
        /// <summary>
        /// Конфиг настроек типов гаражей
        /// </summary>
        public static readonly Dictionary<GarageType, GarageData> HOUSE_GARAGES = new Dictionary<GarageType, GarageData>()
        {
           { GarageType.Econom, new GarageData()  /// 0
                {
                    Position = new Position(178.9925, -1005.661, -98.9995, 175.06194),
                    Places = new List<GaragePositionPlace>()
                    {
                        new GaragePositionPlace(new Position(170.6935, -1004.269, -99.41191, 183.3471)),
                        new GaragePositionPlace(new Position(174.3777, -1003.795, -99.41129, 175.7529)),
                    },
                }
            },
            { GarageType.Comfort, new GarageData() /// 1
                {
                    Position = new Position(206.9094, -999.0917, -100, 90),
                    Places = new List<GaragePositionPlace>()
                    {
                        new GaragePositionPlace(new Position(204.1544, -997.7147, -99.41058, 166.4086)),
                        new GaragePositionPlace(new Position(200.7814, -997.5886, -99.41073, 165.095)),
                        new GaragePositionPlace(new Position(197.3544, -997.4301, -99.41062, 163.7391)),
                        new GaragePositionPlace(new Position(193.8947, -997.2777, -99.41056, 163.4609)),
                    },
                }
            },
            { GarageType.ComfortPlus, new GarageData() /// 2
                {
                    Position = new Position(206.9094, -999.0917, -100, 90),
                    Places = new List<GaragePositionPlace>()
                    {
                        new GaragePositionPlace(new Position(204.1544, -997.7147, -99.41058, 166.4086)),
                        new GaragePositionPlace(new Position(200.7814, -997.5886, -99.41073, 165.095)),
                        new GaragePositionPlace(new Position(197.3544, -997.4301, -99.41062, 163.7391)),
                        new GaragePositionPlace(new Position(193.8947, -997.2777, -99.41056, 163.4609)),
                        new GaragePositionPlace(new Position(201.9032, -1004.244, -99.41065, 163.4917)),
                        new GaragePositionPlace(new Position(196.0699, -1003.287, -99.41054, 161.4624)),
                    },
                }
            },
            { GarageType.Premium, new GarageData() /// 3
                {
                    Position = new Position(206.9094, -999.0917, -100, 90),
                    Places = new List<GaragePositionPlace>()
                    {
                        new GaragePositionPlace(new Position(204.1544, -997.7147, -99.41058, 166.4086)),
                        new GaragePositionPlace(new Position(200.7814, -997.5886, -99.41073, 165.095)),
                        new GaragePositionPlace(new Position(197.3544, -997.4301, -99.41062, 163.7391)),
                        new GaragePositionPlace(new Position(193.8947, -997.2777, -99.41056, 163.4609)),
                        new GaragePositionPlace(new Position(201.9032, -1004.244, -99.41065, 163.4917)),
                        new GaragePositionPlace(new Position(196.0699, -1003.287, -99.41054, 161.4624)),
                        new GaragePositionPlace(new Position(196.0699, -1003.287, -99.41054, 161.4624)),
                        new GaragePositionPlace(new Position(196.0699, -1003.287, -99.41054, 161.4624)),
                    },
                }
            },
            { GarageType.Luxe, new GarageData() /// 4
                {
                    Position = new Position(240.411, -1004.753, -100, 90),
                    Places = new List<GaragePositionPlace>()
                    {
                        new GaragePositionPlace(new Position(223.2661, -978.6877, -99.41358, 251.3986)),
                        new GaragePositionPlace(new Position(223.1918, -982.4593, -99.41795, 246.0103)),
                        new GaragePositionPlace(new Position(222.8921, -985.879, -99.41821, 251.0875)),
                        new GaragePositionPlace(new Position(222.8588, -989.4495, -99.41826, 248.026)),
                        new GaragePositionPlace(new Position(223.0551, -993.4521, -99.41066, 240.252)),
                        new GaragePositionPlace(new Position(223.1918, -982.4593, -99.41795, 130.5622)),
                        new GaragePositionPlace(new Position(233.6587, -983.3923, -99.41045, 130.4442)),
                        new GaragePositionPlace(new Position(234.0298, -987.5615, -99.41094, 129.4973)),
                        new GaragePositionPlace(new Position(234.0298, -991.406, -99.4104, 129.157)),
                        new GaragePositionPlace(new Position(234.2386, -995.7032, -99.41273, 128.8663)),
                    },
                }
            }
        };

        /// <summary>
        /// Конфиг настроек типов домов
        /// </summary>
        public static readonly Dictionary<HouseInteriorType, InteriorData> HOUSE_INTERIORS = new Dictionary<HouseInteriorType, InteriorData>()
        {
            { HouseInteriorType.Econom, new InteriorData() /// 0
                {
                    Name = "Эконом",
                    Position = new Position(151.36922, -1007.74976, -99, 1),
                    Storage = new Vector3(151.66605, -1003.2285, -100.11992),
                    StorageWeight = 10000 * 1000,
                }
            },
            { HouseInteriorType.Comfort, new InteriorData() /// 1
                {
                    Name = "Комфорт",
                    Position = new Position(151.36922, -1007.74976, -99, 1),
                    Storage = new Vector3(151.66605, -1003.2285, -100.11992),
                    StorageWeight = 10000 * 1000,
                }
            },
            { HouseInteriorType.ComfortPlus, new InteriorData() /// 2
                {
                    Name = "Комфорт+",
                    Position = new Position(151.36922, -1007.74976, -99, 1),
                    Storage = new Vector3(151.66605, -1003.2285, -100.11992),
                    StorageWeight = 10000 * 1000,
                }
            },
            { HouseInteriorType.Premium, new InteriorData() /// 3
                {
                    Name = "Премиум",
                    Position = new Position(151.36922, -1007.74976, -99, 1),
                    Storage = new Vector3(151.66605, -1003.2285, -100.11992),
                    StorageWeight = 10000 * 1000,
                }
            },
            { HouseInteriorType.Luxe, new InteriorData() /// 4
                {
                    Name = "Люкс",
                    Position = new Position(151.36922, -1007.74976, -99, 1),
                    Storage = new Vector3(151.66605, -1003.2285, -100.11992),
                    StorageWeight = 10000 * 1000,
                }
            }
        };
    }
}
