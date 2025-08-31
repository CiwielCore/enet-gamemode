using eNetwork;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using Vector3 = GTANetworkAPI.Vector3;

namespace eNetwork.Factions.Data
{
    public class FactionsData
    {
        /// <summary>
        /// Расположение фракций.
        /// </summary>
        public static readonly List<FactionsPosition> FactionsPosition = new List<FactionsPosition>()
        {
            new(new Vector3(0,0,0), "The Families"),
            new(new Vector3(0,0,0), "Ballas"),
            new(new Vector3(0,0,0), "Bloods"),
            new(new Vector3(0,0,0), "Vagos"),
            new(new Vector3(0,0,0), "Marabunta Grande"),
            new(new Vector3(0,0,0), "Полицейский департамент"),
            new(new Vector3(0,0,0), "ФРБ"),
            new(new Vector3(0,0,0), "Правительство"),
            new(new Vector3(0,0,0), "Департамент Шерифа"),
            new(new Vector3(0,0,0), "Федеральная тюрьма"),
            new(new Vector3(0,0,0), "Национальная гвардия"),
            new(new Vector3(0,0,0), "Больница"),
            new(new Vector3(0,0,0), "СМИ"),
        };

        /// <summary>
        /// Тюпл содержащий позиции NPC фракций. Position - позиция, string1 - название модели NPC, string2 - Отображаемое имя NPC
        /// </summary>
        public static readonly Tuple<Position, string, string>[] FactionNPCData =
        {
            new Tuple<Position, string, string> (new Position(-132.80724f, -1564.1393f, 34.23065f, 48.81374f), "g_m_y_famca_01",  "Гангстер"), // FAMILY
            new Tuple<Position, string, string> (new Position(99.34245f, -1958.467f, 20.71033f, -1.6179976f),"g_m_y_ballaorig_01", "Гангстер"), // BALLAS
            new Tuple<Position, string, string>(new Position(171.00041f, -1723.2098f, 29.3717f, 126.85632f), "g_m_y_strpunk_02", "Гангстер"), // BLOODS
            new Tuple<Position, string, string>(new Position(336.7795f, -2030.8362f, 21.403505f, 148.80634f), "g_m_y_mexgoon_01",  "Гангстер"), // VAGOS
            new Tuple<Position, string, string>( new Position(1383.5775f, -2079.203f, 51.978544f, 39.60058f),   "g_m_y_salvagoon_01",  "Гангстер"), // MARABUNTE
            new Tuple<Position, string, string>( new Position(-1111.7598f, -824.6581f, 18.195807f, -148.0f),   "g_m_y_famca_01",   "Дежурный офицер"), // Полицейский департамент
            new Tuple<Position, string, string>( new Position(140.58221f, -713.16736f, 32.01993f, -1.5351123f),  "g_m_y_ballaorig_01",   "Агент"), // ФРБ
            new Tuple<Position, string, string>( new Position(-545.0856f, -204.10904f, 37.09509f, -151.01405f),  "g_m_y_strpunk_02",   "Помощник"), // Правительство
            new Tuple<Position, string, string>( new Position(-442.99142f, 6016.688f, 30.59222f, -44.564236f),  "g_m_y_mexgoon_01",   "Помощник шерифа"), // Департамент Шерифа
            new Tuple<Position, string, string>( new Position(1845.6338f, 2585.733f, 44.552002f, -90.87068f),  "g_m_y_salvagoon_01",   "Надзиратель"), // Федеральная тюрьма
            new Tuple<Position, string, string>( new Position(-2340.4314f, 3266.0884f, 31.705858f, 92.38414f),  "g_m_y_strpunk_02",  "Сержант гвардии"), // Национальная гвардия
            new Tuple<Position, string, string>( new Position(343.1719f, -1397.8823f, 31.389093f, -96.7034f),  "g_m_y_mexgoon_01",  "Дежурный врач"), // Больница ЛС
            new Tuple<Position, string, string>( new Position(1847.3539f, 3678.0596f, 33.16021f, 93.93683f),  "g_m_y_mexgoon_01",  "Дежурный врач"), // Больница Палето
            new Tuple<Position, string, string>( new Position(-599.9011f, -930.7559f, 22.744688f,108.948326f),  "g_m_y_salvagoon_01",  "Репортер"), // СМИ
        };

