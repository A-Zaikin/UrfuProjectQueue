using System;

namespace ProjectQueue
{
    public interface INotificationManager
    {
        void Initialize();
        void SendNotification(string title, string message);
    }
}
