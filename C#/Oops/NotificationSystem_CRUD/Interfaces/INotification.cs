using NotificationSystem.Enums;

namespace NotificationSystem.Interfaces
{
    internal interface INotification
    {
        public NotificationType type { get; set; }
        void Send(string message, string recipient);


    }   
}
