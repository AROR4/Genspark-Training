using NotificationModelLibrary;

namespace NotificationDALLibrary{
public class UserRepository : Repository<int,User>
{
    static int lastid = 0;

    public override User Create(User item)
    {
       int userid=++lastid;
       lastid=userid;
       item.id=userid;
       _items.Add(userid,item);
       return _items[userid];

    }
    public override User? GetById(int id)
    {
        if (_items.TryGetValue(id, out var user))
        {
            return user;
        }
        return null;
    }

    public override User? Update(int id, User updatedUser)
    {
        var user = GetById(id);

        if (user != null)
        {
            _items[id]=updatedUser;
            return GetById(id);
        }
        return null;
    }

    public override User? Delete(int id)
    {
        var user = GetById(id);
        if (user != null)
            _items.Remove(id);
        return user;
    }
}

   
}
