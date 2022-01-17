using Android.App;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;
using Android.Widget;
[assembly: Dependency(typeof(ProjectQueue.Droid.AndroidMessageManager))]
namespace ProjectQueue.Droid
{
    public class AndroidMessageManager : IMessageManager
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(AndroidApp.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(AndroidApp.Context, message, ToastLength.Short).Show();
        }
    }
}