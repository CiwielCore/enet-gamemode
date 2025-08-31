using System;
using System.Collections.Generic;
using System.Text;
using eNetwork.Framework;
using eNetwork.API.ParticleFx;
using eNetwork.Framework.Classes;
using eNetwork.API.Task;
using eNetwork.Framework.API.Sounds;
using eNetwork.Framework.API.SceneManager;
using eNetwork.Framework.API.Clientside;
using eNetwork.Framework.API.SceneManager.SceneAction;
using eNetwork.Framework.API.Pools;
using eNetwork.Framework.API.Scenarios;
using eNetwork.Framework.API.Database;
using eNetwork.Framework.API.CustomEvents;
using eNetwork.Framework.API.ChatCommand;
using eNetwork.Framework.API.Blip;
using eNetwork.Framework.API.Interaction;
using eNetwork.Framework.API.Vehicles;
using eNetwork.Framework.API.ColShape;

namespace eNetwork
{
    public static class ENet
    {
        public static ColshapeManager ColShape = new ColshapeManager();
        
        public static InteractionManagerDeprecated Interaction = new InteractionManagerDeprecated();

        public static SceneActionManager SceneAction = new SceneActionManager();
        
        public static CustomEventManager CustomEvent = new CustomEventManager();
        
        public static ChatManager Chat = new ChatManager();

        public static ChatCommandManager ChatCommands = new ChatCommandManager();
        
        public static MySql Database = new MySql();
        
        public static MySql Config = new MySql();
        
        public static Offroad Offroad = new Offroad();
        
        public static ParticleFx ParticleFx = new ParticleFx();
        
        public static BlipManager Blip = new BlipManager();
        
        public static TaskManager Task = new TaskManager();

        public static SoundManager Sounds = new SoundManager();

        public static SceneManager SceneManager = new SceneManager();

        public static VehicleManager Vehicle = new VehicleManager();

        public static ClientLoader Clientside = new ClientLoader();

        public static PoolsManager Pools = new PoolsManager();

        public static ScenariosManager Scenarios = new ScenariosManager();

        public static Random Random = new Random();
    }
}
