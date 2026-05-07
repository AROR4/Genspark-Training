using NotificationSystem.Interfaces;
using NotificationSystem.Enums;

namespace NotificationSystem.Models
{
    internal class EmailNotification : INotification
    {
        public NotificationType Type => NotificationType.Email;

        public Notification Send(string message, User user)
        {
            Console.WriteLine($"Email sent to {user.Email}: {message} at {DateTime.Now}");
            return new Notification(message, NotificationType.Email, user.Email);
        }

      
    }
}
