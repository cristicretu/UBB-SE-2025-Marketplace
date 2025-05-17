namespace MarketMinds.Shared.Models;

public class Node()
{
    public int? Id { get; set; } // Nullable
    // Button Text
    public required string ButtonLabel { get; set; } = string.Empty;
    // Label Text
    public required string LabelText { get; set; } = string.Empty;
    // Response Text
    public required string Response { get; set; } = string.Empty;
    // Children of current node
    public List<Node> Children { get; set; } = new List<Node>();
}