namespace DatingAPP.API.Helpers
{
    public class MessageParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;    // Always return the first page (set to 1)
        private int pageSize = 10;  // max 10 users
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }
        public int UserId { get; set; }

        // For messages received, filter out messages sent based on the userId
        public string MessageContainer { get; set; } = "Unread";
    }
}