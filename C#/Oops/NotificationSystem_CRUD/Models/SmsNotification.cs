using NotificationSystem.Enums;
using NotificationSystem.Interfaces;

namespace NotificationSystem.Models
{
    internal class SmsNotification : INotification
    {   
        public NotificationType type { get; set; }= NotificationType.Sms;
        public void Send(string message, string recipient)
        {
            Console.WriteLine($"SMS sent to {recipient}: {message} at {DateTime.Now}");
        }
    }
}
