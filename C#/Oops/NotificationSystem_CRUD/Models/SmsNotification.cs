using NotificationSystem.Enums;
using NotificationSystem.Interfaces;

namespace NotificationSystem.Models
{
    internal class SmsNotification : INotification
    {
        public Notification Send(string message, User user)
        {
            Console.WriteLine($"SMS sent to {user.PhoneNumber}: {message} at {DateTime.Now}");
            return new Notification(message, NotificationType.Sms, user.PhoneNumber);
        }
    }
}
