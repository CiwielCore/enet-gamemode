namespace eNetwork.Gambles.Lotteries
{
    class LotteryTicketBank
    {
        private readonly uint _size;
        private uint _tickets;

        public LotteryTicketBank(uint size)
        {
            _size = size;
            _tickets = size;
        }

        public void Fill()
        {
            _tickets = _size;
        }

        public bool Fill(uint count)
        {
            uint tempCountTickets = _tickets + count;
            if (tempCountTickets > _size)
                return false;

            _tickets += count;
            return true;
        }

        public bool TakeTicket()
        {
            if (_tickets - 1 < 0)
                return false;
            
            _tickets--;
            return true;
        }
    }
}