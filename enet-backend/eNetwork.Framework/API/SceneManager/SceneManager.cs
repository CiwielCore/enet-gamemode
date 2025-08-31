using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eNetwork.Framework;
using eNetwork.Framework.API.SceneManager.SceneAction;
using Newtonsoft.Json;

namespace eNetwork.Framework.API.SceneManager
{
    public class SceneManager
    {
        private static readonly Logger _logger = new Logger("scene-manager");
        private static Dictionary<string, object> Scenes = new Dictionary<string, object>();

        public void Initialize()
        {
            try
            {
                object data = ConfigReader.ReadAsync("scenes/intro_main", new Object());
                Scenes.Add("intro", data);

                data = ConfigReader.ReadAsync("scenes/casino_enter", new Object());
                Scenes.Add("casino_enter", data);

                _logger.WriteInfo($"Загруженно {Scenes.Count} кат. сцен");
            }
            catch(Exception ex) { _logger.WriteError("Initialize", ex); }
        }

        public void Load(ENetPlayer player)
        {
            try
            {
                foreach(var data in Scenes)
                {
                    ClientEvent.Event(player, "client.sceneManager.load", data.Key, JsonConvert.SerializeObject(data.Value));
                }

                ClientEvent.Event(player, "client.sceneManager.allLoades");
            }
            catch(Exception ex) { _logger.WriteError("Load", ex); }
        }

        public void Start(ENetPlayer player, string scenName)
        {
            try
            {
                ClientEvent.Event(player, "client.sceneManager.start", scenName);
            }
            catch(Exception ex) { _logger.WriteError("Start", ex); }
        }

        [CustomEvent("server.sceneManager.scena.ended")]
        private static void OnEnd(ENetPlayer player, string scenName)
        {
            try
            {
                if (!player.IsTimeouted("scena.ended", 1)) return;
 
                switch(scenName)
                {
                    case "casino_enter":
                        SceneActionManager.Call(SceneActionType.CasinoEnter, player, scenName);
                        return;
                    default: return;
                }
            }
            catch(Exception ex) { _logger.WriteError("OnEnd", ex); }
        }
    }
}
