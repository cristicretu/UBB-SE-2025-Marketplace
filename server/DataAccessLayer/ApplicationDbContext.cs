using Microsoft.EntityFrameworkCore;
using MarketMinds.Shared.Models;

namespace Server.DataAccessLayer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Condition> ProductConditions { get; set; }
        public DbSet<Category> ProductCategories { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }

        // Reviews
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewImage> ReviewImages { get; set; }

        // Auction products
        public DbSet<AuctionProduct> AuctionProducts { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<ProductImage> AuctionProductImages { get; set; }
        public DbSet<AuctionProductProductTag> AuctionProductProductTags { get; set; }

        // Buy products
        public DbSet<BuyProduct> BuyProducts { get; set; }
        public DbSet<BuyProductImage> BuyProductImages { get; set; }
        public DbSet<BuyProductProductTag> BuyProductProductTags { get; set; }

        // Borrow products
        public DbSet<BorrowProduct> BorrowProducts { get; set; }
        public DbSet<BorrowProductImage> BorrowProductImages { get; set; }
        public DbSet<BorrowProductProductTag> BorrowProductProductTags { get; set; }

        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity relationships and constraints
            // Users
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(user => user.Id);
            modelBuilder.Entity<User>().Property(user => user.Email).IsRequired();
            modelBuilder.Entity<User>().Property(user => user.Username).IsRequired();
            modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(user => user.Username).IsUnique();

            // Reviews
            modelBuilder.Entity<Review>().ToTable("Reviews");
            modelBuilder.Entity<Review>().HasKey(review => review.Id);

            modelBuilder.Entity<ReviewImage>().ToTable("ReviewsPictures");
            modelBuilder.Entity<ReviewImage>().HasKey(reviewPicture => reviewPicture.Id);

            // Product metadata
            modelBuilder.Entity<ProductTag>().ToTable("ProductTags");
            modelBuilder.Entity<ProductTag>().HasKey(productTag => productTag.Id);
            modelBuilder.Entity<ProductTag>().Property(productTag => productTag.Title).IsRequired().HasColumnName("title");
            modelBuilder.Entity<ProductTag>().HasIndex(productTag => productTag.Title).IsUnique();

            modelBuilder.Entity<ProductTag>().Ignore("BuyProductId");
            modelBuilder.Entity<ProductTag>().Ignore("BuyProductId1");
            modelBuilder.Entity<ProductTag>().Ignore("BuyProducts");

            modelBuilder.Entity<Condition>().ToTable("ProductConditions");
            modelBuilder.Entity<Condition>().HasKey(productCondition => productCondition.Id);
            modelBuilder.Entity<Condition>().HasIndex(productCondition => productCondition.Name).IsUnique();

            modelBuilder.Entity<Category>().ToTable("ProductCategories");
            modelBuilder.Entity<Category>().HasKey(productCondition => productCondition.Id);
            modelBuilder.Entity<Category>().HasIndex(productCondition => productCondition.Name).IsUnique();

            // Auction products
            modelBuilder.Entity<AuctionProduct>().ToTable("AuctionProducts");
            modelBuilder.Entity<AuctionProduct>().HasKey(auctionProducts => auctionProducts.Id);

            modelBuilder.Entity<ProductImage>().ToTable("AuctionProductsImages");
            modelBuilder.Entity<ProductImage>().HasKey(image => image.Id);

            modelBuilder.Entity<AuctionProductProductTag>().ToTable("AuctionProductProductTags");
            modelBuilder.Entity<AuctionProductProductTag>().HasKey(productTag => productTag.Id);

            modelBuilder.Entity<Bid>().ToTable("Bids");
            modelBuilder.Entity<Bid>().HasKey(bid => bid.Id);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Bidder)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.BidderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Buy products
            modelBuilder.Entity<BuyProduct>().ToTable("BuyProducts");
            modelBuilder.Entity<BuyProduct>().HasKey(buyProduct => buyProduct.Id);

            modelBuilder.Entity<BuyProduct>()
                .Ignore(buyProduct => buyProduct.Tags)
                .Ignore(buyProduct => buyProduct.NonMappedImages);

            modelBuilder.Entity<BuyProductImage>().ToTable("BuyProductImages");
            modelBuilder.Entity<BuyProductImage>().HasKey(image => image.Id);
            modelBuilder.Entity<BuyProductImage>().Property(image => image.ProductId).HasColumnName("product_id");

            // Configure one-to-many relationship between BuyProduct and BuyProductImage
            modelBuilder.Entity<BuyProductImage>()
                .HasOne(image => image.Product)
                .WithMany(product => product.Images)
                .HasForeignKey(image => image.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BuyProductProductTag>().ToTable("BuyProductProductTags");
            modelBuilder.Entity<BuyProductProductTag>().HasKey(productTag => productTag.Id);
            modelBuilder.Entity<BuyProductProductTag>().Property(productTag => productTag.ProductId).HasColumnName("product_id");
            modelBuilder.Entity<BuyProductProductTag>().Property(productTag => productTag.TagId).HasColumnName("tag_id");

            modelBuilder.Entity<BuyProductProductTag>()
                .HasOne(productTag => productTag.Product)
                .WithMany(product => product.ProductTags)
                .HasForeignKey(productTag => productTag.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BuyProductProductTag>()
                .HasOne(productTag => productTag.Tag)
                .WithMany()
                .HasForeignKey(productTag => productTag.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Borrow products
            modelBuilder.Entity<BorrowProduct>().ToTable("BorrowProducts");
            modelBuilder.Entity<BorrowProduct>().HasKey(borrowProduct => borrowProduct.Id);
            modelBuilder.Entity<BorrowProduct>().Property(borrowProduct => borrowProduct.DailyRate).HasColumnName("daily_rate");
            modelBuilder.Entity<BorrowProduct>().Property(borrowProduct => borrowProduct.TimeLimit).HasColumnName("time_limit");
            modelBuilder.Entity<BorrowProduct>().Property(borrowProduct => borrowProduct.StartDate).HasColumnName("start_date");
            modelBuilder.Entity<BorrowProduct>().Property(borrowProduct => borrowProduct.EndDate).HasColumnName("end_date");
            modelBuilder.Entity<BorrowProduct>().Property(borrowProduct => borrowProduct.IsBorrowed).HasColumnName("is_borrowed");

            modelBuilder.Entity<BorrowProductImage>().ToTable("BorrowProductImages");
            modelBuilder.Entity<BorrowProductImage>().HasKey(image => image.Id);
            modelBuilder.Entity<BorrowProductImage>().Property(image => image.ProductId).HasColumnName("product_id");

            // Configure one-to-many relationship between BorrowProduct and BorrowProductImage
            modelBuilder.Entity<BorrowProductImage>()
                .HasOne(image => image.Product)
                .WithMany(product => product.Images)
                .HasForeignKey(image => image.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BorrowProductProductTag>().ToTable("BorrowProductProductTags");
            modelBuilder.Entity<BorrowProductProductTag>().HasKey(productTag => productTag.Id);
            modelBuilder.Entity<BorrowProductProductTag>().Property(productTag => productTag.ProductId).HasColumnName("product_id");
            modelBuilder.Entity<BorrowProductProductTag>().Property(productTag => productTag.TagId).HasColumnName("tag_id");

            // Configure many-to-many relationship
            modelBuilder.Entity<BorrowProductProductTag>()
                .HasOne(productTag => productTag.Product)
                .WithMany(product => product.ProductTags)
                .HasForeignKey(productTag => productTag.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BorrowProductProductTag>()
                .HasOne(productTag => productTag.Tag)
                .WithMany()
                .HasForeignKey(productTag => productTag.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Basket
            modelBuilder.Entity<Basket>().ToTable("Baskets");
            modelBuilder.Entity<Basket>().HasKey(basket => basket.Id);

            // Explicitly ignore the Items collection
            modelBuilder.Entity<Basket>()
                .Ignore(basket => basket.Items);

            modelBuilder.Entity<BasketItem>().ToTable("BasketItemsBuyProducts");
            modelBuilder.Entity<BasketItem>().HasKey(basketItem => basketItem.Id);
            modelBuilder.Entity<BasketItem>()
                .HasOne<BuyProduct>()
                .WithMany()
                .HasForeignKey(basketItem => basketItem.ProductId);

            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<Order>().HasKey(order => order.Id);
            modelBuilder.Entity<Order>().Property(order => order.Name).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Order>().Property(order => order.Description).IsRequired(false);
            modelBuilder.Entity<Order>().Property(order => order.Cost).IsRequired();
            modelBuilder.Entity<Order>().Property(order => order.SellerId).IsRequired();
            modelBuilder.Entity<Order>().Property(order => order.BuyerId).IsRequired();

            modelBuilder.Entity<Order>()
                .HasOne(order => order.Seller)
                .WithMany()
                .HasForeignKey(order => order.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // modelBuilder.Entity<Order>()
            //     .HasOne(order => order.Buyer)
            //     .WithMany()
            //     .HasForeignKey(order => order.BuyerId)
            //     .OnDelete(DeleteBehavior.Restrict); // Would fail if BuyerId is -1
            // Conversations
            modelBuilder.Entity<Conversation>().ToTable("Conversations");
            modelBuilder.Entity<Conversation>().HasKey(conversation => conversation.Id);

            // Messages
            modelBuilder.Entity<Message>().ToTable("Messages");

            modelBuilder.Entity<Message>().HasKey(message => message.Id);

            modelBuilder.Entity<Message>()
                .HasOne(message => message.User)
                .WithMany()
                .HasForeignKey(message => message.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}