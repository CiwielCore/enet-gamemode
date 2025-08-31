using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace eNetwork.Framework
{
    public class ConfigReader
    {
        private static readonly Logger _logger = new Logger(nameof(ConfigReader));

        public static T ReadAsync<T>(string filePath, T config)
        {
            var path = $"data/{filePath}.json";

            if (!File.Exists(path))
            {
                using (var sw = File.CreateText(path))
                {
                    sw.Write(JsonConvert.SerializeObject(config, Formatting.Indented));
                    sw.Flush();
                    sw.Close();
                }
            }
            else
            {
                using (var r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    config = JsonConvert.DeserializeObject<T>(json);
                    r.Close();
                }
            }

            return config;
        }

        public static void Create(string filePath, string text)
        {
            try
            {
                string path = $"data/{filePath}.json";

                if (!File.Exists(path))
                {
                    File.CreateText(path);
                }
                else
                {
                    File.WriteAllText(path, string.Empty);
                }

                using (var streamWriter = new StreamWriter(path))
                {
                    streamWriter.Write(text);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
            catch(Exception ex) { _logger.WriteError("Create", ex); }
        }

        public static T Read<T>(string filePath)
        {
            string path = $"data/{filePath}";

            if (!path.EndsWith(".json"))
                path += ".json";

            if (!File.Exists(path))
            {
                using StreamWriter sw = File.CreateText(path);
                sw.Write(JsonConvert.SerializeObject(default(T), Formatting.Indented));
                sw.Flush();
                return default(T);
            }

            try
            {
                using StreamReader reader = new StreamReader(path);
                string text = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(text);
            }
            catch (JsonSerializationException)
            {
                _logger.WriteError($"Error deserialization config file along the path: {filePath}");
            }
            catch (System.Exception)
            {
                _logger.WriteError($"Error read config file: {filePath}");
            }

            return default(T);
        }

        public static void Save<T>(string filePath, T config)
        {
            var path = $"data/{filePath}.json";

            File.WriteAllText(path, string.Empty);
            using (var sw = new StreamWriter(path, true, Encoding.UTF8))
            {
                sw.Write(JsonConvert.SerializeObject(config, Formatting.Indented));
                sw.Close();
            }
        }
    }
}
