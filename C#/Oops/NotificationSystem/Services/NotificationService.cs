using NotificationSystem.Interfaces;
using NotificationSystem.Models;

namespace NotificationSystem.Services
{
    internal class NotificationService : INotificationService
    {
        private  List<Notification> notifications = [];

        public void SendEmail(string message,User user)
        {
            SendNotification(new EmailNotification(), message, user.Email);
        }

        public void SendSms( string message, User user)
        {
            SendNotification(new SmsNotification(), message, user.PhoneNumber);
        }

        public void PrintHistory()
        {
            if (notifications.Count == 0)
            {
                Console.WriteLine("No notifications sent yet.");
                return;
            }

            Console.WriteLine("Notification History");
            foreach (Notification notification in notifications)
            {
                Console.WriteLine($"{notification.NotificationType} {notification.SentDateTime:g} - {notification.Message}");
            }
        }

        private void SendNotification(INotification notificationChannel, string message, string recipient)
        {
            notificationChannel.Send(message, recipient);
            
            notifications.Add(new Notification(message,notificationChannel.GetType()==1? Notification.NotifType.Email : Notification.NotifType.SMS));
        }

    }
}
