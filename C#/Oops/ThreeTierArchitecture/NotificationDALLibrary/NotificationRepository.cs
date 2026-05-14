

using NotificationDALLibrary.Contexts;
using NotificationDALLibrary.Interfaces;
using NotificationModelLibrary;

namespace NotificationDALLibrary{
    public class NotificationRepository : INotificationRepository<Notification>
    {

        NotificationContext _notificationContext=new NotificationContext();
        public Notification Create(Notification item)
        {
            try
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }
                _notificationContext.Add(item);
                _notificationContext.SaveChanges();

                return item;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Could not create notification: {ex.InnerException?.Message}");
            }
        }

        public List<Notification> GetAll()
        {
            try
            {
                return _notificationContext.Set<Notification>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Could not fetch notifications: {ex.Message}");
            }
        }

        
    }
}