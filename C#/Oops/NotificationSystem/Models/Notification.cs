namespace  NotificationSystem.Models
{
    public class Notification
    {
        public enum NotifType
        {
            Email=1,SMS=2
        }
        public string Message { get; set; }=String.Empty;
        public DateTime SentDateTime { get; set; } = DateTime.Now;
        public NotifType NotificationType {get ; set;}

  
        
        public Notification(string message,NotifType type)
        {
            Message = message;
            NotificationType=type;
        }
    }
}