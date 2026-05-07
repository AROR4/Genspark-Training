namespace NotificationSystem.Interfaces
{
    internal interface IRepository<K, T> 
    {
        T Create(T item);
        List<T> GetAll();
        T? GetById(K key);
        T? Update(K key, T item);
        T? Delete(K key);
    }
}
