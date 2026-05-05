using NotificationSystem.Models;

namespace NotificationSystem.Repositories{
internal class UserRepository : Repository<int,User>
{
    static int lastid = 0;
    Dictionary<int,User> _users=new Dictionary<int, User>();

    public override User Create(User item)
    {
       int userid=++lastid;
       lastid=userid;
       item.id=userid;
       _users.Add(userid,item);
       return _users[userid];

    }
    public List<User> Get()
    {
        if(_users.Count == 0) 
                return null;
            var list = _users.Values.ToList();
            list.Sort();
            return list;
    }
    public override User? GetById(int id)
    {
        User user;
        if(_users.TryGetValue(id,out user))
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
            _users[id]=updatedUser;
            return GetById(id);
        }
        return null;
    }

    public override User? Delete(int id)
    {
        var user = GetById(id);
        if (user != null)
            _users.Remove(id);
        return user;
    }
}

   
}