using NotificationSystem.Interfaces;

namespace NotificationSystem.Repositories
{
internal abstract class Repository<k,T> : IRepository<k,T>
{
    protected List<T> items = new List<T>();

    public virtual T Create(T item)
    {
        items.Add(item);
        return item;
    }

    public virtual List<T> GetAll()
    {
        return items;
    }

    public abstract T GetById(int k);

    public abstract T Update(int k,T item);

    public abstract T Delete(int k);

    }
}