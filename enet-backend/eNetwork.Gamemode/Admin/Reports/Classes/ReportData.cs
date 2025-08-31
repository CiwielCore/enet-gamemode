using eNetwork.Framework;
using eNetwork.Game.Characters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Admin.Reports.Classes
{
    public class ReportData
    {
        public int Id { get; set; }
        public int Author { get; set; }
        public int Admin { get; set; }
        public List<ReportChatMessage> ChatMessages { get; set; } = new List<ReportChatMessage>();
        public bool IsClosed { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public ENetPlayer GetAuthor()
        {
            return ENet.Pools.GetPlayerByUUID(Author);
        }

        public ENetPlayer GetAdmin()
        {
            return ENet.Pools.GetPlayerByUUID(Admin);
        }

        public void ConnectAdmin(ENetPlayer player)
        {
            Admin = player.GetUUID();
            SendLog($"Администратор {player.GetName()} #{player.GetUUID()} присоединился к обращению!");

            SelectReport(player);
        }

        public void SelectReport(ENetPlayer player)
        {
            ClientEvent.Event(player, "client.report.select", Id);
        }

        public void SendLog(string text)
        {
            ChatMessages.Add(new ReportChatMessage() { Author = -1, Text = text });
            Update();
        }

        public void AddMessage(ENetPlayer player, string message)
        {
            if (player == GetAdmin())
            {
                var target = GetAuthor();
                if (target != null)
                {
                    ENet.Chat.SendMessage(target, "<span style='color: orange'>Администратор ответил на ваше обращение!</span>");
                }

                AdminsManager.SendChat($"Обращение #{Id} | {player.GetName()} #{player.GetUUID()} => {message}");
            }

            ChatMessages.Add(new ReportChatMessage() { Author = player.GetUUID(), AuthorName = player.GetName(), Text = message });
            Update();
        }

        private void Update(ENetPlayer player)
            => ClientEvent.Event(player, "client.reports.update", Id, JsonConvert.SerializeObject(this));

        public void Update()
        {
            if (Author != -1 && !IsClosed)
            {
                var author = GetAuthor();
                if (author != null)
                    Update(author);
            }

            AdminsManager.GetAdmins().ForEach(admin => Update(admin));
        }

        public void Close()
        {
            if (IsClosed) return;

            IsClosed = true;
            Update();

            if (Admin > 0)
                CharacterManager.GetCharacterData(Admin).AdminData.ReportsClosed++;

            ENet.Database.Execute($"INSERT INTO `reports_history` (`id`, `author`, `admin`, `chatMessages`, `date`) VALUES({Id}, {Author}, {Admin}, '{JsonConvert.SerializeObject(ChatMessages)}', '{Date.ToString("s")}')");
        }
    }
}
