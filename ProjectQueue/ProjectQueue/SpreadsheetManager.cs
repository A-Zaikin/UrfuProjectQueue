using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using OfficeOpenXml;
using OfficeOpenXml.Drawing;

namespace ProjectQueue
{
    public static class SpreadsheetManager
    {
        public static List<DateTime> TeamCompletionTimes = new List<DateTime>();

        private static HashSet<string> completedTeams = new HashSet<string>();
        private static Timer checkSpreadsheetTimer;
        private static string subscribedAddress;
        private static Action subscribedCallback;
        private static Action updateTimeCallback;

        public static void SubscribeToCell(string address, Action callback, Action updateTime)
        {
            subscribedAddress = address;
            subscribedCallback = callback;
            checkSpreadsheetTimer = new Timer(CheckCellReadiness, null, 0, 3000);
            updateTimeCallback = updateTime;
        }

        public static void Unsubscribe()
        {
            checkSpreadsheetTimer.Dispose();
        }

        public static Dictionary<string, string> GetTeams(byte[] xlsxFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (MemoryStream ms = new MemoryStream(xlsxFile))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                var sheet = package.Workbook.Worksheets[0];
                return FindTeams(sheet).ToDictionary(cell => cell.Value.ToString(), cell => cell.Address.ToString());
            }
        }

        private static List<ExcelRangeBase> FindTeams(ExcelWorksheet sheet)
        {
            var teamCells = new List<ExcelRangeBase>();
            foreach (var cell in sheet.Cells)
            {
                if (cell.Value == null)
                    continue;
                if (IsRoomHeader(cell.Value.ToString()))
                {
                    var previousCell = cell;
                    while (true)
                    {
                        var currentCell = previousCell.Offset(1, 0);
                        if (currentCell.Value == null || currentCell.Value.ToString() == "")
                            break;
                        else
                        {
                            teamCells.Add(currentCell);
                            previousCell = currentCell;
                        }
                    }
                }
            }
            return teamCells;
        }

        private static void CheckCellReadiness(object state)
        {
            var xlsxFile = HttpManager.DownloadXlsxFile();
            using (MemoryStream ms = new MemoryStream(xlsxFile))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                var sheet = package.Workbook.Worksheets[0];
                var cell = sheet.Cells[subscribedAddress];
                var upperCell = cell.Offset(-1, 0);
                CheckCompletionTimes(upperCell);
                if (IsColored(upperCell))
                    subscribedCallback();
            }
        }

        private static void CheckCompletionTimes(ExcelRangeBase cell)
        {
            var currentCell = cell;
            while (!IsRoomHeader(currentCell.Value.ToString()))
            {
                if (!completedTeams.Contains(currentCell.Address) && IsColored(currentCell))
                {
                    completedTeams.Add(currentCell.Address);
                    TeamCompletionTimes.Add(DateTime.Now);
                }
                currentCell = currentCell.Offset(-1, 0);
            }
            updateTimeCallback();
        }

        private static bool IsRoomHeader(string value)
        {
            return new Regex(@"^Комната ?\d*$").IsMatch(value) || value == "Название команды";
        }

        private static bool IsColored(ExcelRangeBase cell)
        {
            return (cell.Style.Fill.BackgroundColor.Rgb != "FFFFFFFF"
                && cell.Style.Fill.BackgroundColor.Rgb != "")
                || (cell.Style.Fill.BackgroundColor.Theme != null
                && cell.Style.Fill.BackgroundColor.Theme != eThemeSchemeColor.Background1);
        }
    }
}
