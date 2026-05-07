using NotificationModelLibrary.Enums;

namespace  NotificationModelLibrary
{
    public class Notification
    {

        
        public string Message { get; set; }=String.Empty;
        public DateTime SentDateTime { get; set; } = DateTime.Now;
        public NotificationType NotificationType {get ; set;}

        public string Recipient { get; set; }

  
        
        public Notification(string message,NotificationType type, string recipient)
        {
            Message = message;
            NotificationType=type;
            Recipient=recipient;
        }

        public override string ToString()
        {
            return $"{SentDateTime} - {NotificationType} - Recepient : {Recipient} : Message : {Message} \n";
        }

    }


}