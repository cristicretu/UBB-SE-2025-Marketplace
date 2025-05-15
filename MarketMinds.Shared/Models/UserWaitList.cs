using System;
using System.Diagnostics.CodeAnalysis;

namespace SharedClassLibrary.Domain
{
    public class UserWaitList
    {
        [ExcludeFromCodeCoverage]
        public int UserWaitListID { get; set; }
        public int ProductWaitListID { get; set; }
        public int UserID { get; set; }
        public DateTime JoinedTime { get; set; }
        public int PositionInQueue { get; set; }
    }
}
