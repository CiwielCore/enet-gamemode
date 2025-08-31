using eNetwork.Admin.Reports.Classes;
using eNetwork.Admin.Reports.Data;
using eNetwork.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Admin.Reports
{
    public class ReportsController
    {
        private static readonly Logger Logger = new Logger("reports-controller");

        [ChatCommand("report", Description = "Написать репорт", Arguments = "[сообщение]", GreedyArg = true)]
        public void Command_Report(ENetPlayer player, string message)
        {
            SendMessage(player, -1, message);
        }

        [CustomEvent("server.reports.send")]
        public static void SendMessage(ENetPlayer player, int id, string message)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;

                message = Helper.ParseHTML(message);
                if (message.Replace(" ", "").Length == 0) return;

                var currentReport = ReportsManager.GetReportDataByAuthor(player.GetUUID());
                if (id == -1) 
                {
                    if (currentReport is null)
                    {
                        if (characterData.Status > PlayerRank.Media)
                        {
                            player.SendError("Админы не могут создававать репорты!");
                            return;
                        }

                        // Player
                        ReportsManager.CreateReports(player, message);
                    }
                    else
                    {
                        player.SendError("Вы уже отправили репорт, закройте его прежде чем писать новый!");
                        return;
                    }
                }
                else
                {
                    if (currentReport is null) // Admin
                    {
                        currentReport = ReportsManager.GetReportDataByAdmin(player.GetUUID());
                        if (currentReport is null) return;
                    }

                    if (currentReport.Id != id) return;
                    
                    if (!player.IsTimeouted("reports.Message", Config.WAIT_TIME_BETWEEN_MESSAGES))
                    {
                        player.SendError($"Отправлять сообщения можно раз в {Config.WAIT_TIME_BETWEEN_MESSAGES} секунд!");
                        return;
                    }

                    currentReport.AddMessage(player, message);
                }
            }
            catch(Exception ex) { Logger.WriteError("CreateReport", ex); }
        }

        [CustomEvent("server.reports.select")]
        public static void SelectReport(ENetPlayer player, int id)
        {
            try
            {
                if (!player.GetCharacter(out var characterData) || characterData.Status <= PlayerRank.Media) return;

                var report = ReportsManager.GetReportById(id);
                if (report is null)
                {
                    player.SendError("Репорт не найден!");
                    return;
                }

                if (report.IsClosed)
                {
                    player.SendError("Репорт закрыт!");
                    return;
                }

                if (report.Admin != -1)
                {
                    player.SendError("Кто-то уже взял репорт!");
                    return;
                }

                report.ConnectAdmin(player);
            }
            catch(Exception ex) { Logger.WriteError("SelectReport", ex); }
        }

        [CustomEvent("server.reports.actionButton")]
        public static void ActionButton(ENetPlayer player, int id, string action)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;
                      
                var reportData = ReportsManager.GetReportById(id);
                if (reportData is null || reportData.IsClosed) return;

                bool isAdmin = characterData.Status > PlayerRank.Media;

                if (isAdmin && reportData.Admin != player.GetUUID()) return;
                if (!isAdmin && reportData.Author != player.GetUUID()) return;

                switch (action)
                {
                    case "teleport":
                        if (!isAdmin) return;

                        var target = reportData.GetAuthor();
                        if (target is null)
                        {
                            player.SendError("Игрок не в сети!");
                            return;
                        }

                        player.Position = target.Position;
                        player.Dimension = target.Dimension;
                        break;
                    case "close":
                        if (isAdmin)
                        {
                            reportData.SendLog($"Администратор {player.GetName()} #{player.GetUUID()} закрыл обращение!");
                        }
                        else
                        {
                            reportData.SendLog($"Игрок {player.GetName()} #{player.GetUUID()} закрыл обращение!");
                        }

                        reportData.Close();
                        player.SendInfo("Вы закрыли обращение");

                        if (isAdmin)
                        {
                            reportData.GetAuthor()?.SendInfo($"Администратор {reportData.Id} закрыл обращение");
                        }
                        else
                        {
                            reportData.GetAdmin()?.SendInfo($"Игрок {reportData.Id} закрыл обращение");
                        }
                        break;       
                }
            }
            catch(Exception ex) { Logger.WriteError("ActionButton", ex); }
        }                   

        [CustomEvent("server.reports.tryOpen")]
        public static void Open(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;

                int currentReport = -1;

                var selectedReport = characterData.Status > PlayerRank.Media ? ReportsManager.GetReportDataByAdmin(player.GetUUID()) : ReportsManager.GetReportDataByAuthor(player.GetUUID());
                if (selectedReport != null)
                    currentReport = selectedReport.Id;

                ClientEvent.Event(player, "client.reports.open", currentReport, player.GetUUID(), characterData.Status > PlayerRank.Media);
            }
            catch(Exception ex) { Logger.WriteError("Open", ex); }
        }
    }
}
