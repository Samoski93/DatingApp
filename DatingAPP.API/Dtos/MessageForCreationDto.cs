using System;

namespace DatingAPP.API.Dtos
{
    public class MessageForCreationDto
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public DateTime MessageSent { get; set; }
        public string Content { get; set; }

        // So as to set dat - add a constructor
        public MessageForCreationDto()
        {
            MessageSent = DateTime.Now;
        }
    }
}