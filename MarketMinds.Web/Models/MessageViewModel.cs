namespace MarketMinds.Web.Models
{
    [Serializable]
    public class MessageViewModel
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
    }
} 