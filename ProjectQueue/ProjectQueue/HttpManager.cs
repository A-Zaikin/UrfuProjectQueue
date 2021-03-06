using System.Net.Http;
using System.Text.RegularExpressions;

namespace ProjectQueue
{
    public static class HttpManager
    {
        private static HttpClient client = new HttpClient();

        public static string SpreadsheetUrl { get; set; }

        public static byte[] DownloadXlsxFile()
        {
            var url = FormDownloadUrl(SpreadsheetUrl);
            var response = client.GetByteArrayAsync(url);
            return response.Result;
        }

        private static string FormDownloadUrl(string spreadsheetUrl)
        {
            var re = new Regex(@"\/d\/(.+)\/");
            var match = re.Match(spreadsheetUrl);
            var id = match.Groups[1].Value;
            var downloadUrl = $"https://docs.google.com/spreadsheets/d/{id}/export?format=xlsx&id={id}";
            return downloadUrl;
        }
    }
}
