namespace eNetwork.Services.VipServices
{
    public abstract class Vip
    {
        public abstract int ValidDays { get; }
        public abstract string Name { get; }
        public abstract int Price { get; }
    }
}
