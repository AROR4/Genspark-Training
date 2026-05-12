namespace Wordle.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int TotalScore { get; set; }=0;

    }
}
