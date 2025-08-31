using eNetwork.Framework;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Characters.Customization
{
    public static class CustomizationManager
    {
        private static readonly Logger Logger = new Logger("customize-mngr");
        public static void SendToCreator(this ENetPlayer player, bool isFirst, int _slot = 0)
        {
            try
            {
                string _name = "";
                if (player.GetCharacter(out var characterData) && player.GetAccountData(out var accountData) &&
                    accountData.Characters.Contains(characterData.UUID))
                {
                    characterData.CustomizationData.Created = false;
                    isFirst = false;
                    _slot = accountData.Characters.IndexOf(characterData.UUID);
                    _name = characterData.Name;
                }

                player.SetData("creator.isFirst", isFirst);
                player.SetData("creator.slot", _slot);

                if (!player.IsAccountData() || (!isFirst && player.GetCharacter() is null) || (isFirst && player.GetCharacter() != null))
                {
                    player.ExtKick($"Игрок {player.GetName()} Зашел в создание персонажа и был кикнут за абуз системы!");
                    return;
                }

                ClientEvent.Event(player, "client.creator.send", isFirst, _name);
            }
            catch (Exception e) { Logger.WriteError("SendToCreator", e); }
        }

        public static CharacterData SaveCreator(ENetPlayer player, int uuid, bool gender, int age, int mother, int father, float skinSimilarity, int hairId, int hairColor, AppearanceData[] appearanceDatas, float[] features, int eyeColor, int tops, int legs, int shoes)
        {
            try
            {
                if (!player.IsAccountData() || player.GetCharacter() is null)
                {
                    player.ExtKick($"Игрок {player.GetName()} попытался изменить внешность и получил кик за абуз системы!!!");
                    return null;
                }
                if (age < 18 || age > 90)
                {
                    player.SendError(Language.GetText(TextType.CharacterAgeError, 18, 90));
                    return null;
                }

                CharacterData data = CharacterManager.GetCharacterData(uuid);
                data.CustomizationData.Gender = gender ? Gender.Male : Gender.Female;
                data.CustomizationData.parentData = new ParentData(father, mother, skinSimilarity, skinSimilarity);
                data.CustomizationData.Features = features;
                data.CustomizationData.Appearance = appearanceDatas;
                data.CustomizationData.otherCustomize = new OtherCustomizeData(hairColor, hairColor, eyeColor, 0, 0, hairColor);

                data.CustomizationData.Hair = new HairData(hairId, hairColor, 0);
                data.CustomizationData.Created = true;

                data.CustomizationData.Clothes = new ClothesData();

                #region Clothes
                switch (tops) //Верхняя одежда
                {
                    case 0:
                        if (gender)
                            data.CustomizationData.Clothes.Top = new ComponentData(0, 3);
                        else
                            data.CustomizationData.Clothes.Top = new ComponentData(0, 0);
                        break;
                    case 1:
                        if (gender)
                            data.CustomizationData.Clothes.Top = new ComponentData(0, 1);
                        else
                            data.CustomizationData.Clothes.Top = new ComponentData(0, 0);
                        break;
                    case 2:
                        if (gender)
                            data.CustomizationData.Clothes.Top = new ComponentData(5, 1);
                        else
                            data.CustomizationData.Clothes.Top = new ComponentData(0, 0);
                        break;
                    case 3:
                        if (gender)
                            data.CustomizationData.Clothes.Top = new ComponentData(7, 3);
                        else
                            data.CustomizationData.Clothes.Top = new ComponentData(0, 0);
                        break;
                    case 4:
                        if (gender)
                            data.CustomizationData.Clothes.Top = new ComponentData(8, 14);
                        else
                            data.CustomizationData.Clothes.Top = new ComponentData(0, 0);
                        break;
                    case 5:
                        if (gender)
                            data.CustomizationData.Clothes.Top = new ComponentData(3, 14);
                        else
                            data.CustomizationData.Clothes.Top = new ComponentData(0, 0);
                        break;
                    case 6:
                        if (gender)
                            data.CustomizationData.Clothes.Top = new ComponentData(2, 9);
                        else
                            data.CustomizationData.Clothes.Top = new ComponentData(0, 0);
                        break;
                    case 7:
                        if (gender)
                            data.CustomizationData.Clothes.Top = new ComponentData(6, 0);
                        else
                            data.CustomizationData.Clothes.Top = new ComponentData(0, 0);
                        break;
                    default: break;
                }

                switch (legs) // Нижняя одежда
                {
                    case 0:
                        if (gender)
                            data.CustomizationData.Clothes.Leg = new ComponentData(0, 1);
                        else
                            data.CustomizationData.Clothes.Leg = new ComponentData(0, 0);
                        break;
                    case 1:
                        if (gender)
                            data.CustomizationData.Clothes.Leg = new ComponentData(0, 2);
                        else
                            data.CustomizationData.Clothes.Leg = new ComponentData(0, 0);
                        break;
                    case 2:
                        if (gender)
                            data.CustomizationData.Clothes.Leg = new ComponentData(1, 5);
                        else
                            data.CustomizationData.Clothes.Leg = new ComponentData(0, 0);
                        break;
                    case 3:
                        if (gender)
                            data.CustomizationData.Clothes.Leg = new ComponentData(2, 11);
                        else
                            data.CustomizationData.Clothes.Leg = new ComponentData(0, 0);
                        break;
                    case 4:
                        if (gender)
                            data.CustomizationData.Clothes.Leg = new ComponentData(3, 0);
                        else
                            data.CustomizationData.Clothes.Leg = new ComponentData(0, 0);
                        break;
                    case 5:
                        if (gender)
                            data.CustomizationData.Clothes.Leg = new ComponentData(3, 2);
                        else
                            data.CustomizationData.Clothes.Leg = new ComponentData(0, 0);
                        break;
                    case 6:
                        if (gender)
                            data.CustomizationData.Clothes.Leg = new ComponentData(5, 2);
                        else
                            data.CustomizationData.Clothes.Leg = new ComponentData(0, 0);
                        break;
                    case 7:
                        if (gender)
                            data.CustomizationData.Clothes.Leg = new ComponentData(6, 1);
                        else
                            data.CustomizationData.Clothes.Leg = new ComponentData(0, 0);
                        break;
                    default: break;
                }

                switch (shoes) // Обувь
                {
                    case 0:
                        if (gender)
                            data.CustomizationData.Clothes.Feet = new ComponentData(1, 0);
                        else
                            data.CustomizationData.Clothes.Feet = new ComponentData(0, 0);
                        break;
                    case 1:
                        if (gender)
                            data.CustomizationData.Clothes.Feet = new ComponentData(1, 2);
                        else
                            data.CustomizationData.Clothes.Feet = new ComponentData(0, 0);
                        break;
                    case 2:
                        if (gender)
                            data.CustomizationData.Clothes.Feet = new ComponentData(2, 6);
                        else
                            data.CustomizationData.Clothes.Feet = new ComponentData(0, 0);
                        break;
                    case 3:
                        if (gender)
                            data.CustomizationData.Clothes.Feet = new ComponentData(3, 1);
                        else
                            data.CustomizationData.Clothes.Feet = new ComponentData(0, 0);
                        break;
                    case 4:
                        if (gender)
                            data.CustomizationData.Clothes.Feet = new ComponentData(4, 0);
                        else
                            data.CustomizationData.Clothes.Feet = new ComponentData(0, 0);
                        break;
                    case 5:
                        if (gender)
                            data.CustomizationData.Clothes.Feet = new ComponentData(4, 1);
                        else
                            data.CustomizationData.Clothes.Feet = new ComponentData(0, 0);
                        break;
                    case 6:
                        if (gender)
                            data.CustomizationData.Clothes.Feet = new ComponentData(5, 0);
                        else
                            data.CustomizationData.Clothes.Feet = new ComponentData(0, 0);
                        break;
                    case 7:
                        if (gender)
                            data.CustomizationData.Clothes.Feet = new ComponentData(6, 0);
                        else
                            data.CustomizationData.Clothes.Feet = new ComponentData(0, 0);
                        break;
                    default: break;
                }
                #endregion

                return data;
            }
            catch (Exception e) { Logger.WriteError("SaveCharacter", e); return null; }
        }
    }
}
