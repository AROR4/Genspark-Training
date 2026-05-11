using NotificationModelLibrary.Enums;
using NotificationModelLibrary;
using NotificationBLLibrary.Interfaces;



namespace NotificationBLLibrary.Senders
{
    public class SmsNotificationSender : INotificationSender
    {
        public Notification Send(string message, User user)
        {
            Console.WriteLine($"SMS sent to {user.PhoneNumber}: {message} at {DateTime.Now}");
            return new Notification(message, NotificationType.Sms, user.PhoneNumber,user.id);
        }
    }
}
