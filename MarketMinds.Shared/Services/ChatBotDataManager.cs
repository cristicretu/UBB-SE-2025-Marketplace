namespace Marketplace_SE.Service
{
    public static class ChatBotDataManager
    {
        public static Node LoadTree()
        {
            // Return a single empty root node
            return new Node { Id = 1 };
        }
    }

    public class Node
    {
        public int Id { get; set; }
        public string ButtonLabel { get; set; } = string.Empty;
        public string LabelText { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public List<Node> Children { get; set; } = new List<Node>();
    }
}