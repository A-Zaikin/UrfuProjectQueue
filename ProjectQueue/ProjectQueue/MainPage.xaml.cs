﻿using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace ProjectQueue
{
    public partial class MainPage : ContentPage
    {
        private INotificationManager notificationManager;
        private IMessageManager messageManager;
        private string debugLabel;
        private Dictionary<string, string> teams;
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
            unsubscribeButton = (Button)FindByName("unsubscribeButton");
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
            teamPicker.IsVisible = true;
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
            });
            unsubscribeButton.IsVisible = true;
            messageManager.ShortAlert("Вы успешно подписаны");
        }

        private void UnsubscribeButtonPressed(object sender, EventArgs e)
        {
            SpreadsheetManager.Unsubscribe();
            teamPicker.IsVisible = false;
            unsubscribeButton.IsVisible = false;
        }
    }
}
