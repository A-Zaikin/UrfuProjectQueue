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
        private static Action<string> subscribedCallback;

        public static void SubscribeToCell(string address, Action<string> callback)
        {
            subscribedAddress = address;
            subscribedCallback = callback;
            checkSpreadsheetTimer = new Timer(CheckCellReadiness, null, 0, 3000);
        }

        public static void Unsubscribe()
        {
            checkSpreadsheetTimer.Dispose();
        }

        public static List<string> GetTeams(byte[] xlsxFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (MemoryStream ms = new MemoryStream(xlsxFile))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                var sheet = package.Workbook.Worksheets[0];
                return FindTeams(sheet)
                    .Select(cell => cell.Value.ToString())
                    .ToList();
            }
        }

        private static List<ExcelRangeBase> FindTeams(ExcelWorksheet sheet)
        {
            var teamCells = new List<ExcelRangeBase>();
            foreach (var cell in sheet.Cells)
            {
                //if (cell.Value.ToString() == "Команда")
                if (new Regex(@"^Команда ?\d*$").IsMatch(cell.Value.ToString()))
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
                //subscribedCallback($"{DateTime.Now} - {cell.Value} - {cell.Style.Fill.BackgroundColor.Rgb}");
                if (cell.Style.Fill.BackgroundColor.Rgb == "FF00FF00")
                    subscribedCallback(DateTime.Now.ToString());
            }
        }
    }
}
