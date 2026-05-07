using NotificationSystem.Interfaces;
using NotificationSystem.Models;
using NotificationSystem.Repositories;

namespace NotificationSystem.Services
{
    internal class NotificationService : INotificationService
    {
        NotificationRepository notificationRepository=new NotificationRepository();

        public void PrintHistory()
        {
            var notifications=notificationRepository.GetAll();
            if (notifications.Count == 0)
            {
                Console.WriteLine("No notifications sent yet.");
                return;
            }

            Console.WriteLine("Notification History");
            foreach (Notification notification in notifications)
            {
                Console.WriteLine(notification);
            }
        }

        public void SendNotification(INotification notificationChannel, string message, User user)
        {
            Notification currentnotification = notificationChannel.Send(message, user);
            notificationRepository.Create(currentnotification);
        }
    }
}
