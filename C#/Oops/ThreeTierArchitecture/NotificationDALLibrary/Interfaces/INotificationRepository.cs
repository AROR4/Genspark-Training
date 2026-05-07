namespace NotificationDALLibrary.Interfaces
{
    public interface INotificationRepository<T>
    {
        T Create(T item);
        List<T> GetAll();

    }
}