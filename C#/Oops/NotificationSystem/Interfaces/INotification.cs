using NotificationSystem.Enums;
using NotificationSystem.Models;

namespace NotificationSystem.Interfaces
{
    internal interface INotification
    {
        NotificationType Type { get; }
        Notification Send(string message, User user);

    }   
}
