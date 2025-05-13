using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace MarketMinds.Shared.Models // Adjusted namespace to server.Models
{
    [Table("AuctionProducts")]
    public class AuctionProduct : Product
    {
        [Column("start_datetime")]
        [JsonPropertyName("startAuctionDate")]
        public DateTime StartTime { get; set; }

        [Column("end_datetime")]
        [JsonPropertyName("endAuctionDate")]
        public DateTime EndTime { get; set; }

        [Column("starting_price")]
        [JsonPropertyName("startingPrice")]
        public double StartPrice { get; set; }

        [NotMapped]
        [JsonIgnore]
        public double StartingPrice => StartPrice;

        [Column("current_price")]
        [JsonPropertyName("currentPrice")]
        public double CurrentPrice { get; set; }

        [JsonPropertyName("bidHistory")]
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();

        [NotMapped]
        [JsonIgnore]
        public IEnumerable<Bid> BidHistory => Bids?.OrderByDescending(b => b.Timestamp) ?? Enumerable.Empty<Bid>();

        [JsonPropertyName("images")]
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        [NotMapped]
        [JsonIgnore]
        public List<Image> NonMappedImages { get; set; } = new List<Image>();

        public AuctionProduct() : base()
        {
            NonMappedImages = new List<Image>();
            Bids = new List<Bid>();
            Images = new List<ProductImage>();
        }

        public AuctionProduct(string title, string? description, int sellerId, int? conditionId,
                           int? categoryId, DateTime startTime,
                           DateTime endTime, double startPrice) : base()
        {
            Title = title;
            Description = description ?? string.Empty;
            SellerId = sellerId;
            ConditionId = conditionId ?? 0;
            CategoryId = categoryId ?? 0;
            StartTime = startTime;
            EndTime = endTime;
            StartPrice = startPrice;
            CurrentPrice = startPrice;
            Bids = new List<Bid>();
            Images = new List<ProductImage>();
            NonMappedImages = new List<Image>();
        }

        public AuctionProduct(int id, string title, string description, User seller,
                             Condition condition, Category category, List<ProductTag> productTags,
                             List<Image> images, DateTime startTime, DateTime endTime, double startPrice) : base()
        {
            Id = id;
            Title = title;
            Description = description;
            Seller = seller;
            Condition = condition;
            Category = category;
            StartTime = startTime;
            EndTime = endTime;
            StartPrice = startPrice;
            CurrentPrice = startPrice;
            Tags = productTags ?? new List<ProductTag>();
            NonMappedImages = images ?? new List<Image>();
        }

        public void AddBid(Bid bid)
        {
            Bids.Add(bid);
            if (bid.Price > CurrentPrice)
            {
                CurrentPrice = bid.Price;
            }
        }
    }
}