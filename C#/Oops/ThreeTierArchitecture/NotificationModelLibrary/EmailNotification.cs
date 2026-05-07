using NotificationModelLibrary.Enums;
using NotificationModelLibrary.Interfaces;
using NotificationModelLibrary;

namespace NotificationModelLibrary
{
    public class EmailNotification : INotification
    {
        public Notification Send(string message, User user)
        {
            Console.WriteLine($"Email sent to {user.Email}: {message} at {DateTime.Now}");
            return new Notification(message, NotificationType.Email, user.Email);
        }
    }
}
