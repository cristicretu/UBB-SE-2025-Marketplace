namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Contains information about the follow status between a buyer and seller
    /// </summary>
    public class BuyerSellerFollowInfo
    {
        /// <summary>
        /// Gets or sets a value indicating whether the buyer is following the seller
        /// </summary>
        public bool IsFollowing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user can manage (follow/unfollow) this relationship
        /// </summary>
        public bool CanManageFollow { get; set; }

        /// <summary>
        /// Gets or sets the date when the follow relationship was created (if following)
        /// </summary>
        public DateTime? FollowedDate { get; set; }
    }
}