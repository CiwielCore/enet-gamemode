using eNetwork.Framework;
using eNetwork.Framework.Classes;
using eNetwork.Framework.Singleton;
using eNetwork.Game.Autoschool.Config;
using eNetwork.Game.Autoschool.Exam;
using eNetwork.GameUI;
using GTANetworkAPI;
using System;

namespace eNetwork.Game.Autoschool
{
    class AutoschoolManager : Singleton<AutoschoolManager>
    {
        private readonly AutoschoolConfig _config;
        private readonly AutoschoolDialogConfig _dialogConfig;

        private Ped _ped;
        private Blip _blip;
        private Marker _marker;
        private ColShape _colShape;

        private Dialog _dialog;

        private AutoschoolManager()
        {
            _config = ConfigReader.Read<AutoschoolConfig>(AutoschoolConfig.FilePath);
            _dialogConfig = ConfigReader.Read<AutoschoolDialogConfig>(AutoschoolDialogConfig.FilePath);
        }

        public void OnResourceStart()
        {
            CreateInteractionDialog();
            CreateInteractionPoint();
        }

        public void OpenDialogWithPed(ENetPlayer player)
        {
            _dialog.Open(player, _ped);
        }

        private void CreateInteractionDialog()
        {
            _dialog = new Dialog()
            {
                ID = (int)DialogType.AutoschoolExamInteraction,
                Name = _dialogConfig.NameText,
                Description = _dialogConfig.DescriptionText,
                Text = _dialogConfig.ContainerText,
                Answers = _dialogConfig.Buttons.ConvertAll(b =>
                    new DialogAnswer(
                        b.GetFormattedText(),
                        (p, _) => HandleDialogButtonClick(p, b.Callback),
                        b.Callback
                        ))
            };
        }

        private void CreateInteractionPoint()
        {
            if (_config.BlipsEnable)
            {
                _blip = ENet.Blip.CreateBlip(
                    _config.BlipSprite,
                    _config.Location,
                    _config.BlipScale,
                    _config.BlipColor,
                    _config.BlipName,
                    255,
                    0,
                    true,
                    0,
                    0);
            }

            if (_config.MarkerEnable)
            {
                _marker = NAPI.Marker.CreateMarker(
                    _config.MarkerType,
                    _config.PedPosition,
                    new Vector3(),
                    new Vector3(),
                    _config.MarkerScale,
                    _config.MarkerColor,
                    false,
                    0);
            }

            uint pedModel = _config.PedModelHash;
            _ped = NAPI.Ped.CreatePed(pedModel, _config.PedPosition, _config.PedHeading, false, true, true, true, 0);
            _ped.SetData("POSITION_DATA", new Position(_config.PedPosition, _config.PedHeading));

            _colShape = ENet.ColShape.CreateCylinderColShape(
                _config.PedPosition,
                _config.ColShapeRange,
                _config.ColShapeHeight,
                0,
                ColShapeType.AutoschoolExamInteraction);

            _colShape.OnEntityEnterColShape += (_, player) => player.SetData(nameof(AutoschoolManager), this); ;
            _colShape.OnEntityExitColShape += (_, player) => player.ResetData(nameof(AutoschoolManager));
        }

        private void HandleDialogButtonClick(ENetPlayer player, string callback)
        {
            if (callback == "close")
            {
                Dialogs.Close(player);
                return;
            }

            if (Enum.IsDefined(typeof(DrivingLicenseClass), callback) is false)
                return;

            DrivingLicenseClass licenseClass = (DrivingLicenseClass)Enum.Parse(typeof(DrivingLicenseClass), callback, true);
            AutoschoolExam.Instance.StartExam(player, licenseClass);
        }
    }
}
