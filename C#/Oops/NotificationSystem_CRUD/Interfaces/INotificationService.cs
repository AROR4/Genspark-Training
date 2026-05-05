namespace NotificationSystem.Interfaces
{
    internal interface INotificationService
    {
        void SendEmail(String message,Models.User user);
        void SendSms(String message,Models.User user);

        void PrintHistory();

        
    }
}