using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using GTANetworkAPI;
using eNetwork.Framework;
using eNetwork.API.Functions;
using eNetwork.Framework.API.CustomEvents.Data;

namespace eNetwork.Framework.API.CustomEvents.Methods
{
    public class EventParser
    {
        private Logger _logger = new Logger("EventParser");

        public CustomEventAttributeData MethodData;
        public ParameterInfo[] Parameters;
        public void Execute(Player player, params object[] args)
        {
            try
            {
                object[] array = new object[0];
                switch (Parameters.Length)
                {
                    case 1:
                        array = new object[1] { player };
                        break;
                    case 2:
                        if (Parameters[1].ParameterType == typeof(object[]))
                            array = new object[2] { player, args };
                        break;
                }

                if (array.Length == 0)
                {
                    array = new object[Parameters.Length];
                    array[0] = player;
                    for (int i = 0; i < Parameters.Length - 1; i++)
                    {
                        try
                        {
                            Type parameterType = Parameters[i + 1].ParameterType;
                            if (parameterType == typeof(uint))
                                array[i + 1] = (uint)(int)args[i];
                            else
                                array[i + 1] = Convert.ChangeType(args[i], parameterType, CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex) { _logger.WriteError("ParseEvent", ex); }
                    }
                }

                if (MethodData.FastInvokeHandler is FastInvokeHandler nonStaticHandler)
                    nonStaticHandler.Invoke(MethodData.Instance, array);
                else if (MethodData.FastInvokeHandler is FastInvokeHandlerStatic staticHandler)
                    staticHandler.Invoke(array);
            }
            catch(Exception ex) { _logger.WriteError($"ExecuteEvent [{MethodData.Method.Name}]", ex); }
        }
    }
}
