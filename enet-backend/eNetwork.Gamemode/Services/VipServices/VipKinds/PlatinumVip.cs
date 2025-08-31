using eNetwork.Services.VipServices.VipAddons;

namespace eNetwork.Services.VipServices
{
    public class PlatinumVip : Vip,
    IHandlerPlayerDeath,
    IHandlerOfUnemploymentBenefits,
    IHandlerSalarySupplement,
    IHandlerMultiplierSalary,
    IHandlerIncreaseNumberOfTaxPaymentDays,
    IHandlerMultiplierGameExperience
    {
        public override int ValidDays => 30;

        public override string Name => "Platinum";

        public override int Price => 1000;

        public int AmountOfHealthDuringSpawn => 90;

        public int AmountOfUnemploymentBenefits => 1200;

        public int AmountOfSalarySupplement => 2000;

        public float SalaryMultiplier => 1.60f;

        public int AmountOfIncreaseDays => 4 * 7; // 4 недели

        public float ExperienceMultiplier => 2.0f;
    }
}