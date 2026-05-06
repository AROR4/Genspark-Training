
namespace NotificationSystem.Interfaces
{
internal interface IRepository<k,T> 
{
    T Create(T item);
    List<T> GetAll();
    T GetById(k key);
    T Update(k key,T item);  
    T Delete(k key);
}
}