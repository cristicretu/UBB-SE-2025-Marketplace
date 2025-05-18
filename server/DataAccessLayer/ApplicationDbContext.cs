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
        public DbSet<OrderSummary> OrderSummary { get; set; }
        public DbSet<OrderHistory> OrderHistory { get; set; }
        public DbSet<OrderCheckpoint> OrderCheckpoints { get; set; }
        public DbSet<TrackedOrder> TrackedOrders { get; set; }
        public DbSet<UserWaitList> UserWaitList { get; set; }
        public DbSet<DummyCardEntity> DummyCards { get; set; } // TODO change to Cards
        public DbSet<DummyWalletEntity> DummyWallets { get; set; } // TODO change to Wallets
        public DbSet<BuyerCartItemEntity> BuyerCartItems { get; set; }
        public DbSet<BuyerWishlistItemsEntity> BuyersWishlistItems { get; set; }
        public DbSet<PDF> PDFs { get; set; }
        public DbSet<PredefinedContract> PredefinedContracts { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<SellerNotificationEntity> SellerNotifications { get; set; }
        public DbSet<FollowingEntity> Followings { get; set; }
        public DbSet<OrderNotificationEntity> OrderNotifications { get; set; }
        public DbSet<Address> Addresses { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User Configuration ---
            modelBuilder.Entity<User>(entity => 
            {
                entity.ToTable("Users");
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Email).IsRequired();
                entity.Property(user => user.Username).IsRequired();
                entity.HasIndex(user => user.Email).IsUnique();
                entity.HasIndex(user => user.Username).IsUnique();
            });

            // --- Buyer Configuration --- merge-nicusor
            modelBuilder.Entity<Buyer>(entity =>
            {
                entity.HasKey(buyer => buyer.Id);

                entity.HasOne(buyer => buyer.User)
                    .WithOne()
                    .HasForeignKey<Buyer>(buyer => buyer.Id);

                entity.Property(buyer => buyer.Discount)
                    .HasPrecision(18, 2);

                entity.Property(buyer => buyer.TotalSpending)
                    .HasPrecision(18, 2);

                entity.HasOne(buyer => buyer.BillingAddress)
                    .WithMany()
                    .HasForeignKey("BillingAddressId")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(buyer => buyer.ShippingAddress)
                    .WithMany()
                    .HasForeignKey("ShippingAddressId")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Ignore(buyer => buyer.SyncedBuyerIds) // it creates a foreign key from Buyer to Buyer in the database (not needed)
                    .Ignore(buyer => buyer.FollowingUsersIds) // this should be a table of its own (see the FollowingEntity class)
                    .Ignore(buyer => buyer.Email) // not needed, it is in User
                    .Ignore(buyer => buyer.PhoneNumber); // not needed, it is in User
            });

            // --- Seller Configuration --- merge-nicusor
            modelBuilder.Entity<Seller>(entity =>
            {
                entity.HasKey(seller => seller.Id);

                entity.HasOne(seller => seller.User)
                    .WithOne()
                    .HasForeignKey<Seller>(seller => seller.Id);

                entity.Ignore(seller => seller.Email) // not needed, it is in User
                    .Ignore(seller => seller.PhoneNumber); // not needed, it is in User
            });

            // --- Review Configuration ---
            modelBuilder.Entity<Review>(entity => 
            {
                entity.ToTable("Reviews");
                entity.HasKey(review => review.Id);
            });

            // --- ReviewImage Configuration ---
            modelBuilder.Entity<ReviewImage>(entity => 
            {
                entity.ToTable("ReviewsPictures");
                entity.HasKey(reviewPicture => reviewPicture.Id);
            });

            // --- ProductTag Configuration ---
            modelBuilder.Entity<ProductTag>(entity => 
            {
                entity.ToTable("ProductTags");
                entity.HasKey(productTag => productTag.Id);
                entity.Property(productTag => productTag.Title).IsRequired().HasColumnName("title");
                entity.HasIndex(productTag => productTag.Title).IsUnique();
                entity.Ignore("BuyProductId");
                entity.Ignore("BuyProductId1");
                entity.Ignore("BuyProducts");
            });

            // --- Condition Configuration ---
            modelBuilder.Entity<Condition>(entity => 
            {
                entity.ToTable("ProductConditions");
                entity.HasKey(productCondition => productCondition.Id);
                entity.HasIndex(productCondition => productCondition.Name).IsUnique();
            });

            // --- Category Configuration ---
            modelBuilder.Entity<Category>(entity => 
            {
                entity.ToTable("ProductCategories");
                entity.HasKey(productCondition => productCondition.Id);
                entity.HasIndex(productCondition => productCondition.Name).IsUnique();
            });

            // --- AuctionProduct Configuration ---
            modelBuilder.Entity<AuctionProduct>(entity => 
            {
                entity.ToTable("AuctionProducts");
                entity.HasKey(auctionProducts => auctionProducts.Id);
                
                // Configure relationship with Seller instead of User
                entity.HasOne<Seller>()
                    .WithMany(s => s.AuctionProducts)
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Modify any existing relationship that might refer to User
                entity.Ignore(p => p.Seller); // Ignore the User navigation property since we'll use Seller
            });

            // --- ProductImage Configuration ---
            modelBuilder.Entity<ProductImage>(entity => 
            {
                entity.ToTable("AuctionProductsImages");
                entity.HasKey(image => image.Id);
            });

            // --- AuctionProductProductTag Configuration ---
            modelBuilder.Entity<AuctionProductProductTag>(entity => 
            {
                entity.ToTable("AuctionProductProductTags");
                entity.HasKey(productTag => productTag.Id);
            });

            // --- Bid Configuration ---
            modelBuilder.Entity<Bid>(entity => 
            {
                entity.ToTable("Bids");
                entity.HasKey(bid => bid.Id);
                
                // Change relationship from User to Buyer
                entity.HasOne<Buyer>()
                    .WithMany(buyer => buyer.Bids)
                    .HasForeignKey(b => b.BidderId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Ignore the User-based bidder property
                entity.Ignore(b => b.Bidder);
            });

            // --- BuyProduct Configuration ---
            modelBuilder.Entity<BuyProduct>(entity => 
            {
                entity.ToTable("BuyProducts");
                entity.HasKey(buyProduct => buyProduct.Id);
                entity.Ignore(buyProduct => buyProduct.Tags)
                    .Ignore(buyProduct => buyProduct.NonMappedImages);
                
                // Configure relationship with Seller instead of User
                entity.HasOne<Seller>()
                    .WithMany(s => s.BuyProducts)
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Modify any existing relationship that might refer to User
                entity.Ignore(p => p.Seller); // Ignore the User navigation property since we'll use Seller
            });

            // --- BuyProductImage Configuration ---
            modelBuilder.Entity<BuyProductImage>(entity => 
            {
                entity.ToTable("BuyProductImages");
                entity.HasKey(image => image.Id);
                entity.Property(image => image.ProductId).HasColumnName("product_id");
                entity.HasOne(image => image.Product)
                    .WithMany(product => product.Images)
                    .HasForeignKey(image => image.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- BuyProductProductTag Configuration ---
            modelBuilder.Entity<BuyProductProductTag>(entity => 
            {
                entity.ToTable("BuyProductProductTags");
                entity.HasKey(productTag => productTag.Id);
                entity.Property(productTag => productTag.ProductId).HasColumnName("product_id");
                entity.Property(productTag => productTag.TagId).HasColumnName("tag_id");
                entity.HasOne(productTag => productTag.Product)
                    .WithMany(product => product.ProductTags)
                    .HasForeignKey(productTag => productTag.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(productTag => productTag.Tag)
                    .WithMany()
                    .HasForeignKey(productTag => productTag.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- BorrowProduct Configuration ---
            modelBuilder.Entity<BorrowProduct>(entity => 
            {
                entity.ToTable("BorrowProducts");
                entity.HasKey(borrowProduct => borrowProduct.Id);
                entity.Property(borrowProduct => borrowProduct.DailyRate).HasColumnName("daily_rate");
                entity.Property(borrowProduct => borrowProduct.TimeLimit).HasColumnName("time_limit");
                entity.Property(borrowProduct => borrowProduct.StartDate).HasColumnName("start_date");
                entity.Property(borrowProduct => borrowProduct.EndDate).HasColumnName("end_date");
                entity.Property(borrowProduct => borrowProduct.IsBorrowed).HasColumnName("is_borrowed");
                
                // Configure relationship with Seller instead of User
                entity.HasOne<Seller>()
                    .WithMany(s => s.BorrowProducts)
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Modify any existing relationship that might refer to User
                entity.Ignore(p => p.Seller); // Ignore the User navigation property since we'll use Seller
            });

            // --- BorrowProductImage Configuration ---
            modelBuilder.Entity<BorrowProductImage>(entity => 
            {
                entity.ToTable("BorrowProductImages");
                entity.HasKey(image => image.Id);
                entity.Property(image => image.ProductId).HasColumnName("product_id");
                entity.HasOne(image => image.Product)
                    .WithMany(product => product.Images)
                    .HasForeignKey(image => image.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- BorrowProductProductTag Configuration ---
            modelBuilder.Entity<BorrowProductProductTag>(entity => 
            {
                entity.ToTable("BorrowProductProductTags");
                entity.HasKey(productTag => productTag.Id);
                entity.Property(productTag => productTag.ProductId).HasColumnName("product_id");
                entity.Property(productTag => productTag.TagId).HasColumnName("tag_id");
                entity.HasOne(productTag => productTag.Product)
                    .WithMany(product => product.ProductTags)
                    .HasForeignKey(productTag => productTag.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(productTag => productTag.Tag)
                    .WithMany()
                    .HasForeignKey(productTag => productTag.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Order Configuration --- merge-nicusor
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasOne<BuyProduct>()
                    .WithMany()
                    .HasPrincipalKey(bp => bp.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(order => order.BuyerId)
                    .HasPrincipalKey(buyer => buyer.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Seller>()
                    .WithMany()
                    .HasForeignKey(order => order.SellerId)
                    .HasPrincipalKey(seller => seller.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<OrderSummary>()
                    .WithMany()
                    .HasForeignKey(order => order.OrderSummaryID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<OrderHistory>()
                    .WithMany()
                    .HasForeignKey(order => order.OrderHistoryID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint("PaymentMethodConstraint", "[PaymentMethod] IN ('card', 'wallet', 'cash')"));
                
                entity.Property(o => o.BuyerId).HasColumnName("BuyerID");
            });

            // --- OrderSummary Configuration --- merge-nicusor
            modelBuilder.Entity<OrderSummary>(entity =>
            {
                entity.HasKey(OrderSummary => OrderSummary.ID);
            });

            // --- OrderHistory Configuration --- merge-nicusor
            modelBuilder.Entity<OrderHistory>(entity =>
            {
                entity.HasKey(OrderHistory => OrderHistory.OrderID);
            });

            // --- Conversation Configuration ---
            modelBuilder.Entity<Conversation>(entity => 
            {
                entity.ToTable("Conversations");
                entity.HasKey(conversation => conversation.Id);
            });

            // --- Message Configuration ---
            modelBuilder.Entity<Message>(entity => 
            {
                entity.ToTable("Messages");
                entity.HasKey(message => message.Id);
                entity.HasOne(message => message.User)
                    .WithMany()
                    .HasForeignKey(message => message.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- PDF Configuration --- merge-nicusor
            modelBuilder.Entity<PDF>(entity =>
            {
                entity.HasKey(pdf => pdf.PdfID);
                entity.Ignore(pdf => pdf.ContractID); // ignored to respect Maria's DB design (but not deleted to avoid breaking changes)
            });

            // --- PredefinedContract Configuration --- merge-nicusor
            modelBuilder.Entity<PredefinedContract>(entity =>
            {
                entity.HasKey(predefinedContract => predefinedContract.ContractID);
                entity.Ignore(predefinedContract => predefinedContract.ID); // ignored to respect Maria's DB design (but not deleted to avoid breaking changes)
            });

            // --- Contract Configuration --- merge-nicusor
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(contract => contract.ContractID);

                entity.HasOne<Order>()
                    .WithMany()
                    .HasForeignKey(contract => contract.OrderID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<PredefinedContract>()
                    .WithMany()
                    .HasForeignKey(predefinedContract => predefinedContract.PredefinedContractID)
                    .IsRequired(false) // nullable to respect Maria's DB design
                    .OnDelete(DeleteBehavior.SetNull); // SetNull because it is Nullable

                entity.HasOne<PDF>()
                    .WithMany()
                    .HasForeignKey(pdf => pdf.PDFID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint("ContractStatusConstraint", "[ContractStatus] IN ('ACTIVE', 'RENEWED', 'EXPIRED')"));
            });

            // --- TrackedOrder Configuration --- merge-nicusor
            modelBuilder.Entity<TrackedOrder>(entity =>
            {
                entity.HasKey(trackedOrder => trackedOrder.TrackedOrderID);

                entity.HasIndex(trackedOrder => trackedOrder.OrderID)
                    .IsUnique(); // to respect Maria's DB design

                entity.HasOne<Order>()
                    .WithMany()
                    .HasForeignKey(trackedOrder => trackedOrder.OrderID)
                    .OnDelete(DeleteBehavior.Restrict) // set to on delete cascade by Maria, I put restrict to avoid breaking changes (can be changed later)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(trackedOrder => trackedOrder.EstimatedDeliveryDate)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(trackedOrder => trackedOrder.DeliveryAddress)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(trackedOrder => trackedOrder.CurrentStatus)
                    .IsRequired(); // to respect Maria's DB design

                entity.ToTable(t => t.HasCheckConstraint("TrackedOrderConstraint", "[CurrentStatus] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')")); // to respect Maria's DB design
            });

            // --- OrderCheckpoint Configuration --- merge-nicusor
            modelBuilder.Entity<OrderCheckpoint>(entity =>
            {
                entity.HasKey(orderCheckpoint => orderCheckpoint.CheckpointID);

                entity.HasOne<TrackedOrder>()
                    .WithMany()
                    .HasForeignKey(orderCheckpoint => orderCheckpoint.TrackedOrderID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(orderCheckpoint => orderCheckpoint.Timestamp)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(orderCheckpoint => orderCheckpoint.Description)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(orderCheckpoint => orderCheckpoint.Status)
                    .IsRequired(); // to respect Maria's DB design

                entity.ToTable(orderCheckpoint => orderCheckpoint.HasCheckConstraint("OrderChekpointConstraint", "[Status] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')")); // to respect Maria's DB design
            });

            // --- UserWaitList Configuration --- merge-nicusor
            modelBuilder.Entity<UserWaitList>(entity =>
            {
                entity.HasKey(userWaitList => userWaitList.UserWaitListID);

                entity.HasOne<BorrowProduct>()
                    .WithMany()
                    .HasForeignKey(userWaitList => userWaitList.ProductWaitListID)
                    .HasPrincipalKey(bp => bp.Id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(); // to respect Maria's DB design

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(userWaitList => userWaitList.UserID)
                    .OnDelete(DeleteBehavior.Restrict) // set to on delete cascade by Maria, I put restrict to avoid breaking changes (can be changed later)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(userWaitList => userWaitList.JoinedTime)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(userWaitList => userWaitList.PositionInQueue)
                    .IsRequired(); // to respect Maria's DB design
            });

            // --- DummyCard Configuration --- merge-nicusor
            modelBuilder.Entity<DummyCardEntity>(entity =>
            {
                entity.HasKey(dummyCard => dummyCard.ID);

                entity.HasOne<Buyer>() // not in Maria's DB design, I added it to have code maintainability
                    .WithMany()
                    .HasForeignKey(dummyCard => dummyCard.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict) // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)
                    .IsRequired();

                entity.HasIndex(dummyCard => dummyCard.CardNumber); // because Golubiro Spioniro is stealing our cards :))

                entity.Property(dummyCard => dummyCard.CardholderName)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(dummyCard => dummyCard.CardNumber)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(dummyCard => dummyCard.CVC)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(dummyCard => dummyCard.Month)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(dummyCard => dummyCard.Year)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(dummyCard => dummyCard.Country)
                    .IsRequired(); // to respect Maria's DB design
            });

            // --- DummyWallet Configuration --- merge-nicusor
            modelBuilder.Entity<DummyWalletEntity>(entity =>
            {
                entity.HasOne<Buyer>()
                    .WithOne()
                    .HasForeignKey<DummyWalletEntity>(dw => dw.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict); // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)

                entity.HasKey(dummyWallet => dummyWallet.ID);
            });

            // --- BuyerCartItem Configuration --- merge-nicusor
            modelBuilder.Entity<BuyerCartItemEntity>(entity =>
            {
                entity.HasKey(buyerCartItem => new { buyerCartItem.BuyerId, buyerCartItem.ProductId });

                entity.Property(buyerCartItem => buyerCartItem.Quantity)
                    .HasDefaultValue(1); // to respect Maria's DB design

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(buyerCartItem => buyerCartItem.BuyerId)
                    .HasPrincipalKey(b => b.Id)
                    .OnDelete(DeleteBehavior.Restrict) // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)
                    .IsRequired();

                entity.HasOne<BuyProduct>()
                    .WithMany()
                    .HasPrincipalKey(bp => bp.Id)
                    .OnDelete(DeleteBehavior.Restrict) // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)
                    .IsRequired();
            });

            // --- BuyerWishlistItems Configuration --- merge-nicusor
            modelBuilder.Entity<BuyerWishlistItemsEntity>(entity =>
            {
                entity.HasKey(buyerWishlistItem => new { buyerWishlistItem.BuyerId, buyerWishlistItem.ProductId });

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(buyerWishlistItem => buyerWishlistItem.BuyerId)
                    .HasPrincipalKey(b => b.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<BuyProduct>()
                    .WithMany()
                    .HasPrincipalKey(bp => bp.Id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- BuyerLinkage Configuration --- merge-nicusor
            modelBuilder.Entity<BuyerLinkageEntity>(entity =>
            {
                entity.HasKey(buyerLinkage => new { buyerLinkage.RequestingBuyerId, buyerLinkage.ReceivingBuyerId });

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(buyerLinkage => buyerLinkage.RequestingBuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(buyerLinkage => buyerLinkage.ReceivingBuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(t => t.HasCheckConstraint("CK_BuyerLinkage_DifferentBuyers", "[RequestingBuyerId] <> [ReceivingBuyerId]"));
            });

            // --- Following Configuration --- merge-nicusor
            modelBuilder.Entity<FollowingEntity>(entity =>
            {
                entity.HasKey(following => new { following.BuyerId, following.SellerId });

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(following => following.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Seller>()
                    .WithMany()
                    .HasForeignKey(following => following.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- SellerNotification Configuration --- merge-nicusor
             modelBuilder.Entity<SellerNotificationEntity>(entity =>
            {
                entity.HasKey(sellerNotification => sellerNotification.NotificationID);

                entity.HasOne<Seller>()
                    .WithMany()
                    .HasForeignKey(sellerNotification => sellerNotification.SellerID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- OrderNotification Configuration --- merge-nicusor
            modelBuilder.Entity<OrderNotificationEntity>(entity =>
            {
                entity.HasKey(orderNotification => orderNotification.NotificationID);

                entity.Property(orderNotification => orderNotification.Category)
                    .IsRequired(); // to respect Maria's DB design

                entity.Property(orderNotification => orderNotification.Timestamp)
                    .IsRequired(); // to respect Maria's DB design

                entity.HasOne<Buyer>()
                    .WithMany()
                    .HasForeignKey(orderNotification => orderNotification.RecipientID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(); // to respect Maria's DB design

                entity.HasOne<Order>()
                    .WithMany()
                    .HasForeignKey(orderNotification => orderNotification.OrderID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // to respect Maria's DB design

                entity.HasOne<BuyProduct>()
                    .WithMany()
                    .HasForeignKey(orderNotification => orderNotification.ProductID)
                    .HasPrincipalKey(bp => bp.Id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // to respect Maria's DB design

                entity.HasOne<Contract>()
                    .WithMany()
                    .HasForeignKey(orderNotification => orderNotification.ContractID)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // to respect Maria's DB design

                entity.ToTable(t => t.HasCheckConstraint("NotificationCategoryConstraint", "[Category] IN ('CONTRACT_EXPIRATION', 'OUTBIDDED', 'ORDER_SHIPPING_PROGRESS', 'PRODUCT_AVAILABLE', 'PAYMENT_CONFIRMATION', 'PRODUCT_REMOVED', 'CONTRACT_RENEWAL_REQ', 'CONTRACT_RENEWAL_ANS', 'CONTRACT_RENEWAL_WAITLIST')"));
            });

            // --- Address Configuration ---
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(address => address.Id);
            });

            // IGNORING ENTITIES --- merge-nicusor
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