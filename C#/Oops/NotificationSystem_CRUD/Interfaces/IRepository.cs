
namespace NotificationSystem.Interfaces
{
internal interface IRepository<k,T> 
{
    T Create(T item);
    List<T> GetAll();
    T GetById(int k);
    T Update(int k,T item);  
    T Delete(int k);
}
}