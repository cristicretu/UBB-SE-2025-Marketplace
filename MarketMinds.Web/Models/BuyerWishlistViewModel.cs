// BuyerWishlistViewModel.cs
using MarketMinds.Shared.Models;

namespace WebMarketplace.Models
{
    /// <summary>
    /// Represents a group of wishlist items belonging to a specific buyer
    /// </summary>
    public class BuyerWishlistGroup
    {
        /// <summary>
        /// Gets or sets the buyer ID who owns these wishlist items
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the buyer
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the buyer
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the full display name of the buyer
        /// </summary>
        public string DisplayName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// Gets or sets the wishlist items for this buyer
        /// </summary>
        public List<BuyProduct> WishlistItems { get; set; } = new List<BuyProduct>();

        /// <summary>
        /// Gets the count of items in this buyer's wishlist
        /// </summary>
        public int ItemCount => WishlistItems?.Count ?? 0;
    }

    /// <summary>
    /// View model for the buyer wishlist page
    /// </summary>
    public class BuyerWishlistViewModel
    {
        /// <summary>
        /// Gets or sets the buyer ID
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the list of wishlist items (for "My Wishlist" view)
        /// </summary>
        public List<BuyProduct> WishlistItems { get; set; } = new List<BuyProduct>();

        /// <summary>
        /// Gets or sets the grouped wishlist items (for "Friends Wishlist" view)
        /// </summary>
        public List<BuyerWishlistGroup> GroupedWishlistItems { get; set; } = new List<BuyerWishlistGroup>();

        /// <summary>
        /// Gets or sets the current view mode ("my" or "friends")
        /// </summary>
        public string ViewMode { get; set; } = "my";

        /// <summary>
        /// Gets whether we're viewing the "My Wishlist" mode
        /// </summary>
        public bool IsMyWishlistView => ViewMode?.ToLower() == "my";

        /// <summary>
        /// Gets whether we're viewing the "Friends Wishlist" mode
        /// </summary>
        public bool IsFriendsWishlistView => ViewMode?.ToLower() == "friends";

        /// <summary>
        /// Gets or sets the count of items in the wishlist (for "My Wishlist" view)
        /// </summary>
        public int ItemCount => WishlistItems?.Count ?? 0;

        /// <summary>
        /// Gets the total count of items across all friends' wishlists
        /// </summary>
        public int TotalFriendsItemCount => GroupedWishlistItems?.Sum(g => g.ItemCount) ?? 0;

        /// <summary>
        /// Gets a value indicating whether the wishlist is empty
        /// </summary>
        public bool IsEmpty => IsMyWishlistView ? ItemCount == 0 : TotalFriendsItemCount == 0;

        /// <summary>
        /// Gets the current buyer's first name (for "My Wishlist" display)
        /// </summary>
        public string CurrentBuyerFirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the current buyer's last name (for "My Wishlist" display)
        /// </summary>
        public string CurrentBuyerLastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the current buyer's display name
        /// </summary>
        public string CurrentBuyerDisplayName => $"{CurrentBuyerFirstName} {CurrentBuyerLastName}".Trim();
    }
}
