using NotificationSystem.Models;

namespace NotificationSystem.Interfaces
{
    internal interface INotification
    {
        Notification Send(string message, User user);
    }
}
