using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework
{
    public class CharacterStats
    {
        public int TimePlaying { get; set; } = 0;
        public int AllTimePlaying { get; set; } = 0;
        public DateTime LastTimeUpdate { get; set; } = DateTime.Now;
    }
}
