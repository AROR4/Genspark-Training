namespace NotificationModelLibrary
{
    public class User : IComparable<User>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public ICollection<Notification>? Notifications { get; set; }
        public User(string name, string email, string phoneNumber)
        {
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
        }

        public override string ToString()
        {
            return $"ID : {Id} \n Name : {Name}\n Email : {Email}\n Phone Number : {PhoneNumber}\n";
        }

        public int CompareTo(User? other)
        {
           return other is null ? 1 : Id.CompareTo(other.Id);
        }

    }
}
