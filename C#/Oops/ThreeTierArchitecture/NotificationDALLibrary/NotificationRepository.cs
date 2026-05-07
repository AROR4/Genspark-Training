

using NotificationDALLibrary.Interfaces;
using NotificationModelLibrary;

namespace NotificationDALLibrary{
    public class NotificationRepository : INotificationRepository<Notification>
    {

        List<Notification> _notifications=new List<Notification>();
        public Notification Create(Notification item)
        {
            _notifications.Add(item);
            return item;
        }

        public List<Notification> GetAll()
        {
            var history=_notifications;
            return history;
        }

        
    }
}