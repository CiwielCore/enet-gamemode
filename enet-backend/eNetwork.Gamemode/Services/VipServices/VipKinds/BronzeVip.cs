using eNetwork.Services.VipServices.VipAddons;

namespace eNetwork.Services.VipServices
{
    public class BronzeVip : Vip, IHandlerPlayerDeath, IHandlerOfUnemploymentBenefits, IHandlerSalarySupplement, IHandlerMultiplierSalary
    {
        public override int ValidDays => 30;

        public override string Name => "Bronze";

        public override int Price => 300;

        public int AmountOfHealthDuringSpawn => 50;

        public int AmountOfUnemploymentBenefits => 250;

        public int AmountOfSalarySupplement => 500;

        public float SalaryMultiplier => 1.15f;
    }

}