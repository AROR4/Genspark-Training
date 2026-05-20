namespace LibraryManagementSystem.Models
{
    public class Member
    {
        public int MemberId { get; set; }
        public string FullName { get; set; }=String.Empty;
        public string Email { get; set; }=String.Empty;
        public string PhoneNumber { get; set; }=String.Empty;
        public DateTime MembershipDate { get; set; }=DateTime.Now;
    }
}