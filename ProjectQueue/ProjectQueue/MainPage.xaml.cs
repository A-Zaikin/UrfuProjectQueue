using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectQueue
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            var entry = ((Entry)sender);
            var text = entry.Text;
            DisplayAlert("Debug", text, "OK");
        }
    }
}
