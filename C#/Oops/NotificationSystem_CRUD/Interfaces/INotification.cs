namespace NotificationSystem.Interfaces
{
    internal interface INotification
    {
        int GetType();
        void Send(string message, string recipient);


    }   
}
