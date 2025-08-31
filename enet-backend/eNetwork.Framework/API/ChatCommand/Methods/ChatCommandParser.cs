using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GTANetworkAPI;
using eNetwork.Framework;
using Newtonsoft.Json;
using eNetwork.API.Functions;
using eNetwork.Framework.API.ChatCommand.Data;
using System.Globalization;

namespace eNetwork.Framework.API.ChatCommand.Methods
{
    public class ChatCommandParser
    {
        private static readonly Logger _logger = new Logger("command-parser");

        public ChatCommandAttributeData MethodData;
        public ParameterInfo[] Parameters;
        public PlayerRank Access;
        public bool GreedyArg;
        public ChatError Parse(ENetPlayer player, string commandName, string parameters)
        {
            try
            {
                object[] args = new object[Parameters.Length];
                string[] split = parameters.Split(' ');
                if (string.IsNullOrEmpty(parameters))
                    split = new string[0];

                int countOffNoneDefalut = 0;
                foreach (var param in Parameters)
                    if (!param.HasDefaultValue) countOffNoneDefalut++;

                if (!GreedyArg && (countOffNoneDefalut - 1 > split.Length || args.Length - 1 <  split.Length))
                {
                    ENet.ChatCommands.ErrorMessageParams(player, commandName, Parameters);
                    return ChatError.Parameters;
                }

                args[0] = player;
                if (!string.IsNullOrEmpty(parameters))
                {
                    for (int i = 0; i < split.Length; i++)
                    {
                        if (Parameters.Length <= i + 1) continue;
                        dynamic param = Convert.ChangeType(split[i], Parameters[i + 1].ParameterType, CultureInfo.InvariantCulture);
                        if (param is string && GreedyArg)
                        {
                            param = "";
                            for (int a = i; a < split.Length; a++)
                                param += (a == i ? "" : " ") + split[a];
                        }
                        args[i + 1] = param;
                    }
                }

                int splitCounted = 0;
                foreach (var obj in args)
                {
                    if (obj is null)
                    {
                        if (!Parameters[splitCounted].HasDefaultValue)
                        {
                            ENet.ChatCommands.ErrorMessageParams(player, commandName, Parameters);
                            return ChatError.Parameters;
                        }
                        args[splitCounted] = Parameters[splitCounted].DefaultValue;
                    }
                    splitCounted++;
                }

                if (MethodData.FastInvokeHandler is FastInvokeHandler nonStaticHandler)
                    nonStaticHandler.Invoke(MethodData.Instance, args);
                else if (MethodData.FastInvokeHandler is FastInvokeHandlerStatic staticHandler)
                    staticHandler.Invoke(args);

                return ChatError.Done;
            }
            catch (Exception e) {  _logger.WriteError("Parse", e); return ChatError.NotPerms; }
        }
    }
}
