using NotificationModelLibrary;

namespace NotificationBLLibrary.Interfaces
{
    public interface INotificationSender
    {
        Notification Send(string message, User user);
    }
}
