using eNetwork.Businesses;
using eNetwork.Framework;
using eNetwork.Framework.API.Sounds.Enums;
using eNetwork.Framework.API.Sounds;
using eNetwork.GameUI;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkMethods;
using Newtonsoft.Json;

namespace eNetwork.Game
{
    class Radio
    {
        private static readonly Logger Logger = new Logger("vehicleradio");

        [CustomEvent("server.vehicleRadio.open")]
        private static void Event_RadioOpen(ENetPlayer player)
        {
            try
            {
                if (player.Vehicle is ENetVehicle eVehicle)
                {


                    if (eVehicle.Class == 8 || eVehicle.Class == 13 || eVehicle.Class == 14 || eVehicle.Class == 15 || eVehicle.Class == 16 || eVehicle.Class == 21)
                    {
                        player.SendError("У данного вида транспорта нет автозвука!");
                        return;
                    }

                    if (NAPI.Vehicle.GetVehicleDriver(player.Vehicle) != player)
                    {
                        player.SendError("Вы должны сидеть за водительским местом!");
                        return;
                    }

                    string soundId = eVehicle.HasData("sound.id") ? eVehicle.GetData<string>("sound.id") : "";
                    Framework.ClientEvent.Event(player, "client.vehicleRadio.open", soundId);
                }
            }
            catch (Exception ex) { Logger.WriteError("Open", ex); }
        }

        [CustomEvent("server.vehicleRadio.playOrPause")]
        private static void Event_RadioPlayOrPause(ENetPlayer player, int id, bool looped)
        {
            try
            {

                Station sound = VehicleRadioList.List.Find(x => x.Id == id);
                string url = sound.Url;


                if ( player.Vehicle is ENetVehicle eVehicle )
                {
                    if (!player.IsTimeouted("vehicleRadio.playOrPause", 1) || !player.IsInVehicle || NAPI.Vehicle.GetVehicleDriver(player.Vehicle) != player || String.IsNullOrEmpty(url)) return;

                    string _soundId = eVehicle.GetData<string>("sound.id");
                    if( _soundId == null )
                    {
                        string soundId = ENet.Sounds.Create3d(player.Vehicle, SoundType.VehicleAudio, url, 100, looped);
                        ENet.Sounds.Play3d(soundId);
                        eVehicle.SetData<string>("sound.id", soundId);
                        return;
                    }
                    var currentSound = ENet.Sounds.GetSound3d(_soundId);

                    if (currentSound is null || currentSound.Url != url)
                    {
                        if (!player.IsTimeouted("vehicleRadio.play", 5))
                        {
                            player.SendInfo("Немного подождите...");
                            return;
                        }

                        if (currentSound != null)
                            ENet.Sounds.Destroy3d(eVehicle.GetData<string>("sound.id"));


                        string soundId = ENet.Sounds.Create3d(player.Vehicle, SoundType.VehicleAudio, url, 100, looped);
                        ENet.Sounds.Play3d(soundId);
                        eVehicle.SetData<string>("sound.id", soundId);
                    }
                    else
                    {
                        string soundId = ENet.Sounds.Create3d(player.Vehicle, SoundType.VehicleAudio, url, 100, looped);
                        ENet.Sounds.Play3d(soundId);
                        eVehicle.SetData<string>("sound.id", soundId);
                    }
                }

                

            }
            catch (Exception ex) { Logger.WriteError("Open", ex); }
        }

        [RemoteEvent("server.vehicleRadio.setLooped")]
        public static void SetLooped(ENetPlayer player, string soundId, bool state)
        {
            try
            {
                if (!player.IsTimeouted("vehicleRadio.setLooped", 1) || player.Vehicle is null || NAPI.Vehicle.GetVehicleDriver(player.Vehicle) != player) return;

                ENet.Sounds.SetLooped3d(soundId, state);
            }
            catch (Exception ex) { Logger.WriteError("SetLooped: " + ex.ToString()); }
        }

        [RemoteEvent("server.vehicleRadio.volume.change")]
        public static void ChangeVolume(ENetPlayer player, string soundId, int volume)
        {
            try
            {
                if (!player.IsTimeouted("vehicleRadio.volume", 1) || player.Vehicle is null || NAPI.Vehicle.GetVehicleDriver(player.Vehicle) != player) return;

                ENet.Sounds.SetVolume3d(soundId, volume);
            }
            catch (Exception ex) { Logger.WriteError("ChangeVolume: " + ex.ToString()); }
        }

        [ServerEvent(Event.VehicleDeath)]
        public static void OnDeathVehicle(ENetVehicle vehicle)
        {
            try
            {
                string soundId = string.Empty;
                if (vehicle.HasData("sound.id"))
                    soundId = vehicle.GetData<string>("sound.id");

                if (string.IsNullOrEmpty(soundId)) return;
                ENet.Sounds.Destroy3d(soundId);
            }
            catch (Exception ex) { Logger.WriteError("OnDeathVehicle: " + ex.ToString()); }
        }
    }
    public class VehicleRadioList
    {
        public static List<Station> List = new List<Station>()
        {
            new Station(1, "gimme_gimme", "gimme_gimme", "./assets/gimme_gimme.mp3", "01:43"),

        };

        public static void Init(ENetPlayer player)
        {
            Console.WriteLine(JsonConvert.SerializeObject(List));
            Framework.ClientEvent.Event(player, "client.vehicleAudio.init", JsonConvert.SerializeObject(List));
        }
    }

    public class Station
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Duration { get; set; }

        public Station(int id, string name, string description, string url, string duration)
        {
            Id = id;
            Name = name;
            Description = description;
            Url = url;
            Duration = duration;
        }
    }
}
