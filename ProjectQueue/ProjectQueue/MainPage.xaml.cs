using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using OfficeOpenXml;

namespace ProjectQueue
{
    public partial class MainPage : ContentPage
    {
        private HttpClient client;

        //private Picker teamPicker;

        private string debugLabel;
        public string DebugLabel
        {
            get { return debugLabel; }
            set
            {
                debugLabel = value;
                OnPropertyChanged(nameof(DebugLabel)); // Notify that there was a change on this property
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            teamPicker = (Picker)FindByName("teamPicker");
            DebugLabel = "New label text";
        }

        void EntryCompleted(object sender, EventArgs e)
        {
            var entry = (Entry)sender;
            var text = entry.Text;
            var xlsxFile = HttpManager.DownloadXlsxFile(text);
            //DebugLabel = xlsxFile.Length.ToString();
            var teams = OpenExcelFile(xlsxFile);
            DebugLabel = string.Join(" ", teams);
            teamPicker.ItemsSource = teams;
        }

        List<string> OpenExcelFile(byte[] xlsxFile)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (MemoryStream ms = new MemoryStream(xlsxFile))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                var sheet = package.Workbook.Worksheets[0];
                return FindTeams(sheet)
                    .Select(cell => cell.Value.ToString())
                    .ToList();
            }


            List<ExcelRangeBase> FindTeams(ExcelWorksheet sheet)
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
        }

        private void TeamPickerSelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
