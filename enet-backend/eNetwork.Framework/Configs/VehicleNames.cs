using eNetwork.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Configs
{
    public class VehicleNames
    {
        private static readonly Logger _logger = new Logger("vehicle-names-config");
        private static ConcurrentDictionary<string, string> _vehicleNames = new ConcurrentDictionary<string, string>();

        public static void Initialize()
        {
            try
            {
                _vehicleNames = ConfigReader.ReadAsync("vehicleNames", _vehicleNames);
            }
            catch(Exception ex) { _logger.WriteError("Initialize", ex); }
        }

        public static void Load(ENetPlayer player)
        {
            try
            {
                ClientEvent.Event(player, "client.vehiclenames.load", JsonConvert.SerializeObject(_vehicleNames));
            }
            catch (Exception ex) { _logger.WriteError("Load: " + ex.ToString()); }
        }

        public static string Get(string model)
        {
            return _vehicleNames.ContainsKey(model) ? _vehicleNames[model] : model;
        }
    }
}
