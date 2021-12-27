using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ProjectQueue
{
    public static class SpreadsheetManager
    {
        private static Timer checkSpreadsheetTimer;
        private static string subscribedAddress;
        private static Action subscribedCallback;

        public static void SubscribeToCell(string address, Action callback)
        {
            subscribedAddress = address;
            subscribedCallback = callback;
            checkSpreadsheetTimer = new Timer(CheckCellReadiness, null, 0, 3000);
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
                if (new Regex(@"^Комната ?\d*$").IsMatch(cell.Value.ToString())
                    || cell.Value.ToString() == "Название команды")
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
                if (upperCell.Style.Fill.BackgroundColor.Rgb == "FF00FF00")
                    subscribedCallback();
            }
        }
    }
}
