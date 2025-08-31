using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using eNetwork.Framework;

namespace eNetwork.External
{
    public class Telegram
    {
        private static readonly Logger _logger = new Logger("Telegram");

        private static string chatID = "";
        private static string botID = "";
        public static void Init(string chatId, string botId)
        {
            chatID = chatId; botID = botId;
            _logger.WriteInfo("Модуль Telgram загружен");
        }
        public static void SendMessage(string text)
        {
            try
            {
                Uri uri = new Uri($"https://api.telegram.org/bot{botID}/sendMessage?chat_id={chatID}&text={text}");
                WebRequest request = WebRequest.Create(uri);
                request.GetResponse();
            }
            catch (Exception ex) { _logger.WriteError("SendMessage", ex); }
        }
    }
}
