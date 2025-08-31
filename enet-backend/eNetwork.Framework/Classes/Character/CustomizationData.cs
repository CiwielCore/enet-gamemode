using eNetwork.Framework.Classes.Character.Customization;
using eNetwork.Framework.Configs.Tattoo.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Framework
{
    public class CustomizationData
    {
        public ParentData parentData;
        public float[] Features = new float[20];
        public AppearanceData[] Appearance = new AppearanceData[10];
        public ClothesData Clothes = new ClothesData();
        public AccessoriesData Accessories = new AccessoriesData();

        public HairData Hair;
        public Gender Gender = Gender.Male;
        public OtherCustomizeData otherCustomize;

        public bool Created = false;

        public Dictionary<TattooZone, List<PlayerTattooData>> Tattoos { get; set; } = new Dictionary<TattooZone, List<PlayerTattooData>>()
        {
            { TattooZone.ZONE_TORSO, new List<PlayerTattooData>() },
            { TattooZone.ZONE_HEAD, new List<PlayerTattooData>() },
            { TattooZone.ZONE_LEFT_ARM, new List<PlayerTattooData>() },
            { TattooZone.ZONE_RIGHT_ARM, new List<PlayerTattooData>() },
            { TattooZone.ZONE_LEFT_LEG, new List<PlayerTattooData>() },
            { TattooZone.ZONE_RIGHT_LEG, new List<PlayerTattooData>() },
        };

        public CustomizationData()
        {
            parentData = new ParentData(0, 0, 1f, 1f);
            for (int i = 0; i < Features.Length; i++) Features[i] = 0f;
            for (int i = 0; i < Appearance.Length; i++) Appearance[i] = new AppearanceData(255, 1f);
            Hair = new HairData(0, 0, 0);
            otherCustomize = new OtherCustomizeData(0, 0, 0, 0, 0, 0);
        }
    }
    public class ParentData
    {
        public int Father;
        public int Mother;
        public float Similarity;
        public float SkinSimilarity;
        public ParentData(int father, int mother, float similarity, float skinsimilarity)
        {
            Father = father;
            Mother = mother;
            Similarity = similarity;
            SkinSimilarity = skinsimilarity;
        }
    }
    public class OtherCustomizeData
    {
        public int EyeBrowColor;
        public int BreadColor;
        public int EyeColor;
        public int BlushColor;
        public int LipstickColor;
        public int ChestHairColor;
        public OtherCustomizeData(int eyeBrowColor, int breadColor, int eyeColor, int blushColor, int lipstickColor, int chestHairColor)
        {
            EyeBrowColor = eyeBrowColor;
            BreadColor = breadColor;
            EyeColor = eyeColor;
            BlushColor = blushColor;
            LipstickColor = lipstickColor;
            ChestHairColor = chestHairColor;
        }
    }
    public class AppearanceData
    {
        public int Value;
        public float Opacity;

        public AppearanceData(int value, float opacity)
        {
            Value = value;
            Opacity = opacity;
        }
    }
    public class HairData
    {
        public int Hair;
        public int Color;
        public int HighlightColor;

        public HairData(int hair, int color, int highlightcolor)
        {
            Hair = hair;
            Color = color;
            HighlightColor = highlightcolor;
        }
    }
    public class ClothesData
    {
        public ComponentData Mask { get; set; }
        public ComponentData Gloves { get; set; }
        public ComponentData Torso { get; set; }
        public ComponentData Leg { get; set; }
        public ComponentData Bag { get; set; }
        public ComponentData Feet { get; set; }
        public ComponentData Accessory { get; set; }
        public ComponentData Undershit { get; set; }
        public ComponentData Bodyarmor { get; set; }
        public ComponentData Decals { get; set; }
        public ComponentData Top { get; set; }
        public Costumes Costume { get; set; }

        public ClothesData()
        {
            Mask = new ComponentData(0, 0);
            Gloves = new ComponentData(0, 0);
            Torso = new ComponentData(15, 0);
            Leg = new ComponentData(21, 0);
            Bag = new ComponentData(0, 0);
            Feet = new ComponentData(34, 0);
            Accessory = new ComponentData(0, 0);
            Undershit = new ComponentData(15, 0);
            Bodyarmor = new ComponentData(0, 0);
            Decals = new ComponentData(0, 0);
            Top = new ComponentData(15, 0);
            Costume = Costumes.Empty;
        }
    }
    public class AccessoriesData
    {
        public ComponentData Hat { get; set; }
        public ComponentData Glasses { get; set; }
        public ComponentData Ear { get; set; }
        public ComponentData Watches { get; set; }
        public ComponentData Bracelets { get; set; }

        public AccessoriesData()
        {
            Hat = new ComponentData(-1, 0);
            Glasses = new ComponentData(-1, 0);
            Ear = new ComponentData(-1, 0);
            Watches = new ComponentData(-1, 0);
            Bracelets = new ComponentData(-1, 0);
        }
    }
    public class ComponentData
    {
        public int DrawableId;
        public int TextureId;

        public ComponentData(int variation, int texture)
        {
            DrawableId = variation;
            TextureId = texture;
        }
    }
    public enum Costumes
    {
        Empty, Diving
    }
}
