using Microsoft.EntityFrameworkCore;
using MarketMinds.Shared.Models;
using Server.DataModels;

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

        // public DbSet<Basket> Baskets { get; set; }
        // public DbSet<BasketItem> BasketItems { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        // merge-nicusor
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<BuyerLinkageEntity> BuyerLinkages { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<OrderSummary> OrderSummaries { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<OrderCheckpoint> OrderCheckpoints { get; set; }
        public DbSet<TrackedOrder> TrackedOrders { get; set; }
        public DbSet<UserWaitList> UserWaitLists { get; set; }
        public DbSet<DummyCardEntity> DummyCards { get; set; } // TODO change to Cards
        public DbSet<DummyWalletEntity> DummyWallets { get; set; } // TODO change to Wallets
        public DbSet<BuyerCartItemEntity> BuyerCartItems { get; set; }
        public DbSet<PDF> PDFs { get; set; }
        public DbSet<PredefinedContract> PredefinedContracts { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<SellerNotificationEntity> SellerNotifications { get; set; }
        public DbSet<FollowingEntity> Followings { get; set; }
        public DbSet<OrderNotificationEntity> OrderNotifications { get; set; }
        

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

            // --- Buyer Configuration --- merge-nicusor
            modelBuilder.Entity<Buyer>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.HasOne(b => b.User)
                    .WithOne()
                    .HasForeignKey<Buyer>(b => b.Id);

                entity.Property(b => b.Discount)
                    .HasPrecision(18, 2);

                entity.Property(b => b.TotalSpending)
                    .HasPrecision(18, 2);

                entity.HasOne(b => b.BillingAddress)
                    .WithMany()
                    .HasForeignKey("BillingAddressId")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.ShippingAddress)
                    .WithMany()
                    .HasForeignKey("ShippingAddressId")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Ignore(b => b.SyncedBuyerIds) // it creates a foreign key from Buyer to Buyer in the database (not needed)
                    .Ignore(b => b.FollowingUsersIds) // this should be a table of its own (see the FollowingEntity class)
                    .Ignore(b => b.Email) // not needed, it is in User
                    .Ignore(b => b.PhoneNumber); // not needed, it is in User
            });

            // --- Seller Configuration --- merge-nicusor
            modelBuilder.Entity<Seller>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.HasOne(s => s.User)
                    .WithOne()
                    .HasForeignKey<Seller>(s => s.Id);

                entity.Ignore(s => s.Email) // not needed, it is in User
                    .Ignore(s => s.PhoneNumber); // not needed, it is in User
            });

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
            // modelBuilder.Entity<Basket>().ToTable("Baskets");
            // modelBuilder.Entity<Basket>().HasKey(basket => basket.Id);

            // Explicitly ignore the Items collection
            // modelBuilder.Entity<Basket>()
            //     .Ignore(basket => basket.Items);

            // modelBuilder.Entity<BasketItem>().ToTable("BasketItemsBuyProducts");
            // modelBuilder.Entity<BasketItem>().HasKey(basketItem => basketItem.Id);
            // modelBuilder.Entity<BasketItem>()
            //     .HasOne<BuyProduct>()
            //     .WithMany()
            //     .HasForeignKey(basketItem => basketItem.ProductId);

            // modelBuilder.Entity<Order>().ToTable("Orders");
            // modelBuilder.Entity<Order>().HasKey(order => order.Id);
            // modelBuilder.Entity<Order>().Property(order => order.Name).IsRequired().HasMaxLength(100);
            // modelBuilder.Entity<Order>().Property(order => order.Description).IsRequired(false);
            // modelBuilder.Entity<Order>().Property(order => order.Cost).IsRequired();
            // modelBuilder.Entity<Order>().Property(order => order.SellerId).IsRequired();
            // modelBuilder.Entity<Order>().Property(order => order.BuyerId).IsRequired();

            // modelBuilder.Entity<Order>()
            //     .HasOne(order => order.Seller)
            //     .WithMany()
            //     .HasForeignKey(order => order.SellerId)
            //     .OnDelete(DeleteBehavior.Restrict);

            // --- Order Configuration --- merge-nicusor
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderID);

                entity.HasOne<BuyProduct>()
                    .WithMany()
                    .HasForeignKey(o => o.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(o => o.BuyerID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<OrderSummary>()
                    .WithMany()
                    .HasForeignKey(o => o.OrderSummaryID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<OrderHistory>()
                    .WithMany()
                    .HasForeignKey(o => o.OrderHistoryID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint("PaymentMethodConstraint", "[PaymentMethod] IN ('card', 'wallet', 'cash')"));
            });

            // --- Order Summary Configuration --- merge-nicusor
            modelBuilder.Entity<OrderSummary>(entity =>
            {
                entity.HasKey(os => os.ID);
            });

            // --- Order History Configuration --- merge-nicusor
            modelBuilder.Entity<OrderHistory>(entity =>
            {
                entity.HasKey(oh => oh.OrderID);
            });

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

            // --- PDF Configuration --- merge-nicusor
            modelBuilder.Entity<PDF>(entity =>
            {
                entity.HasKey(p => p.PdfID);
                entity.Ignore(p => p.ContractID); // ignored to respect Maria's DB design (but not deleted to avoid breaking changes)
            });

            // --- Predefined Contract Configuration --- merge-nicusor
            modelBuilder.Entity<PredefinedContract>(entity =>
            {
                entity.HasKey(pc => pc.ContractID);
                entity.Ignore(pc => pc.ID); // ignored to respect Maria's DB design (but not deleted to avoid breaking changes)
            });

            // --- Contract Configuration --- merge-nicusor
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(c => c.ContractID);

                entity.HasOne<Order>()
                    .WithMany()
                    .HasForeignKey(c => c.OrderID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<PredefinedContract>()
                    .WithMany()
                    .HasForeignKey(pc => pc.PredefinedContractID)
                    .IsRequired(false) // nullable to respect Maria's DB design
                    .OnDelete(DeleteBehavior.SetNull); // SetNull because it is Nullable

                entity.HasOne<PDF>()
                    .WithMany()
                    .HasForeignKey(p => p.PDFID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint("ContractStatusConstraint", "[ContractStatus] IN ('ACTIVE', 'RENEWED', 'EXPIRED')"));
            });

            // --- Tracked Order Configuration --- merge-nicusor
            modelBuilder.Entity<TrackedOrder>(entity =>
            {
                entity.HasKey(to => to.TrackedOrderID);

                entity.HasIndex(to => to.OrderID)
                    .IsUnique(); // to respect Maria's DB design

                entity.HasOne<Order>()
                    .WithMany()
                    .HasForeignKey(to => to.OrderID)
                    .OnDelete(DeleteBehavior.Restrict) // set to on delete cascade by Maria, I put restrict to avoid breaking changes (can be changed later)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(to => to.EstimatedDeliveryDate)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(to => to.DeliveryAddress)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(to => to.CurrentStatus)
                    .IsRequired(); // to respect Maria's DB design

                entity.ToTable(t => t.HasCheckConstraint("TrackedOrderConstraint", "[CurrentStatus] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')")); // to respect Maria's DB design
            });

            // --- Order Checkpoint Configuration --- merge-nicusor
            modelBuilder.Entity<OrderCheckpoint>(entity =>
            {
                entity.HasKey(oc => oc.CheckpointID);

                entity.HasOne<TrackedOrder>()
                    .WithMany()
                    .HasForeignKey(oc => oc.TrackedOrderID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(oc => oc.Timestamp)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(oc => oc.Description)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(oc => oc.Status)
                    .IsRequired(); // to respect Maria's DB design

                entity.ToTable(oc => oc.HasCheckConstraint("OrderChekpointConstraint", "[Status] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')")); // to respect Maria's DB design
            });

            // --- User Waitlist Configuration --- merge-nicusor
            modelBuilder.Entity<UserWaitList>(entity =>
            {
                entity.HasKey(uw => uw.UserWaitListID);

                entity.HasOne<BorrowProduct>()
                    .WithMany()
                    .HasForeignKey(borrowProduct => borrowProduct.Id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(); // to respect Maria's DB design

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(uw => uw.UserID)
                    .OnDelete(DeleteBehavior.Restrict) // set to on delete cascade by Maria, I put restrict to avoid breaking changes (can be changed later)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(uw => uw.JoinedTime)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(uw => uw.PositionInQueue)
                    .IsRequired(); // to respect Maria's DB design
            });

            // --- Dummy Card Configuration --- merge-nicusor
            // Please do check the DummyCardEntity class for more information about this abomination of a table :))
            // TODO: Change DummyCardEntity to CardEntity
            modelBuilder.Entity<DummyCardEntity>(entity =>
            {
                entity.HasKey(dc => dc.ID);

                entity.HasOne<Buyer>() // not in Maria's DB design, I added it to have code maintainability
                    .WithMany()
                    .HasForeignKey(dc => dc.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict) // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)
                    .IsRequired();

                entity.HasIndex(dc => dc.CardNumber); // because Golubiro Spioniro is stealing our cards :))

                entity.Property(dc => dc.CardholderName)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(dc => dc.CardNumber)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(dc => dc.CVC)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(dc => dc.Month)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(dc => dc.Year)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(dc => dc.Country)
                    .IsRequired(); // to respect Maria's DB design
            });

            // --- Dummy Wallet Configuration --- merge-nicusor
            // Please do check the DummyWalletEntity class for more information about this abomination of a table :))
            // TODO: Change DummyWalletEntity to WalletEntity
            modelBuilder.Entity<DummyWalletEntity>(entity =>
            {
                entity.HasOne<Buyer>()
                    .WithOne()
                    .HasForeignKey<DummyWalletEntity>(dw => dw.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict); // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)

                entity.HasKey(dw => dw.ID);
            });

            // --- Buyer Cart Item Configuration --- merge-nicusor
            modelBuilder.Entity<BuyerCartItemEntity>(entity =>
            {
                entity.HasKey(bci => new { bci.BuyerId, bci.ProductId });

                entity.Property(bci => bci.Quantity)
                    .HasDefaultValue(1); // to respect Maria's DB design

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(bci => bci.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict) // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)
                    .IsRequired();

                entity.HasOne<BuyProduct>()
                    .WithMany()
                    .HasForeignKey(bci => bci.Id)
                    .OnDelete(DeleteBehavior.Restrict) // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)
                    .IsRequired();
            });

            // --- Buyer Wishlist Items Configuration --- merge-nicusor
            modelBuilder.Entity<BuyerWishlistItemsEntity>(entity =>
            {
                entity.HasKey(bwi => new { bwi.BuyerId, bwi.ProductId });

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(bwi => bwi.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<BuyProduct>()
                    .WithMany()
                    .HasForeignKey(bwi => bwi.Id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Buyer Linkage Entity Configuration --- merge-nicusor
            modelBuilder.Entity<BuyerLinkageEntity>(entity =>
            {
                entity.HasKey(bl => new { bl.RequestingBuyerId, bl.ReceivingBuyerId });

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(bl => bl.RequestingBuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(bl => bl.ReceivingBuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint("CK_BuyerLinkage_DifferentBuyers", "[RequestingBuyerId] <> [ReceivingBuyerId]"));
            });

            // --- Following Entity Configuration --- merge-nicusor
            modelBuilder.Entity<FollowingEntity>(entity =>
            {
                entity.HasKey(f => new { f.BuyerId, f.SellerId });

                entity.HasOne<Buyer>()
                .WithMany()
                .HasForeignKey(f => f.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Seller>()
                .WithMany()
                .HasForeignKey(f => f.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Seller Notification Entity Configuration --- merge-nicusor
             modelBuilder.Entity<SellerNotificationEntity>(entity =>
            {
                entity.HasKey(sn => sn.NotificationID);

                entity.HasOne<Seller>()
                .WithMany()
                .HasForeignKey(sn => sn.SellerID)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Order Notification Entity Configuration ---
            modelBuilder.Entity<OrderNotificationEntity>(entity =>
            {
                entity.HasKey(on => on.NotificationID);

                entity.Property(on => on.Category)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(on => on.Timestamp)
                    .IsRequired(); // to respect Maria's DB design

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(on => on.RecipientID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(); // to respect Maria's DB design

                entity.HasOne<Order>()
                    .WithMany()
                    .HasForeignKey(on => on.OrderID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // to respect Maria's DB design

                entity.HasOne<BuyProduct>()
                    .WithMany()
                    .HasForeignKey(on => on.Id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // to respect Maria's DB design

                entity.HasOne<Contract>()
                    .WithMany()
                    .HasForeignKey(on => on.ContractID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // to respect Maria's DB design

                entity.ToTable(t => t.HasCheckConstraint("NotificationCategoryConstraint", "[Category] IN ('CONTRACT_EXPIRATION', 'OUTBIDDED', 'ORDER_SHIPPING_PROGRESS', 'PRODUCT_AVAILABLE', 'PAYMENT_CONFIRMATION', 'PRODUCT_REMOVED', 'CONTRACT_RENEWAL_REQ', 'CONTRACT_RENEWAL_ANS', 'CONTRACT_RENEWAL_WAITLIST')"));
            });

            // IGNORING SHIT - merge-nicusor
            modelBuilder.Ignore<BuyerWishlist>();
            modelBuilder.Ignore<BuyerWishlistItem>();
            modelBuilder.Ignore<BuyerLinkage>();
            modelBuilder.Ignore<Notification>();
            modelBuilder.Ignore<ContractRenewalAnswerNotification>();
            modelBuilder.Ignore<ContractRenewalWaitlistNotification>();
            modelBuilder.Ignore<OutbiddedNotification>();
            modelBuilder.Ignore<OrderShippingProgressNotification>();
            modelBuilder.Ignore<PaymentConfirmationNotification>();
            modelBuilder.Ignore<ProductRemovedNotification>();
            modelBuilder.Ignore<ProductAvailableNotification>();
            modelBuilder.Ignore<ContractRenewalRequestNotification>();
            modelBuilder.Ignore<ContractExpirationNotification>();

        }
    }
}