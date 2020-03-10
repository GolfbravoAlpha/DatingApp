namespace DatingApp.API.Models
{
    public class Like
    {
        public int LikerId { get; set; } //user likes another user
        public int LikeeId { get; set; } //user being liked by another user
        public User Liker { get; set; }
        public User Likee { get; set; }
    }
}