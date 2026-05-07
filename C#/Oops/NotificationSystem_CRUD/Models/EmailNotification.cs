using NotificationSystem.Enums;
using NotificationSystem.Interfaces;

namespace NotificationSystem.Models
{
    internal class EmailNotification : INotification
    {
        public Notification Send(string message, User user)
        {
            Console.WriteLine($"Email sent to {user.Email}: {message} at {DateTime.Now}");
            return new Notification(message, NotificationType.Email, user.Email);
        }
    }
}
