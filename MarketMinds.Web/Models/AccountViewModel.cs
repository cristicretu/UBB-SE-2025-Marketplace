using MarketMinds.Shared.Models;

namespace MarketMinds.Web.Models
{
    public class AccountViewModel
    {
        public UserDto User { get; set; }
        public List<UserOrder> Orders { get; set; }
    }
}