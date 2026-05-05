using NotificationSystem.Interfaces;

namespace NotificationSystem.Models
{
    internal class EmailNotification : INotification
    {
        public int type =1;
        public new int GetType()
        {
            return type;
        }
        public void Send(string message, string recipient)
        {
            Console.WriteLine($"Email sent to {recipient}: {message} at {DateTime.Now}");
        }

       
    }
}
