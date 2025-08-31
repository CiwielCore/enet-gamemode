using eNetwork.Services.VipServices.VipAddons;

namespace eNetwork.Services.VipServices
{
    public class SilverVip : Vip, IHandlerPlayerDeath, IHandlerOfUnemploymentBenefits, IHandlerSalarySupplement, IHandlerMultiplierSalary
    {
        public override int ValidDays => 30;

        public override string Name => "Silver";

        public override int Price => 60;

        public int AmountOfHealthDuringSpawn => 50;

        public int AmountOfUnemploymentBenefits => 500;

        public int AmountOfSalarySupplement => 1000;

        public float SalaryMultiplier => 1.30f;
    }
}
