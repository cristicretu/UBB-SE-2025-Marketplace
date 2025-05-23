using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    public class BorrowProduct : Product
    {
        public DateTime TimeLimit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double DailyRate { get; set; }
        public bool IsBorrowed { get; set; }

        // Override Price from base class and mark as NotMapped
        [NotMapped]
        public override double Price { get; set; }

        // [NotMapped]
        // public new int SellerId { get; set; }
        //
        // [NotMapped]
        // public new int? CategoryId { get; set; }
        //
        // [NotMapped]
        // public new int? ConditionId { get; set; }

        // Navigation properties
        public ICollection<BorrowProductImage> Images { get; set; } = new List<BorrowProductImage>();
        public ICollection<BorrowProductProductTag> ProductTags { get; set; } = new List<BorrowProductProductTag>();

        [NotMapped]
        public List<Image> NonMappedImages { get; set; } = new List<Image>();

        public BorrowProduct() : base()
        {
        }

        public BorrowProduct(int id, string title, string description, User seller,
            Condition condition, Category category, DateTime timeLimit,
            double dailyRate) : base()
        {
            Id = id;
            Title = title;
            Description = description;
            Seller = seller;
            Condition = condition;
            Category = category;
            TimeLimit = timeLimit;
            DailyRate = dailyRate;
            IsBorrowed = false;
        }

        public BorrowProduct(int id, string title, string description, User seller,
            Condition condition, Category category, List<ProductTag> productTags,
            List<Image> images, DateTime timeLimit, DateTime startDate, DateTime endDate,
            double dailyRate, bool isBorrowed) : base()
        {
            Id = id;
            Title = title;
            Description = description;
            Seller = seller;
            Condition = condition;
            Category = category;
            TimeLimit = timeLimit;
            StartDate = startDate;
            EndDate = endDate;
            DailyRate = dailyRate;
            IsBorrowed = isBorrowed;
            Tags = productTags ?? new List<ProductTag>();
            NonMappedImages = images ?? new List<Image>();
        }
    }
}
