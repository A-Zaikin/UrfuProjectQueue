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

namespace ProjectQueue
{
    public partial class MainPage : ContentPage
    {
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
        }

        void EntryCompleted(object sender, EventArgs e)
        {
            var entry = (Entry)sender;
            var text = entry.Text;
            DebugLabel = text;
            var downloadUrl = FormDownloadUrl(text);
            DebugLabel = downloadUrl;
            //var resultTask = DownloadXlsxFile(downloadUrl);
            //resultTask.Wait();
            //DebugLabel = resultTask.Result;
        }

        string FormDownloadUrl(string inputUrl)
        {
            var re = new Regex(@"\/d\/(.+)\/");
            var match = re.Match(inputUrl);
            var id = match.Groups[1].Value;
            var downloadUrl = $"https://docs.google.com/spreadsheets/d/{id}/export?format=xlsx&id={id}";
            return downloadUrl;
        }

        async Task<string> DownloadXlsxFile(string url)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = response.Content;
                var json = await responseContent.ReadAsStringAsync();
                return json;
            }
            else
                return $"Download failed with code: {response.StatusCode}";
        }
    }
}
