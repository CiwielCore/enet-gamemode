using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using GTANetworkAPI;
using eNetwork.Framework;

namespace eNetwork.World
{
    public class WeatherHandler : Script
    {
        private static readonly Logger Logger = new Logger("weather");
        public static Weather LastWeather = Weather.CLEAR;
        private static DateTime _nexTimeChangeWeather = DateTime.Now;

        private static int timeOffset = -1;

        public static void Initialize()
        {
            Timers.StartTask("ChangeWeather", 1000, () => ChangeWeather());
            NAPI.World.SetWeather(Enum.GetName(typeof(Weather), getCurrentSeason() == SeasonTypes.Winter ? Weather.XMAS : Weather.CLEAR).ToUpper());
        }

        public static void LoadWeather(ENetPlayer player)
        {
            if (timeOffset == -1)
            {
                ClientEvent.Event(player, "client.world.start",
                    new int[3] { DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second },
                    new int[3] { DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year },
                    Enum.GetName(typeof(Weather), LastWeather).ToUpper()
                );
            }
            else
            {
                ClientEvent.Event(player, "client.world.start",
                    new int[3] { timeOffset, 0, 0 },
                    new int[3] { DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year },
                    Enum.GetName(typeof(Weather), LastWeather).ToUpper()
                );
            }
        }

        public static string SetCustomHour(int hour)
        {
            try
            {
                if (hour == -1) {
                    timeOffset = -1;
                    ClientEvent.EventForAll("client.world.set.timeoffset",
                        new int[3] { DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year },
                        new int[3] { DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second }
                    );
                    return "Вы вернули игровое время"; 
                }
                if (hour < 0 || hour > 23) 
                    return "Невозможно установить такое время";

                timeOffset = hour;
                ClientEvent.EventForAll("client.world.set.timeoffset", 
                    new int[3] { DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year }, 
                    new int[3] { timeOffset, 0, 0 }
                );
                return $"Вы сменили время на {timeOffset} часов";
            }
            catch(Exception e) { Logger.WriteError("SetCustomHour", e); return "Невозможно установить такое время"; }
        }

        public static void ChangeWeather()
        {
            try
            {
                Weather newWeather = LastWeather;
                if (_nexTimeChangeWeather <= DateTime.Now)
                {
                    int random = ENet.Random.Next(0, 101);
                    SeasonTypes season = getCurrentSeason();
                    if (season != SeasonTypes.Winter)
                    {
                        if (random < 75)
                        {
                            if (random < 60 && random > 40) newWeather = Weather.EXTRASUNNY;
                            else if (random >= 60) newWeather = Weather.CLOUDS;
                            else newWeather = Weather.CLEAR;
                            _nexTimeChangeWeather = DateTime.Now.AddMinutes(120);
                        }
                        else
                        {
                            if (random < 85) newWeather = Weather.RAIN;
                            else
                            {
                                int randomSmog = ENet.Random.Next(0, 101);
                                if (randomSmog < 40) newWeather = Weather.SMOG;
                                else if (randomSmog >= 40 && randomSmog < 70) newWeather = Weather.OVERCAST;
                                else newWeather = Weather.FOGGY;
                            }
                            _nexTimeChangeWeather = DateTime.Now.AddMinutes(75);
                        }
                    }
                    else
                    {
                        if (random < 75) newWeather = Weather.XMAS;
                        else { if (LastWeather != Weather.XMAS) newWeather = Weather.XMAS; else newWeather = Weather.SNOW; };
                        _nexTimeChangeWeather = DateTime.Now.AddMinutes(120);
                    }

                    if (LastWeather != newWeather)
                    {
                        LastWeather = newWeather;
                        Logger.WriteDone($"Новая погода на сервере: {Enum.GetName(typeof(Weather), newWeather).ToUpper()}");
                        ClientEvent.EventForAll("client.world.weather.change", Enum.GetName(typeof(Weather), newWeather).ToUpper());
                    }
                }
            }
            catch(Exception e) { Logger.WriteError("ChangeWeather", e); }
        }

        public static SeasonTypes getCurrentSeason()
        {
            if (DateTime.Now.Month == 1 || DateTime.Now.Month == 12) return SeasonTypes.Winter;
            if (DateTime.Now.Month == 2 || DateTime.Now.Month == 3 || DateTime.Now.Month == 4 || DateTime.Now.Month == 5) return SeasonTypes.Spring;
            if (DateTime.Now.Month == 6 || DateTime.Now.Month == 7 || DateTime.Now.Month == 8) return SeasonTypes.Summer;
            if (DateTime.Now.Month == 9 || DateTime.Now.Month == 10 || DateTime.Now.Month == 11) return SeasonTypes.Аutumn;
            return SeasonTypes.Summer;
        }
        public enum SeasonTypes { Winter, Spring, Summer, Аutumn, Empty }
    }
}
