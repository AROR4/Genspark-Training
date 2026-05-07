using NotificationSystem.Enums;
using NotificationSystem.Interfaces;

namespace NotificationSystem.Models
{
    internal class SmsNotification : INotification
    {   
        public NotificationType Type => NotificationType.Sms;

        public Notification Send(string message, User user)
        {
            Console.WriteLine($"SMS sent to {user.PhoneNumber}: {message} at {DateTime.Now}");
            return new Notification(message, Type, user.PhoneNumber);
        }
    }
}
