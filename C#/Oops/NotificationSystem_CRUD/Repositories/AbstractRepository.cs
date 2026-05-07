using NotificationSystem.Interfaces;

namespace NotificationSystem.Repositories
{
    internal abstract class Repository<TKey, T> : IRepository<TKey, T> where TKey : notnull
    {
        protected readonly Dictionary<TKey, T> _items = new();

        public abstract T Create(T item);

        public virtual List<T> GetAll()
        {
            return _items.Values.ToList();
        }

        public abstract T? GetById(TKey key);

        public abstract T? Update(TKey key, T item);

        public abstract T? Delete(TKey key);
    }
}