        /// <summary>
        /// Информация о блипах для фракций.
        /// </summary>
        public static List<BlipSpritePlusColor> BlipData = new List<BlipSpritePlusColor>()
        {
            new(84, 2), // The Families
            new(84, 7), // Ballas
            new(84, 1), // Bloods
            new(84, 5), // Vagos
            new(84, 3), // Marabunta Grande
            new(60, 38), // Полицейский департамент
            new(60, 55), // ФРБ
            new(419, 0), // Правительство
            new(60, 31), // Департамент Шерифа
            new(768, 42), // Федеральная тюрьма
            new(557, 52), // Национальная гвардия
            new(184, 1), // Больница
            new(153, 6), // СМИ
        };

        /// <summary>
        /// Расположение складов для фракций.
        /// </summary>
        public static FactionStock[] FactionStockPosition = new FactionStock[]
        {
            new(new Vector3(-103.362305f, -1586.9528f, 30.572638f), new Vector3(0.0f ,0.0f, 0.0f)), // The Families
            new(new Vector3(86.123726f, -1971.7351f, 19.627457f), new Vector3(0.0f ,0.0f, 0.0f)), // Ballas
            new(new Vector3(139.1661f, -1694.0887f, 28.171661f), new Vector3(0.0f ,0.0f, 0.0f)), // Bloods
            new(new Vector3(336.95972f, -2035.7473f, 20.209341f), new Vector3(0.0f ,0.0f, 0.0f)), // Vagos
            new(new Vector3(1377.3091f, -2079.1692f, 50.878506f), new Vector3(0.0f ,0.0f, 0.0f)), // Marabunta Grande
            new(new Vector3(0.0f ,0.0f, 0.0f), new Vector3(0.0f ,0.0f, 0.0f)), // Полицейский департамент
            new(new Vector3(0.0f ,0.0f, 0.0f), new Vector3(0.0f ,0.0f, 0.0f)), // ФРБ
            new(new Vector3(0.0f ,0.0f, 0.0f), new Vector3(0.0f ,0.0f, 0.0f)), // Правительство
            new(new Vector3(0.0f ,0.0f, 0.0f), new Vector3(0.0f ,0.0f, 0.0f)), // Департамент Шерифа
            new(new Vector3(0.0f ,0.0f, 0.0f), new Vector3(0.0f ,0.0f, 0.0f)), // Федеральная тюрьма
            new(new Vector3(0.0f ,0.0f, 0.0f), new Vector3(0.0f ,0.0f, 0.0f)), // Национальная гвардия
            new(new Vector3(0.0f ,0.0f, 0.0f), new Vector3(0.0f ,0.0f, 0.0f)), // Больница
            new(new Vector3(0.0f ,0.0f, 0.0f), new Vector3(0.0f ,0.0f, 0.0f)), // СМИ
        };
    }

    public class FactionsPosition(Vector3 position, string factionName)
    {
        public Vector3 Position { get; set; } = position;
        public string FactionName { get; set; } = factionName;
    }

    /// <summary>
    /// Информация о фракционных блипах.
    /// </summary>
    /// <param name="sprite">Спрайт блипа фракции.</param>
    /// <param name="color">Цвет спрайта блипа фракции.</param>
    public class BlipSpritePlusColor(uint sprite, byte color)
    {
        public uint Sprite { get; set; } = sprite;
        public byte Color { get; set; } = color;
    }

    /// <summary>
    /// Склады фракций.
    /// </summary>
    /// <param name="unLoadingStockPosition">Расположение колшейпа для выгрузки материалов.</param>
    /// <param name="stockPosition">Расположение склада фракций.</param>
    public class FactionStock(Vector3 unLoadingStockPosition, Vector3 stockPosition)
    {
        public Vector3 UnLoadingStockPosition { get; set; } = unLoadingStockPosition;
        public Vector3 StockPosition { get; set; } = stockPosition;
    }
}