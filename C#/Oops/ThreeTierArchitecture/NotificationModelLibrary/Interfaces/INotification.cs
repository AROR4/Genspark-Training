namespace NotificationModelLibrary.Interfaces
{
    public interface INotification
    {
        Notification Send(string message, User user);
    }
}
