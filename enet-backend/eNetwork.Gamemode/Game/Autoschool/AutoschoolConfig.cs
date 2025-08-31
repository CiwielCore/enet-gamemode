using eNetwork.Framework.Classes;
using eNetwork.Game.Autoschool.Exam;
using GTANetworkAPI;
using System;
using System.Collections.Generic;

namespace eNetwork.Game.Autoschool.Config
{
    interface IJsonConfig
    {
        public static string FilePath { get; }
    }

    class AutoschoolConfig : IJsonConfig
    {
        public static string FilePath => "autoschool/config.json";

        public Vector3 Location { get; set; }

        public Vector3 PedPosition { get; set; }
        public float PedHeading { get; set; }
        public uint PedModelHash { get; set; }

        public float ColShapeRange { get; set; }
        public float ColShapeHeight { get; set; }

        public bool BlipsEnable { get; set; }
        public string BlipName { get; set; }
        public uint BlipSprite { get; set; }
        public float BlipScale { get; set; }
        public byte BlipColor { get; set; }

        public bool MarkerEnable { get; set; }
        public uint MarkerType { get; set; }
        public float MarkerScale { get; set; }
        public Color MarkerColor { get; set; }
    }

    class AutoschoolDialogConfig : IJsonConfig
    {
        public static string FilePath => "autoschool/dialog_config.json";

        public string NameText { get; set; }
        public string DescriptionText { get; set; }
        public string ContainerText { get; set; }
        public List<AutoschoolDialogButton> Buttons { get; set; }
    }

    class AutoschoolExamConfig : IJsonConfig
    {
        public static string FilePath => "autoschool/exam_config.json";

        public uint DimensionStartOffset { get; set; }
        public int TimeToCompleteRouteInMinutes { get; set; }
        public Dictionary<DrivingLicenseClass, int> ExamPrices { get; set; }
        public Dictionary<DrivingLicenseClass, string> VehicleModels { get; set; }
        public Dictionary<DrivingLicenseClass, Position> VehicleSpawns { get; set; }
    }

    class AutoschoolDialogButton
    {
        public string Text { get; set; }
        public string Callback { get; set; }

        public string GetFormattedText()
        {
            if (Enum.TryParse(typeof(DrivingLicenseClass), Callback, true, out object licenseClass))
                return $"{Text} – {AutoschoolExam.Instance.GetExamPrice((DrivingLicenseClass)licenseClass)}";

            return Text;
        }
    }
}
