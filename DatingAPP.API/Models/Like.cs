namespace DatingAPP.API.Models
{
    public class Like
    {
        // the first 2 properties are userId
        public int LikerId { get; set; }
        public int LikeeId { get; set; }
        public User Liker { get; set; }
        public User Likee { get; set; }
    }
}