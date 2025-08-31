using eNetwork.Admin.Reports.Classes;
using eNetwork.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eNetwork.Admin.Reports
{
    public class ReportsManager
    {
        private static readonly Logger Logger = new Logger("reports-manager");
        public static ConcurrentDictionary<int, ReportData> Reports = new ConcurrentDictionary<int, ReportData>();

        public static ReportData GetReportById(int id)
        {
            Reports.TryGetValue(id, out ReportData reportData);
            return reportData;
        }

        public static ReportData GetReportDataByAuthor(int authorId)
        {
            return Reports.Values.Where(x => x.IsClosed == false).ToList().Find(x => x.Author == authorId);
        }

        public static ReportData GetReportDataByAdmin(int adminId)
        {
            return Reports.Values.Where(x => x.IsClosed == false).ToList().Find(x => x.Admin == adminId);
        }

        public static void CreateReports(ENetPlayer player, string message)
        {
            try
            {
                var report = new ReportData()
                {
                    Id = GenerateId(),
                    Admin = -1,
                    Author = player.GetUUID(),
                    Date = DateTime.Now,
                    ChatMessages = new List<ReportChatMessage>(),
                    IsClosed = false
                };

                Reports.TryAdd(report.Id, report);

                report.SendLog($"Обращение #{report.Id} было создано!");
                report.AddMessage(player, message);
                report.Update();

                report.SelectReport(player);

                AdminsManager.SendNotify($"Пришел новый репорт!");
            }
            catch(Exception ex) { Logger.WriteError("CreateReports", ex); }
        }

        public static void LoadPlayer(ENetPlayer player)
        {
            try
            {
                if (!player.GetCharacter(out var characterData)) return;
                Dictionary<int, ReportData> reports = new Dictionary<int, ReportData>();

                if (characterData.Status > PlayerRank.Media)
                    reports = Reports.Where(x => !x.Value.IsClosed).ToDictionary(x => x.Key, x => x.Value);
                else
                    reports = Reports.Where(x => x.Value.Author == player.GetUUID()).ToDictionary(x => x.Key, x => x.Value);

                ClientEvent.Event(player, "client.reports.init", JsonConvert.SerializeObject(reports));
            }
            catch (Exception ex) { Logger.WriteError("Init", ex); }
        }

        public static int GenerateId()
        {
            int index = 1;
            if (Reports.Any())
                index = Reports.Last().Key + 1;

            return index;
        }
    }
}
