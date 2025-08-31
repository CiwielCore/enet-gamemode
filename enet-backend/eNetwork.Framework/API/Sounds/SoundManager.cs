using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using eNetwork.Framework;
using System.Collections.Concurrent;
using eNetwork.Framework.API.Sounds.Enums;
using Newtonsoft.Json;

namespace eNetwork.Framework.API.Sounds
{
    public class SoundManager
    {
        private static readonly Logger Logger = new Logger("sound-manager");
        private static ConcurrentDictionary<string, Sound3D> Sound3dPool = new ConcurrentDictionary<string, Sound3D>();
        private int _soundLastId = 0;

        public void LoadSounds3dForPlayer(ENetPlayer player)
        {
            try
            {
                Sound3dPool.Values.ToList().ForEach(sound => LoadSound3dForPlayer(player, sound));
            }
            catch(Exception ex) { Logger.WriteError("LoadSoundsForPlayer", ex); }
        }

        public void LoadSound3dForPlayer(ENetPlayer player, Sound3D sound) => ClientEvent.Event(player, "client.soundManager", sound.Entity, JsonConvert.SerializeObject(sound.GetData()));

        public string Create3d(Entity entity, SoundType soundType, string url, float volume, bool isLooped)
        {
            try
            {
                string id = $"{entity.Type.ToString().ToLower()}_{_soundLastId}";

                Sound3D sound3d = new Sound3D();
                sound3d.Entity = entity;
                sound3d.EntityType = entity.Type;
                sound3d.Url = url;
                sound3d.Volume = volume;
                sound3d.Looped = isLooped;
                sound3d.IsPausing = true;
                sound3d.Id = id;
                sound3d.Volume = 100;
                sound3d.StartOffset = 0;
                sound3d.SoundType = soundType;
                sound3d.Distance = 10;

                _soundLastId++;
            
                Sound3dPool.TryAdd(id, sound3d);
                return id;
            }
            catch(Exception ex) { Logger.WriteError("Create", ex); return string.Empty; }
        }

        public void Play3d(string id)
        {
            try
            {
                var sound3d = GetSound3d(id);
                if (sound3d is null) return;

                sound3d.Play();
            }
            catch(Exception ex) { Logger.WriteError("Play3d", ex); }
        }

        public void SetLooped3d(string id, bool toggle)
        {
            try
            {
                var sound3d = GetSound3d(id);
                if (sound3d is null) return;

                sound3d.SetLooped(toggle);
            }
            catch (Exception ex) { Logger.WriteError("SetLooped3d", ex); }
        }

        public void SetPausing3d(string id, bool toggle)
        {
            try
            {
                var sound3d = GetSound3d(id);
                if (sound3d is null) return;

                sound3d.SetPausing(toggle);
            }
            catch (Exception ex) { Logger.WriteError("SetPausing", ex); }
        }    
        
        public void SetVolume3d(string id, int volume)
        {
            try
            {
                var sound3d = GetSound3d(id);
                if (sound3d is null) return;

                sound3d.SetVolume(volume);
            }
            catch (Exception ex) { Logger.WriteError("SetVolume", ex); }
        }

        public void Destroy3d(string id)
        {
            try
            {
                Sound3dPool.TryRemove(id, out var sound);
                if (sound != null) sound.Destory();
            }
            catch(Exception ex) { Logger.WriteError("Destroy", ex); }
        }

        public Sound3D GetSound3d(string id)
        {
            Sound3dPool.TryGetValue(id, out var sound3d);
            return sound3d;
        } 
    }
}
