using eNetwork.Framework;
using eNetwork.Framework.API.Sounds.Enums;
using eNetwork.Framework.Classes;
using eNetwork.GameUI;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static eNetwork.Configs.AnimationsConfig;

namespace eNetwork.Game.Player
{
    public class Animation : Script
    {
        private static readonly Logger Logger = new Logger("animations");

        public static void Initialize()
        {
            try {
                int countAnimatons = ANIMATIONS.Count;
                Logger.WriteInfo("Загружено " + countAnimatons + " aнимаций");

                ConfigReader.Create("animations", JsonConvert.SerializeObject(ANIMATIONS));
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        public static AnimationConfigItem GetAnimationData(int id)
            => ANIMATIONS.Find(x => x.id == id);

        [CustomEvent("server.custom_animatons.play")]
        public static void PlayCustomAnimation(ENetPlayer player, int id)
        {
            try
            {
                if (!player.GetSessionData(out var sessionData) || !player.IsTimeouted("custom_animation", 2)) return;

                var animationData = GetAnimationData(id);
                if (animationData is null) return;

                if (sessionData.AnimationData.CurrentAnimation != null)
                {
                    StopCustomAnimation(player);
                }

                player.PlayAnimation(animationData.animData[0], animationData.animData[1], animationData.flag);

                string url = $"https://cdn.majestic-files.com/sounds/animations/{animationData.music}.ogg";
                sessionData.AnimationData.SoundId = ENet.Sounds.Create3d(player, SoundType.Animation, url, 1, true);
                
                ENet.Sounds.Play3d(sessionData.AnimationData.SoundId);

                sessionData.AnimationData.CurrentAnimation = animationData;

                if (animationData.looped)
                    player.Freeze(true);
            }
            catch(Exception ex) { Logger.WriteError("PlayCustomAnimation", ex); }
        }

        [CustomEvent("server.custom_animatons.stop")]
        public static void StopCustomAnimation(ENetPlayer player)
        {
            try
            {
                if (!player.GetSessionData(out var sessionData) || !player.IsTimeouted("custom_animation", 2)) return;

                var animationData = sessionData.AnimationData.CurrentAnimation;
                if (animationData is null) return;

                if (sessionData.AnimationData.SoundId != string.Empty)
                {
                    ENet.Sounds.Destroy3d(sessionData.AnimationData.SoundId);
                    sessionData.AnimationData.SoundId = string.Empty;
                }

                sessionData.AnimationData.CurrentAnimation = null;

                player.StopAnimation();
                player.Freeze(false);
            }
            catch (Exception ex) { Logger.WriteError("StopCustomAnimation", ex); }
        }

        [CustomEvent("server.animation.stop")]
        public void StopAnimation(ENetPlayer player, string dict, string name, int flag)
        {
            try
            {
                if (player.GetCharacter() is null || !player.GetSharedData("player.animation", out string json)) return;
                AnimationData data = JsonConvert.DeserializeObject<AnimationData>(json);

                if (data.Dict != dict || data.Name != name || data.Flag != flag) return;
                switch (dict) 
                {
                    case "anim@mp_player_intmenu@key_fob@":
                        if (name == "fob_click")
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 33, "rcmcollect_paperleadinout@", "kneeling_arrest_get_up");
                            player.AddAttachment("carkey", remove: true);
                        }
                        break;
                    case "pickup_object":
                        if (name == "pickup_low")
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 33, "rcmcollect_paperleadinout@", "kneeling_arrest_get_up");
                        }
                        break;
                    case "anim@heists@narcotics@trash":
                        if (name == "drop_side")
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 33, "rcmcollect_paperleadinout@", "kneeling_arrest_get_up");
                        }
                        break;
                    case "anim@amb@office@boardroom@crew@female@var_b@base@":
                        if (name == "idle_a")
                        {
                            if (player.HasData("item.health.data"))
                            {
                                int _healthCount = player.GetData<int>("item.health.data");
                                int _playerHealth = 0;
                                if (player.Health + _healthCount > 100)
                                    _playerHealth = player.Health = 100;
                                else
                                    _playerHealth = player.Health + _healthCount;

                                player.Health = _playerHealth;

                                player.SendDone($"Здоровье пополнено до {_playerHealth} процентов!");
                                
                                player.ResetData("item.health.data");
                                player.AddAttachment("health_pack", remove: true);
                                player.RemoveItemHand();
                            }

                            NAPI.Player.PlayPlayerAnimation(player, 33, "rcmcollect_paperleadinout", "kneeling_arrest_get_up");
                        }
                        break;
                }

                player.ResetSharedData("player.animation");
            }
            catch(Exception e) { Logger.WriteError($"StopAnimation: {e.ToString()}"); }
        }
    }
}
