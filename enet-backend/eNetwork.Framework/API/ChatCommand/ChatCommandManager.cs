using System;
using System.Linq;
using System.Reflection;
using GTANetworkAPI;
using eNetwork.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using eNetwork.API.Functions;
using eNetwork.Framework.API.ChatCommand.Data;
using eNetwork.Framework.API.ChatCommand.Methods;

namespace eNetwork.Framework.API.ChatCommand
{
    public class ChatCommandManager
    {
        private static readonly Logger _logger = new Logger("chat-commands");
        
        private List<ChatCommandData> ChatCommandsList = new List<ChatCommandData>();
        private Dictionary<string, ChatCommandParser> CommandsList = new Dictionary<string, ChatCommandParser>();

        public void Initialize()
        {
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
                var methods = assembly.SelectMany(type => type
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                   .Where(m => m.GetCustomAttribute<ChatCommandAttribute>(false) != null));

                foreach (var method in methods)
                {
                    foreach (var prop in method.GetCustomAttributes(false))
                    {
                        if (!(prop is ChatCommandAttribute attribute)) continue;

                        var fastInvokeHandler = new FastMethodInvoker().GetMethodInvoker(method);
                        var attributeData = new ChatCommandAttributeData(method, fastInvokeHandler);

                        if (AddInstanceIfRequired(method, attributeData))
                        {
                            CommandsList.Add(attribute.Command, new ChatCommandParser()
                            {
                                MethodData = attributeData,
                                GreedyArg = attribute.GreedyArg,
                                Parameters = method.GetParameters(),
                                Access = attribute.Access
                            });

                            ChatCommandData cmd = new ChatCommandData(attribute.Command, attribute.Description, attribute.Arguments, attribute.Access);
                            ChatCommandsList.Add(cmd);
                        }
                    }
                }

                _logger.WriteInfo($"Загруженно {CommandsList.Count} команд на сервер");
            }
            catch (Exception ex) { _logger.WriteError("LoadCommands", ex); }
        }
        private bool AddInstanceIfRequired(MethodInfo method, ChatCommandAttributeData methodData)
        {
            if (method.IsStatic) return true;

            var instance = GetMethodInstance(method);
            if (instance is null) return false;
            methodData.Instance = instance;
            return true;
        }
        private readonly Dictionary<Type, object> _instancesPerClass = new Dictionary<Type, object>();
        private object GetMethodInstance(MethodInfo method)
        {
            var classType = method.DeclaringType;
            if (_instancesPerClass.TryGetValue(classType, out var instance))
                return instance;

            instance = Activator.CreateInstance(classType);
            if (instance is null) return null;
            _instancesPerClass[classType] = instance;
            return instance;
        }
        public void ErrorMessageParams(ENetPlayer player, string command, ParameterInfo[] parameters)
        {
            try
            {
                int i = 0; string needParams = String.Empty;
                foreach (var it in parameters)
                {
                    if (i != 0) 
                        needParams += $"[{it.Name}] "; 
                    i++;
                }
                ENet.Chat.SendMessage(player, $"Используйте: /{command} {needParams}", new ChatAddition(ChatType.System));
            }
            catch (Exception ex) { _logger.WriteError("ErrorMessageParams", ex); }
        }
        public ChatError UseCommand(ENetPlayer player, PlayerRank playerRank, string commandName, string parameters)
        {
            try
            {
                if (!CommandsList.ContainsKey(commandName)) return ChatError.UndefinedCommand;
                var command = CommandsList[commandName];
                if (playerRank < command.Access) return ChatError.NotPerms;
                
                ChatError result = command.Parse(player, commandName, parameters);
                return result;
            }
            catch (Exception ex) { _logger.WriteError("UseCommand", ex); return ChatError.UndefinedCommand; }
        }
        public List<ChatCommandData> GetCommandsForUser(PlayerRank acces)
        {
            try
            {
                List<ChatCommandData> chatCommands = new List<ChatCommandData>();
                foreach (ChatCommandData chatCommand in ChatCommandsList)
                {
                    if (chatCommand.Access <= acces)
                    {
                        chatCommands.Add(chatCommand);
                    }
                }
                return chatCommands;
            }
            catch (Exception ex) { _logger.WriteError("GetCommandsForUser", ex); return null; }
        }
    }
    public enum ChatError { UndefinedCommand, NotPerms, Parameters, Done }
}
