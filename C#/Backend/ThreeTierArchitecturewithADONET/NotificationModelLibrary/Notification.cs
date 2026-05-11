using NotificationModelLibrary.Enums;

namespace  NotificationModelLibrary
{
    public class Notification
    {   
        public string Message { get; set; }=String.Empty;
        public DateTime SentDateTime { get; set; } = DateTime.Now;
        public NotificationType NotificationType {get ; set;}

        public string Recipient { get; set; } = string.Empty;
        public int userId {get; set;}
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhoneNumber { get; set; } = string.Empty;

        public Notification(string message,NotificationType type, string recipient,int userid)
        {
            Message = message;
            NotificationType=type;
            Recipient=recipient;
            userId=userid;
        }
        
        public Notification(string message, NotificationType type, string recipient, DateTime dateTime, int userid)
        {
            Message = message;
            NotificationType = type;
            Recipient = recipient;
            SentDateTime = dateTime;
            userId = userid;
        }

        public override string ToString()
        {
            string userDetails = string.IsNullOrWhiteSpace(UserName)
                ? string.Empty
                : $"User : {UserName} | Email : {UserEmail} | Phone : {UserPhoneNumber} | ";

            return $"{SentDateTime} - {NotificationType} - {userDetails}Recipient : {Recipient} : Message : {Message} \n";
        }

    }


}
