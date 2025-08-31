using System;
using System.Collections.Generic;
using System.Linq;

namespace eNetwork.Framework.Utils
{
    public class RandomValues
    {
        private Dictionary<string, float> _values { get; set; }
        private static Random _random { get; set; }

        public RandomValues()
        {
            _values = new Dictionary<string, float>();
        }

        /// <summary>
        /// Добавить значение и вероятность
        /// </summary>
        /// <param name="value"></param>
        /// <param name="probability"></param>
        /// <returns></returns>
        public bool TryAddValue(string value, float probability)
        {
            if (_values.ContainsKey(value))
                return false;

            if (probability <= 0 || probability > 100f)
                return false;

            if (_values.TryAdd(value, probability))
                return true;

            return false;
        }

        /// <summary>
        /// Получить случайное значение из добавленных
        /// </summary>
        /// <returns></returns>
        public string GetRandomValue()
        {
            var normalizedValues = GetNormalizedValues();
            _random = new Random();
            float randomValue = _random.Next(0, 10000) / 100f;

            var keys = normalizedValues.Keys.ToList();
            var values = normalizedValues.Values.ToList();

            float currentValue = 0;
            for (int i = 0; i < values.Count; i++)
            {
                if (i == 0)
                {
                    if (randomValue <= values[i])
                        return keys[i];
                }
                else
                {
                    if (randomValue <= currentValue + values[i])
                        return keys[i];
                }

                currentValue += values[i];
            }

            return "";
        }

        private Dictionary<string, float> GetNormalizedValues()
        {
            float summ = _values.Values.ToArray().Sum();

            Dictionary<string, float> normalizedValues = new Dictionary<string, float>();
            foreach (var value in _values)
            {
                normalizedValues.Add(value.Key, value.Value / summ * 100);
            }

            return normalizedValues;
        }
    }
}