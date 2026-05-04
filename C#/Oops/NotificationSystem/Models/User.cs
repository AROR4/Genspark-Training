namespace NotificationSystem.Models
{
    internal class User
    {
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
            return $"Name : {Name}\n Email : {Email}\n Phone Number : {PhoneNumber}";
        }
    }
}
