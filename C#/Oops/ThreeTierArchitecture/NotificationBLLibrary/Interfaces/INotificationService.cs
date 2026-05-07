using NotificationModelLibrary;
using NotificationModelLibrary.Interfaces;


namespace NotificationBLLibrary.Interfaces
{
    public interface INotificationService
    {
        void SendNotification(INotification notificationChannel, string message, User user);
        void PrintHistory();
    }
}
