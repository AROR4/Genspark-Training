using NotificationModelLibrary;
using NotificationModelLibrary.Enums;



namespace NotificationBLLibrary.Interfaces
{
    public interface INotificationService
    {
        void SendNotification(NotificationType notificationType, User user, string message);
        List<Notification> GetHistory();

        void SendToAllUsers(NotificationType notificationType, List<User> users, string message);


    }
}
