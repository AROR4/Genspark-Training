using NotificationSystem.Models;

namespace NotificationSystem.Interfaces
{
    internal interface INotificationService
    {
        void SendNotification(INotification notificationChannel, string message, User user);
        void PrintHistory();
    }
}
