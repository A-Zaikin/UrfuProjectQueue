using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ProjectQueue
{
    public partial class MainPage : ContentPage
    {
        private INotificationManager notificationManager;

        private string debugLabel;
        private Dictionary<string, string> teams;
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
            teams = SpreadsheetManager.GetTeams(xlsxFile);
            DebugLabel = string.Join(" ", teams.Keys);
            teamPicker.ItemsSource = teams.Keys.ToList();
        }

        private void TeamPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            var team = picker.Items[picker.SelectedIndex];
            DebugLabel = team;
            SpreadsheetManager.SubscribeToCell(teams[team], (time) =>
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
