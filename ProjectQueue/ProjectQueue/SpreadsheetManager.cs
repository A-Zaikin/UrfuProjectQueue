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
        public static int TeamsLeftCount = 0;

        private static HashSet<string> completedTeams = new HashSet<string>();
        private static Timer checkSpreadsheetTimer;
        private static string subscribedAddress;
        private static Action<TeamNumber> subscribedCallback;
        private static Action updateTimeCallback;
        private static int sheetIndex = 0;

        public static void SubscribeToCell(string address, Action<TeamNumber> callback, Action updateTime)
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

        public static void SetSheetIndex(int index)
        {
            sheetIndex = index;
        }

        public static Dictionary<string, int> GetSheets(byte[] xlsxFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (MemoryStream ms = new MemoryStream(xlsxFile))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                return package.Workbook.Worksheets.ToDictionary(sheet => sheet.Name, sheet => sheet.Index);
            }
        }

        public static Dictionary<string, string> GetRooms(byte[] xlsxFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (MemoryStream ms = new MemoryStream(xlsxFile))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                var sheet = package.Workbook.Worksheets[sheetIndex];
                return FindRooms(sheet).ToDictionary(cell => cell.Value.ToString(), cell => cell.Address.ToString());
            }
        }

        public static Dictionary<string, string> GetTeams(byte[] xlsxFile, string roomAddress)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (MemoryStream ms = new MemoryStream(xlsxFile))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                var sheet = package.Workbook.Worksheets[sheetIndex];
                return FindTeams(sheet, roomAddress).ToDictionary(cell => cell.Value.ToString(), cell => cell.Address.ToString());
            }
        }

        private static List<ExcelRangeBase> FindTeams(ExcelWorksheet sheet, string roomAddress)
        {
            var teamCells = new List<ExcelRangeBase>();
            var previousCell = (ExcelRangeBase)sheet.Cells[roomAddress];
            while (true)
            {
                var currentCell = previousCell.Offset(1, 0);
                if (currentCell.Value == null || currentCell.Value.ToString() == "")
                    break;
                teamCells.Add(currentCell);
                previousCell = currentCell;
            }
            return teamCells;
        }

        private static List<ExcelRangeBase> FindRooms(ExcelWorksheet sheet)
        {
            var roomCells = new List<ExcelRangeBase>();
            foreach (var cell in sheet.Cells)
            {
                if (cell.Value == null)
                    continue;
                if (IsRoomHeader(cell.Value.ToString()))
                {
                    roomCells.Add(cell);
                }
            }
            return roomCells;
        }

        private static void CheckCellReadiness(object state)
        {
            var xlsxFile = HttpManager.DownloadXlsxFile();
            using (MemoryStream ms = new MemoryStream(xlsxFile))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                var sheet = package.Workbook.Worksheets[sheetIndex];
                var cell = sheet.Cells[subscribedAddress];
                var upperCell = cell.Offset(-1, 0);
                CheckCompletionTimes(upperCell);
                if (IsColored(upperCell))
                    subscribedCallback(TeamNumber.TeamUp1);
                else if (IsColored(cell.Offset(-2, 0)))
                    subscribedCallback(TeamNumber.TeamUp2);
                else if (IsColored(cell.Offset(-4, 0)))
                    subscribedCallback(TeamNumber.TeamUp4);
            }
        }

        private static void CheckCompletionTimes(ExcelRangeBase cell)
        {
            var currentCell = cell;
            var newTeamsLeftCount = 0;
            while (!IsRoomHeader(currentCell.Value.ToString()))
            {
                if (!IsColored(currentCell))
                {
                    newTeamsLeftCount++;
                }
                if (!completedTeams.Contains(currentCell.Address) && IsColored(currentCell))
                {
                    completedTeams.Add(currentCell.Address);
                    TeamCompletionTimes.Add(DateTime.Now);
                }
                currentCell = currentCell.Offset(-1, 0);
            }
            TeamsLeftCount = newTeamsLeftCount;
            updateTimeCallback();
        }

        private static bool IsRoomHeader(string value)
        {
            return new Regex(@"Комната ?").IsMatch(value) || value == "Название команды";
        }

        private static bool IsColored(ExcelRangeBase cell)
        {
            return (cell.Style.Fill.BackgroundColor.Rgb != "FFFFFFFF"
                && !string.IsNullOrEmpty(cell.Style.Fill.BackgroundColor.Rgb))
                || (cell.Style.Fill.BackgroundColor.Theme != null
                && cell.Style.Fill.BackgroundColor.Theme != eThemeSchemeColor.Background1);
        }

        public enum TeamNumber
        {
            TeamUp1,
            TeamUp2,
            TeamUp4
        }
    }
}
