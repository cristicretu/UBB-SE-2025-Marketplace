using System.Diagnostics.CodeAnalysis;

namespace MarketMinds.Shared.Models
{
    public class UserWaitList
    {
        [ExcludeFromCodeCoverage]
        public int UserWaitListID { get; set; }
        public int ProductWaitListID { get; set; }
        public int UserID { get; set; }
        public DateTime JoinedTime { get; set; }
        public int PositionInQueue { get; set; }
        public DateTime? PreferredEndDate { get; set; }
    }
}
