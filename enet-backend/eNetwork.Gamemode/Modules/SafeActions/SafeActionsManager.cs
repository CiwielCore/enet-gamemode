using eNetwork.Framework;
using eNetwork.Modules.SafeActions.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Modules.SafeActions
{
    public class SafeActionsManager
    {
        public static EveryMinute EveryMinute = new EveryMinute();

        public static Saving SavingDatabase = new Saving();
    }
}
