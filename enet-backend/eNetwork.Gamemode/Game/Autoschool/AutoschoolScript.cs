using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.Framework.Classes;
using eNetwork.Game.Autoschool.Exam;
using System;

namespace eNetwork.Game.Autoschool.Script
{
    class AutoschoolScript
    {
        [InteractionDeprecated(ColShapeType.AutoschoolExamInteraction, InteractionType.Key)]
        private void OnInteractionPressKeyWithCarRentalPoint(ENetPlayer player)
        {
            if (!player.GetData(nameof(AutoschoolManager), out AutoschoolManager autoschool))
                return;

            autoschool.OpenDialogWithPed(player);
        }

        [CustomEvent("server.autoschool.finishExam")]
        public static void HandleFinishExamEvent(ENetPlayer player, string licenseClassString)
        {
            if (player.HasData(nameof(DrivingLicenseClass)) is false)
                return;

            if (Enum.TryParse(typeof(DrivingLicenseClass), licenseClassString, true, out object licenseClass) is false)
                return;

            if (player.GetData<DrivingLicenseClass>(nameof(DrivingLicenseClass)) != (DrivingLicenseClass)licenseClass)
                return;

            AutoschoolExam.Instance.FinishExam(player, (DrivingLicenseClass)licenseClass);
        }
    }
}
