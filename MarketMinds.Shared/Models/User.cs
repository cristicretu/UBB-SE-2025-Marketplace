using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("phoneNumber")]
        public string PhoneNumber { get; set; }

        [Column("passwordHash")]
        public string? PasswordHash { get; set; }

        [NotMapped]
        public string Password { get; set; } = string.Empty;

        [Column("userType")]
        public int UserType { get; set; }

        [Column("balance")]
        public double Balance { get; set; }

        [Column("failedLogIns")]
        public int FailedLogIns { get; set; }

        [Column("bannedUntil")]
        public DateTime? BannedUntil { get; set; }

        [Column("isBanned")]
        public bool IsBanned { get; set; }

        [NotMapped]
        public string Token { get; set; } = string.Empty;

        [NotMapped]
        public double Rating { get; set; }

        private const double MAX_BALANCE = 999999;

        // Navigation properties
        public ICollection<AuctionProduct> SellingItems { get; set; }
        public ICollection<Bid> Bids { get; set; }

        // Default constructor for EF Core
        public User()
        {
        }

        public User(int id = 0, string username = "", string email = "", string phoneNumber = "", int userType = (int)UserRole.Unassigned, double balance = 0, DateTime? bannedUntil = null, bool isBanned = false, int failedLogins = 0, string passwordHash = "", string token = "")
        {
            Username = username;
            Email = email;
            PhoneNumber = phoneNumber;
            PasswordHash = passwordHash;
            UserType = userType;
            BannedUntil = bannedUntil;
            IsBanned = isBanned;
            FailedLogIns = failedLogins;
            Token = string.Empty;
            Password = string.Empty;
            SellingItems = new List<AuctionProduct>();
            Bids = new List<Bid>();
        }
    }
}