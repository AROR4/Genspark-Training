using NotificationModelLibrary.Enums;

namespace  NotificationModelLibrary
{
    public class Notification
    {

        public int Id {get;set;}
        public string Message { get; set; }=String.Empty;
        public DateTime SentDateTime { get; set; } = DateTime.Now;
        public NotificationType NotificationType {get ; set;}
        public string Recipient { get; set; }

        public int? UserId {get; set;}

        public User? User {get; set;}=null;

        


        
        public Notification()
        {

        }


        public Notification(string Message,NotificationType NotificationType, string Recipient,int? UserId)
        {
            this.Message = Message;
            this.NotificationType=NotificationType;
            this.Recipient=Recipient;
            this.UserId=UserId;
        }

        public override string ToString()
        {
            return $"{SentDateTime} - {NotificationType} - Recipient : {Recipient} : Message : {Message} \n";
        }

    }


}
