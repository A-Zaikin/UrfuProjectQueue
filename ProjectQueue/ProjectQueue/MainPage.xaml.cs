﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ProjectQueue
{
    public partial class MainPage : ContentPage
    {
        private INotificationManager notificationManager;
        private IMessageManager messageManager;
        private string debugLabel;
        private Dictionary<string, string> teams;
        private Dictionary<string, int> sheets;
        private byte[] xlsxFile;
        private int sheetNumber = 0;
        public string DebugLabel
        {
            get { return debugLabel; }
            set
            {
                debugLabel = value;
                OnPropertyChanged(nameof(DebugLabel));
            }
        }

        public MainPage()
        {
            InitializeComponent();
            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.Initialize();
            messageManager = DependencyService.Get<IMessageManager>();
            BindingContext = this;

            entry = (Entry)FindByName("entry");
            teamPicker = (Picker)FindByName("teamPicker");
            sheetPicker = (Picker) FindByName("sheetPicker");
            unsubscribeButton = (Button)FindByName("unsubscribeButton");
            timeLabel = (Label)FindByName("timeLabel");
            DebugLabel = "New label text";
            Options = (StackLayout)FindByName("Options");
        }

        private void EntryCompleted(object sender, EventArgs e)
        {
            var entry = (Entry)sender;
            HttpManager.SpreadsheetUrl = entry.Text;
            xlsxFile = HttpManager.DownloadXlsxFile();
            sheets = SpreadsheetManager.GetSheets(xlsxFile);
            sheetPicker.ItemsSource = sheets.Keys.ToList();
            sheetPicker.IsVisible = true;
        }

        private void SheetPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            if (picker.SelectedIndex < 0)
                return;
            sheetNumber = sheets[picker.Items[picker.SelectedIndex]];
            teams = SpreadsheetManager.GetTeams(xlsxFile, sheetNumber);
            DebugLabel = string.Join(" ", teams.Keys);
            teamPicker.ItemsSource = teams.Keys.ToList();
            teamPicker.IsVisible = true;
            //roomPicker.IsVisible = true;
        }

        private void RoomPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void TeamPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            if (picker.SelectedIndex < 0)
                return;
            var team = picker.Items[picker.SelectedIndex];
            DebugLabel = team;
            SpreadsheetManager.SubscribeToCell(teams[team], () =>
            {
                notificationManager.SendNotification("Подошла Ваша очередь на защиту!", $"Вы представляете команду \"{team}\"");
                SpreadsheetManager.Unsubscribe();
            }, UpdateAverageTime);
            unsubscribeButton.IsVisible = true;
            messageManager.ShortAlert("Вы успешно подписаны");
            Options.IsVisible = true;
        }

        private void UnsubscribeButtonPressed(object sender, EventArgs e)
        {
            SpreadsheetManager.Unsubscribe();
            sheetPicker.IsVisible = false;
            teamPicker.IsVisible = false;
            unsubscribeButton.IsVisible = false;
            Options.IsVisible = false;
        }

        private void UpdateAverageTime()
        {
            var times = SpreadsheetManager.TeamCompletionTimes;
            if (times.Count > 1)
            {
                var timeDiffs = times.Zip(times.Skip(1), (time1, time2) => time2 - time1);
                var averageTime = timeDiffs.Average(timeSpan => timeSpan.TotalSeconds);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var timeSpan = TimeSpan.FromSeconds(averageTime);
                    timeLabel.Text = $"Среднее время защиты: {timeSpan.Minutes} минут {timeSpan.Seconds} секунд";
                });
            }
        }
    }
}
