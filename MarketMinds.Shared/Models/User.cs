using System;
using System.Collections.Generic;
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

        [Column("rating")]
        public double Rating { get; set; }


        [Column("failedLogIns")]
        public int FailedLogIns { get; set; }

        [Column("bannedUntil")]
        public DateTime? BannedUntil { get; set; }

        [Column("isBanned")]
        public bool IsBanned { get; set; }

        [NotMapped]
        public string Token { get; set; } = string.Empty;

        private const double MAX_BALANCE = 999999;

        // Navigation properties
        public ICollection<AuctionProduct> SellingItems { get; set; }
        public ICollection<Bid> Bids { get; set; }

        public User()
        {
            Username = string.Empty;
            Email = string.Empty;
            Token = string.Empty;
            Password = string.Empty;
            SellingItems = new List<AuctionProduct>();
            Bids = new List<Bid>();
        }

        public User(string username, string email, string passwordHash)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Token = string.Empty;
            Password = string.Empty;
            SellingItems = new List<AuctionProduct>();
            Bids = new List<Bid>();
        }

        public User(int id, string username, string email, string token)
        {
            Id = id;
            Username = username;
            Email = email;
            Token = token;
            Password = string.Empty;
            SellingItems = new List<AuctionProduct>();
            Bids = new List<Bid>();
        }
    }
}