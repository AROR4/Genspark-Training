namespace NotificationDALLibrary.Interfaces
{
    public interface IRepository<K, T> 
    {
        T Create(T user);
        List<T> GetAll();
        T? GetById(K key);
        T? Update(K key, T user);
        T? Delete(K key);
    }
}
