using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GTANetworkAPI;

namespace eNetwork.Framework.API.Clientside
{
    public class ClientLoader
    {
        private static readonly Logger _logger = new Logger("client-loader");
        private static string ClientSide = "";
        private static IEnumerable<IEnumerable<string>> cacheChunkedCode;

        public void Initialize()
        {
            try
            {
                ClientSide = File.ReadAllText("client_side/elision-clientside.js", Encoding.UTF8);
                if (String.IsNullOrEmpty(ClientSide))
                {
                    _logger.WriteError("Не удалось загрузить клиентскую часть");
                    return;
                }

                List<string> cacheSplittedCode = StringSplitFromArray(ClientSide, 65535);
                cacheChunkedCode = Chunk(cacheSplittedCode, 3);

                _logger.WriteInfo("Клиентская часть успешно подгружена");
            }
            catch (Exception ex) { _logger.WriteError("Load", ex); }
        }

        private List<string> StringSplitFromArray(string str, int length)
        {
            List<string> arrayStrings = new List<string>();
            for (int i = 0; i < str.Length; i += length)
            {
                string subStringText = str.Substring(i, Math.Min(length, str.Length - i));
                arrayStrings.Add(subStringText);
            }

            return arrayStrings;
        }

        public static IEnumerable<IEnumerable<T>> Chunk<T>(IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }

        public void Load(ENetPlayer player)
        {
            try
            {
                if (String.IsNullOrEmpty(ClientSide))
                {
                    _logger.WriteError("Не удалось загрузить клиентскую часть");
                    return;
                }

                //player.Eval(ClientSide);
            }
            catch(Exception ex) { _logger.WriteError("Load", ex); }
        }
    }
}
