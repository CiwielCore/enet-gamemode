using System;
using System.Text.RegularExpressions;
using eNetwork.Framework;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace eNetwork
{
    public static class Helper
    {
        public static Color GTAColor = new Color(67, 140, 239, 120);
        public static readonly float DrawDistance = 200;
        public static readonly string DollarColor = "#a2ff29";

        public static T Clone<T>(this T obj)
        {
            var clonedJson = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(clonedJson);
        }

        #region Formated functions
        public static string FormatName(string name)
        {
            return name.Replace("_", " ");
        }
        public static string FormatZero(int num)
        {
            return num < 10 ? "0" + num.ToString() : num.ToString();
        }
        public static string FormatPrice(int price)
        {
            return String.Format("{0:n0}", price);
        }
        public static string FormatPrice(long price)
        {
            return String.Format("{0:n0}", price);
        }
        public static string ParseHTML(string message)
        {
            return Regex.Replace(message, "<.*?>", string.Empty);
        }
        public static string ConvertTime(DateTime DateTime)
        {
            return DateTime.ToString("s");
        }
        public static string GenderString(string text, Gender gender)
        {
            return gender == Gender.Male ? text : $"{text}a";
        }
        #endregion

        #region Getters functions
        public static TimeSpan GetTimeSpan(DateTime dateTime)
        {
            dateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime);
            return dateTime.Subtract(DateTime.MinValue);
        }

        public static T GetRandomEnum<T>()
        {
            Array values = Enum.GetValues(typeof(T));
            T type = (T)values.GetValue(new Random().Next(values.Length));
            return type;
        }
        public static bool GetUpperInWord(string word, int maxUpps)
        {
            int upps = 0;
            for (int i = 0; i < word.Length; i++)
                if (word[i] == word.ToUpper()[i]) upps++;
            return upps > 0 && upps <= maxUpps;
        }
        #endregion

        #region Computing functions
        public static double GetAngle(float x1, float y1, float x2, float y2)
        {
            float xDiff = x2 - x1;
            float yDiff = y2 - y1;
            return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
        }

        public static Vector3 GetPositionOffset(Vector3 pos, double angle, double dist)
        {
            angle = angle * 0.0174533;
            double y = pos.Y + dist * Math.Sin(angle);
            double x = pos.X + dist * Math.Cos(angle);

            return new Vector3(x, y, pos.Z);
        }

        public static bool IsInRangeOfPoint(Vector3 firstPosition, Vector3 secondPosition, float range)
        {
            Vector3 direct = new Vector3(secondPosition.X - firstPosition.X, secondPosition.Y - firstPosition.Y, secondPosition.Z - firstPosition.Z);
            float len = direct.X * direct.X + direct.Y * direct.Y + direct.Z * direct.Z;
            return range * range > len;
        }
        #endregion
    }
}
