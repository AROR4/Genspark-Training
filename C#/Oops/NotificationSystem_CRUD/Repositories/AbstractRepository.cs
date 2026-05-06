using NotificationSystem.Interfaces;

namespace NotificationSystem.Repositories
{
internal abstract class Repository<k,T> : IRepository<k,T>
{
    protected Dictionary<k,T> _items;

    public abstract T Create(T item);

    public virtual List<T> GetAll()
    {
        if(_items.Count==0) return null;
        var list=_items.Values.ToList();
        return list;
    }

    public abstract T GetById(k key);

    public abstract T Update(k key,T item);

    public abstract T Delete(k key);

    }
}