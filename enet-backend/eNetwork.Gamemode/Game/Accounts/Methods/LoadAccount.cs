using eNetwork.Framework;
using eNetwork.Framework.Enums;
using eNetwork.Game.Characters;
using eNetwork.Game.Characters.Customization;
using eNetwork.Game.Core.BansCharacter;
using eNetwork.Game.Core.BansCharacter.Models;
using eNetwork.Houses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace eNetwork.Game.Accounts.Methods
{
    public static class LoadAccount
    {
        private static readonly Logger Logger = new Logger("load-account");

        public static void Load(this AccountData account, ENetPlayer player)
        {
            try
            {
                if (player.IsAccountData()) return;

                AccountBanData banData = BanHandler.Instance.GetBanAccount(account.Login);
                banData ??= BanHandler.Instance.GetBanAccount(player.SocialClubId);
                banData ??= BanHandler.Instance.GetBanAccountByHwid(player.Serial);
                banData ??= BanHandler.Instance.GetBanAccountByIp(player.Address);

                if (banData != null)
                {
                    ClientEvent.Event(player, "client.authorization.show_ban", JsonConvert.SerializeObject(new
                    {
                        Login = banData.AccountLogin,
                        Admin = banData.Admin,
                        Reason = banData.Reason,
                        Time = banData.Time.ToString("d"),
                        Ended = banData.Ended.ToString("d")
                    }));
                    return;
                }

                account.LastLogin = DateTime.Now;
                account.IP = player.IP;
                account.HWID = player.HardwareID;
                account.Player = player;
                account.IsLogined = true;

                player.SetAccountData(account);

                if (account.Characters[0] == -1)
                {
                    CustomizationManager.SendToCreator(player, true);
                    return;
                }

                int lastCharacter = 0;
                DateTime lastTime = DateTime.Now;
                foreach (var characterId in account.Characters)
                {
                    var character = CharacterManager.GetCharacterData(characterId);
                    if (character != null)
                    {
                        if (character.Stats.LastTimeUpdate < lastTime)
                        {
                            lastCharacter = account.Characters.IndexOf(characterId);
                            lastTime = character.Stats.LastTimeUpdate; //а что тут происходит? я так и не понял / fares
                        }
                    }
                }

                var charactersData = new object[] {
                    GetSlotInfo(0, account.Characters[0]),
                    GetSlotInfo(1, account.Characters[1]),
                    GetSlotInfo(2, account.Characters[2])
                };

                var customizationData = new CustomizationData[] {
                    new CustomizationData(),
                    new CustomizationData(),
                    new CustomizationData(),
                };

                foreach (var characterId in account.Characters)
                {
                    var characterData = CharacterManager.GetCharacterData(characterId);
                    if (characterData != null)
                        customizationData[account.Characters.IndexOf(characterId)] = characterData.CustomizationData;
                    else
                        customizationData[account.Characters.IndexOf(characterId)] = new CustomizationData()
                        {
                            Clothes = new ClothesData()
                            {
                                Top = new ComponentData(86, 3),
                                Leg = new ComponentData(26, 0),
                                Torso = new ComponentData(1, 0),
                                Feet = new ComponentData(2, 6),
                            },
                            Hair = new HairData(19, 0, 0),
                        };
                }

                ClientEvent.Event(player, 
                    "client.authorization.selector.show", 
                    account.Login, 
                    JsonConvert.SerializeObject(customizationData), 
                    JsonConvert.SerializeObject(charactersData), 
                    lastCharacter
                    );
            }
            catch (Exception ex) { Logger.WriteError("Load", ex); }
        }

        private static object GetSlotInfo(int slot, int uuid)
        {
            CharacterData characterData = CharacterManager.GetCharacterData(uuid);
            if (characterData is null) return new
            {
                Status = "free",
                Price = slot == 2 ? 1000 : 0,
            };

            var banData = BanHandler.Instance.GetBanCharacter(uuid);
            if (banData != null)
            {
                return new
                {
                    Status = "ban",
                    Name = characterData.Name,
                    UUID = characterData.UUID,
                    BlockBy = banData.Admin,
                    BlockDate = banData.Time.ToString("d"),
                    BlockReason = banData.Reason,
                    BlockUnblock = banData.Ended.ToString("d"),
                };
            }

            var spawns = new List<object>() { new { Name = "Место выхода", Type = "Last", Street = "", X = characterData.LastVector.X, Y = characterData.LastVector.Y, Z = characterData.LastVector.Z } };
            if (characterData.FactionId != -1)
            {
                //var factionData = Factions.Factionnager.GetFaction(characterData.Faction);
                //if (factionData != null)
                //    spawns.Add(new { Name = "База организации", Type = "Faction", Street = factionData.Name });
            }

            var house = HousesManager.GetPlayerHouse(uuid);
            if (house != null)
                spawns.Add(new { Name = "Дом", Type = "House", Street = $"#{house.Id}", X = house.Position.X, Y = house.Position.Y, Z = house.Position.Z });

            return new
            {
                Status = characterData.AdminData is null ? "Игрок" : "Администратор",
                Name = characterData.Name,
                UUID = characterData.UUID,
                Spawns = spawns,
                Lvl = characterData.Lvl,
                Exp = characterData.Exp
            };
        }
    }
}