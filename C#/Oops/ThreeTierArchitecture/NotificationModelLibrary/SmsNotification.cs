using NotificationModelLibrary.Enums;
using NotificationModelLibrary.Interfaces;


namespace NotificationModelLibrary
{
    public class SmsNotification : INotification
    {
        public Notification Send(string message, User user)
        {
            Console.WriteLine($"SMS sent to {user.PhoneNumber}: {message} at {DateTime.Now}");
            return new Notification(message, NotificationType.Sms, user.PhoneNumber);
        }
    }
}
