using MarketMinds.Shared.Models;

namespace MarketMinds.Web.Models
{
    public class ChatViewModel
    {
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
        public Conversation CurrentConversation { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
        public string NewMessage { get; set; }
    }
}
