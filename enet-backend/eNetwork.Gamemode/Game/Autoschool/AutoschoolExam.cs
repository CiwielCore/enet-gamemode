using eNetwork.Framework;
using eNetwork.Framework.Classes;
using eNetwork.Framework.Singleton;
using eNetwork.Game.Autoschool.Config;
using eNetwork.GameUI;
using eNetwork.Services.DrivingLicensing;
using GTANetworkAPI;
using System;
using System.Collections.Generic;

namespace eNetwork.Game.Autoschool.Exam
{
    class AutoschoolExam : Singleton<AutoschoolExam>
    {
        private readonly AutoschoolExamConfig _examConfig;
        private readonly DrivingLicenseService _drivingLicenseService;

        private AutoschoolExam()
        {
            _examConfig = ConfigReader.Read<AutoschoolExamConfig>(AutoschoolExamConfig.FilePath);
            _drivingLicenseService = DrivingLicenseService.Instance;
        }

        public void StartExam(ENetPlayer player, DrivingLicenseClass licenseClass)
        {
            if (!player.GetCharacter(out var characterData)) return;

            DrivingLicense license = _drivingLicenseService.GetDrivingLicense(player);
            if (license.HasCategory(licenseClass))
            {
                player.SendWarning("У вас уже есть данная категория");
                return;
            }

            if (!player.ChangeWallet(-GetExamPrice(licenseClass)))
                return;

            characterData.ExteriosPosition = player.Position;
            NAPI.Data.SetEntityData(player, nameof(DrivingLicenseClass), licenseClass);
            NAPI.Entity.SetEntityDimension(player, _examConfig.DimensionStartOffset + player.Id);

            string numberplate = GenerateNumberPlateText();
            string model = _examConfig.VehicleModels[licenseClass];
            Position spawn = _examConfig.VehicleSpawns[licenseClass];

            ClientEvent.Event(player, "client.autoschool.createVehicle", model, numberplate, spawn.GetVector3(), spawn.Heading);
            ClientEvent.Event(player, "client.autoschool.startExam", licenseClass.ToString("G"), _examConfig.TimeToCompleteRouteInMinutes);
            Dialogs.Close(player);
        }

        public void FinishExam(ENetPlayer player, DrivingLicenseClass licenseClass)
        {
            if (!player.GetCharacter(out var characterData)) return;
            player.Position = characterData.ExteriosPosition;
            characterData.ExteriosPosition = null;

            NAPI.Data.ResetEntityData(player, nameof(DrivingLicenseClass));
            NAPI.Entity.SetEntityDimension(player, 0);

            _drivingLicenseService.GiveLicense(player, licenseClass);
        }

        public int GetExamPrice(DrivingLicenseClass licenseClass)
        {
            if (_examConfig.ExamPrices.ContainsKey(licenseClass))
                return _examConfig.ExamPrices[licenseClass];

            throw new KeyNotFoundException(licenseClass.ToString());
        }

        private string GenerateNumberPlateText()
        {
            return Guid.NewGuid().ToString().Replace("-","")[..8];
        }
    }
}
