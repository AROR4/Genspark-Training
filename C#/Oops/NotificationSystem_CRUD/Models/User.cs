namespace NotificationSystem.Models
{
    internal class User : IComparable<User>
    {
        public int id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;


        public User(string name, string email, string phoneNumber)
        {
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
        }

        public override string ToString()
        {
            return $"ID : {id} \n Name : {Name}\n Email : {Email}\n Phone Number : {PhoneNumber}\n";
        }

        public int CompareTo(User? other)
        {
           return this.id.CompareTo(other.id);
        }

    }
}
