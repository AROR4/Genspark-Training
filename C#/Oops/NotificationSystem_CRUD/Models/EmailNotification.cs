using NotificationSystem.Enums;
using NotificationSystem.Interfaces;

namespace NotificationSystem.Models
{
    internal class EmailNotification : INotification
    {
        public NotificationType type { get; set; }= NotificationType.Email;
       
        public void Send(string message, string recipient)
        {
            Console.WriteLine($"Email sent to {recipient}: {message} at {DateTime.Now}");
        }

       
    }
}
