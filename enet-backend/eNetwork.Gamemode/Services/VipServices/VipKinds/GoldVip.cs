using eNetwork.Services.VipServices.VipAddons;

namespace eNetwork.Services.VipServices
{
    public class GoldVip : Vip,
    IHandlerPlayerDeath,
    IHandlerOfUnemploymentBenefits,
    IHandlerSalarySupplement,
    IHandlerMultiplierSalary,
    IHandlerIncreaseNumberOfTaxPaymentDays
    {
        public override int ValidDays => 30;

        public override string Name => "Gold";

        public override int Price => 800;

        public int AmountOfHealthDuringSpawn => 75;

        public int AmountOfUnemploymentBenefits => 850;

        public int AmountOfSalarySupplement => 1500;

        public float SalaryMultiplier => 1.45f;

        public int AmountOfIncreaseDays => 3 * 7; // 3 недели
    }

}