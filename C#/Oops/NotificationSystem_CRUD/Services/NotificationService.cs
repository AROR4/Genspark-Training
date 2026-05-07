using NotificationSystem.Interfaces;
using NotificationSystem.Models;
using NotificationSystem.Repositories;

namespace NotificationSystem.Services
{
    internal class NotificationService : INotificationService
    {

        NotificationRepository notificationRepository=new NotificationRepository();
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

        private void SendNotification(INotification notificationChannel, string message, string recipient)
        {
            notificationChannel.Send(message, recipient);
            
            Notification currentnotification=new Notification(message,notificationChannel.type,recipient);
            
            notificationRepository.Create(currentnotification);
        }

    }
}
