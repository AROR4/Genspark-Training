using NotificationSystem.Interfaces;
using NotificationSystem.Models;

namespace NotificationSystem.Services
{
    internal class NotificationService : INotificationService
    {
        private  List<Notification> notifications = [];


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

        public void SendNotification(INotification notificationChannel, string message, User user)
        {
            var current=notificationChannel.Send(message, user);
            
            notifications.Add(current);
        }

        
    }
}
