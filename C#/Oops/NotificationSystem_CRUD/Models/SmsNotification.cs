using NotificationSystem.Interfaces;

namespace NotificationSystem.Models
{
    internal class SmsNotification : INotification
    {   
        public int type =2;
        public new int GetType()
        {
            return type;
        }
        public void Send(string message, string recipient)
        {
            Console.WriteLine($"SMS sent to {recipient}: {message} at {DateTime.Now}");
        }
    }
}
