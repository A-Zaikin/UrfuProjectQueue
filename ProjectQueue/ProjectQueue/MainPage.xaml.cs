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

            DebugLabel = "New label text";
            client = new HttpClient();
        }

        void EntryCompleted(object sender, EventArgs e)
        {
            var entry = (Entry)sender;
            var text = entry.Text;
            //DebugLabel = text;
            var downloadUrl = FormDownloadUrl(text);
            //DebugLabel = downloadUrl;
            var xlsxFile = DownloadXlsxFile(downloadUrl);
            //DebugLabel = xlsxFile.Length.ToString();
            var a1Value = OpenExcelFile(xlsxFile);
            DebugLabel = a1Value;
        }

        string OpenExcelFile(byte[] xlsxFile)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (MemoryStream ms = new MemoryStream(xlsxFile))
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                if (package.Workbook.Worksheets.Count == 0)
                    return "Your Excel file does not contain any work sheets";
                else
                {
                    foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                    {
                        return worksheet.Cells["A1"].Value.ToString();
                    }
                }
            }
            return "Something failed...";
        }

        string FormDownloadUrl(string inputUrl)
        {
            var re = new Regex(@"\/d\/(.+)\/");
            var match = re.Match(inputUrl);
            var id = match.Groups[1].Value;
            var downloadUrl = $"https://docs.google.com/spreadsheets/d/{id}/export?format=xlsx&id={id}";
            return downloadUrl;
        }

        byte[] DownloadXlsxFile(string url)
        {
            var response = client.GetByteArrayAsync(url);
            return response.Result;
        }
    }
}
