using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using eNetwork.Framework;
using GTANetworkAPI;
using eNetwork.Framework.API.Sounds.Enums;

namespace eNetwork
{
    public class Sound3D
    {
        private static readonly Logger Logger = new Logger("sound");
        public string Id { get; set; }
        public string Url { get; set; }
        public bool Looped { get; set; }
        public float Volume { get; set; }
        public int Distance { get; set; }
        public int StartOffset { get; set; }
        public bool IsPausing { get; set; }
        public SoundType SoundType { get; set; }

        [JsonIgnore]
        public DateTime Start { get; set; } = DateTime.Now;
        [JsonIgnore]
        public Entity Entity { get; set; }
        [JsonIgnore]
        public EntityType EntityType { get; set; }

        public void Play()
        {
            try
            {
                if (Entity is null) return;
                Start = DateTime.Now;
                IsPausing = false;

                var data = GetData();
                ClientEvent.EventForAll("client.soundManager.create3d", Entity, JsonConvert.SerializeObject(data));
            }
            catch(Exception ex) { Logger.WriteError("Play", ex); }
        }

        public object GetData() => new { Id, Url, Looped, Volume, Distance, IsPausing, SoundType = SoundType.ToString(), Start = Helper.GetTimeSpan(Start).TotalMilliseconds };

        public void SetLooped(bool toggle)
        {
            try
            {
                if (Looped == toggle) return;

                Looped = toggle;
                ClientEvent.EventForAll("client.soundManager.setLooped", Id, toggle);
            }
            catch(Exception ex) { Logger.WriteError("SetLooped", ex); }
        }

        public void SetPausing(bool toggle)
        {
            try
            {
                if (IsPausing == toggle) return;

                IsPausing = toggle;
                ClientEvent.EventForAll("client.soundManager.pause3d", Id, toggle);
            }
            catch (Exception ex) { Logger.WriteError("SetPausing", ex); }
        }

        public void SetVolume(int volume)
        {
            try
            {
                if (Volume == volume) return;

                Volume = volume;
                ClientEvent.EventForAll("client.soundManager.volume3d.change", Id, volume);
            }
            catch (Exception ex) { Logger.WriteError("SetVolume", ex); }
        }

        public void Destory()
        {
            ClientEvent.EventForAll("client.soundManager.stop3d", Id);
        }
    }
}
