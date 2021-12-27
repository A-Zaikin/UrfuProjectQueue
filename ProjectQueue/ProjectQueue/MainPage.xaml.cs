using System;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace ProjectQueue
{
    public partial class MainPage : ContentPage
    {
        private INotificationManager notificationManager;

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
            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.Initialize();
            BindingContext = this;

            teamPicker = (Picker)FindByName("teamPicker");
            DebugLabel = "New label text";
        }

        private void EntryCompleted(object sender, EventArgs e)
        {
            var entry = (Entry)sender;
            HttpManager.SpreadsheetUrl = entry.Text;
            var xlsxFile = HttpManager.DownloadXlsxFile();
            var teams = SpreadsheetManager.GetTeams(xlsxFile);
            DebugLabel = string.Join(" ", teams);
            teamPicker.ItemsSource = teams;
        }

        private void TeamPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            DebugLabel = picker.Items[picker.SelectedIndex];
            SpreadsheetManager.SubscribeToCell("A1", (time) =>
            {
                //MainThread.BeginInvokeOnMainThread(() =>
                //{
                //    //DisplayAlert("Your queue!", time, "OK");
                //    //SpreadsheetManager.Unsubscribe();
                //});
                notificationManager.SendNotification("Test title", time);
            });
        }

        private void UnsubscribeButtonPressed(object sender, EventArgs e)
        {
            SpreadsheetManager.Unsubscribe();
        }
    }
}
