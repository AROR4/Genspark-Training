using NotificationBLLibrary.Interfaces;
using NotificationModelLibrary;
using NotificationDALLibrary;
using NotificationModelLibrary.Exceptions;
using NotificationModelLibrary.Enums;
using NotificationBLLibrary.Senders;
using NotificationBLLibrary.Validators;

namespace NotificationBLLibrary
{
    public class NotificationService : INotificationService
    {
        NotificationRepository notificationRepository = new NotificationRepository();

        public List<Notification> GetHistory()
        {
            return notificationRepository.GetAll();
        }

        public void SendNotification(NotificationType notificationType, User user, string message)
        {
            INotificationSender notificationChannel;
            switch (notificationType)
            {
                case NotificationType.Email:
                    notificationChannel = new EmailNotificationSender();
                    break;
                case NotificationType.Sms:
                    notificationChannel = new SmsNotificationSender();
                    break;
                default:
                    Console.WriteLine("Invalid notification type.");
                    return;
            }
            NotificationValidator.ValidateMessage(message, notificationType);
            Notification currentnotification = notificationChannel.Send(message, user);
            notificationRepository.Create(currentnotification);
        }

        public void SendToAllUsers(NotificationType notificationType, List<User> users, string message)
        {
            INotificationSender notificationChannel;
            switch (notificationType)
            {
                case NotificationType.Email:
                    notificationChannel = new EmailNotificationSender();
                    break;
                case NotificationType.Sms:
                    notificationChannel = new SmsNotificationSender();
                    break;
                default:
                    Console.WriteLine("Invalid notification type.");
                    return;
            }
            if (users.Count == 0)
            {
                Console.WriteLine("No user added yet. Please add user first");
                return;
            }
            NotificationValidator.ValidateMessage(message, notificationType);
            foreach(User user in users)
            {
                Notification currentnotification = notificationChannel.Send(message, user);
                notificationRepository.Create(currentnotification);
            }
        }
    }
}
