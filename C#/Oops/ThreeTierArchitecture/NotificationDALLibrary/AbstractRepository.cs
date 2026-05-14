using Microsoft.EntityFrameworkCore;
using NotificationDALLibrary.Contexts;
using NotificationDALLibrary.Interfaces;

namespace NotificationDALLibrary
{
    public abstract class Repository<TKey, T> : IRepository<TKey, T> where T : class where TKey : notnull
    {
        
        protected NotificationContext _notificationContext=new NotificationContext();
        public virtual T Create(T item)
        {
            try
            {
                _notificationContext.Set<T>().Add(item);
                _notificationContext.SaveChanges();
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Database update failed: {ex.Message}");
            }
        }

        public virtual List<T> GetAll()
        {
            return _notificationContext.Set<T>().ToList();
        }

        public abstract T? GetById(TKey key);

        public abstract T? Update(TKey key, T item);

        public abstract T? Delete(TKey key);
    }
}
