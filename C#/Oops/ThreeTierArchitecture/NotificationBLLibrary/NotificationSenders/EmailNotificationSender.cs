using NotificationModelLibrary.Enums;
using NotificationModelLibrary;
using NotificationBLLibrary.Interfaces;

namespace NotificationBLLibrary.Senders
{
    public class EmailNotificationSender : INotificationSender
    {
        public Notification Send(string Message, User user)
        {
            Console.WriteLine($"Email sent to {user.Email}: {Message} at {DateTime.Now}");

            return new Notification(Message, NotificationType.Email, user.Email,user.Id);
        }
    }
}
