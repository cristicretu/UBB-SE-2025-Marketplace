namespace MarketMinds.Shared.Models;

public class UserOrder
{
    public int Id { get; set; }

    // Client-side property for display
    public string Name { get; set; } = string.Empty;

    // Server-side property from API
    public string ItemName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Client-side property for display
    public float Cost { get; set; }

    // Server-side property from API
    public float Price { get; set; }

    // Created timestamp - used for display purposes
    public ulong Created { get; set; }

    public int SellerId { get; set; }
    public int BuyerId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
}
