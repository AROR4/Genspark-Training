namespace NotificationSystem.Interfaces
{
    internal interface INotificationRepository<T>
    {
        T Create(T item);
        List<T> GetAll();

    }
}