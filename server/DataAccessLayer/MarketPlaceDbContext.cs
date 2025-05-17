//// <copyright file="MarketPlaceDbContext.cs" company="PlaceholderCompany">
//// Copyright (c) PlaceholderCompany. All rights reserved.
//// </copyright>

//namespace Server.DBConnection
//{
//    using Microsoft.EntityFrameworkCore;
//    using Microsoft.EntityFrameworkCore.Diagnostics;
//    using Server.DataModels;

//    /// <summary>
//    /// Represents the database context for the MarketPlace application.
//    /// </summary>
//    public class MarketPlaceDbContext : DbContext
//    {
//        /// <summary>
//        /// Initializes a new instance of the <see cref="MarketPlaceDbContext"/> class.
//        /// </summary>
//        /// <param name="options">The options for the database context.</param>
//        public MarketPlaceDbContext(DbContextOptions<MarketPlaceDbContext> options)
//            : base(options)
//        {
//        }

//        /// <summary>
//        /// Gets or sets the users table.
//        /// </summary>
//        public DbSet<User> Users { get; set; }

//        /// <summary>
//        /// Gets or sets the buyers table.
//        /// </summary>
//        public DbSet<Buyer> Buyers { get; set; }

//        /// <summary>
//        /// Gets or sets the buyers wishlist items table.
//        /// </summary>
//        public DbSet<BuyerWishlistItemsEntity> BuyersWishlistItems { get; set; }

//        /// <summary>
//        /// Gets or sets the buyers linkages table.
//        /// We use BuyerLinkageEntity in orde to have the corresponding table created in the database
//        /// We do not change the BuyerLinkage class in order to avoid breaking changes.
//        /// </summary>
//        public DbSet<BuyerLinkageEntity> BuyerLinkages { get; set; }

//        /// <summary>
//        /// Gets or sets the addresses table.
//        /// </summary>
//        public DbSet<Address> Addresses { get; set; } // buyer's address

//        /// <summary>
//        /// Gets or sets the sellers table.
//        /// </summary>
//        public DbSet<Seller> Sellers { get; set; }

//        /// <summary>
//        /// Gets or sets the products table.
//        /// </summary>
//        public DbSet<Product> Products { get; set; }

//        /// <summary>
//        /// Gets or sets the followings table.
//        /// </summary>
//        public DbSet<FollowingEntity> Followings { get; set; }

//        /// <summary>
//        /// Gets or sets the seller notifications table.
//        /// </summary>
//        public DbSet<SellerNotificationEntity> SellerNotifications { get; set; }

//        /// <summary>
//        /// Gets or sets the order notifications table.
//        /// </summary>
//        public DbSet<OrderNotificationEntity> OrderNotifications { get; set; }

//        /// <summary>
//        /// Gets or sets the reviews table.
//        /// </summary>
//        public DbSet<Review> Reviews { get; set; }

//        /// <summary>
//        /// Gets or sets the order summaries table.
//        /// </summary>
//        public DbSet<OrderSummary> OrderSummary { get; set; }

//        /// <summary>
//        /// Gets or sets the order history table.
//        /// </summary>
//        public DbSet<OrderHistory> OrderHistory { get; set; }

//        /// <summary>
//        /// Gets or sets the orders table.
//        /// </summary>
//        public DbSet<Order> Orders { get; set; }

//        /// <summary>
//        /// Gets or sets the PDFs table.
//        /// </summary>
//        public DbSet<PDF> PDFs { get; set; }

//        /// <summary>
//        /// Gets or sets the predefined contracts table.
//        /// </summary>
//        public DbSet<PredefinedContract> PredefinedContracts { get; set; }

//        /// <summary>
//        /// Gets or sets the contracts table.
//        /// </summary>
//        public DbSet<Contract> Contracts { get; set; }

//        /// <summary>
//        /// Gets or sets the tracked orders table.
//        /// </summary>
//        public DbSet<TrackedOrder> TrackedOrders { get; set; }

//        /// <summary>
//        /// Gets or sets the order checkpoints table.
//        /// </summary>
//        public DbSet<OrderCheckpoint> OrderCheckpoints { get; set; }

//        /// <summary>
//        /// Gets or sets the waitlist products table.
//        /// </summary>
//        public DbSet<WaitlistProductEntity> WaitlistProducts { get; set; }

//        /// <summary>
//        /// Gets or sets the user waitlist table.
//        /// </summary>
//        public DbSet<UserWaitList> UserWaitList { get; set; }

//        /// <summary>
//        /// Gets or sets the dummy cards table.
//        /// </summary>
//        public DbSet<DummyCardEntity> DummyCards { get; set; }

//        /// <summary>
//        /// Gets or sets the dummy wallets table.
//        /// </summary>
//        public DbSet<DummyWalletEntity> DummyWallets { get; set; }

//        /// <summary>
//        /// Gets or sets the buyer cart items table.
//        /// </summary>
//        public DbSet<BuyerCartItemEntity> BuyerCartItems { get; set; }

//        /// <summary>
//        /// On model creating.
//        /// </summary>
//        /// <param name="modelBuilder">The model builder.</param>
//        /// 

  
//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            // --- Buyer Configuration ---
//            // modelBuilder.Entity<Buyer>(entity =>
//            // {
//            //     entity.HasKey(b => b.Id);

//            //     entity.HasOne(b => b.User)
//            //         .WithOne()
//            //         .HasForeignKey<Buyer>(b => b.Id);

//            //     entity.Property(b => b.Discount)
//            //         .HasPrecision(18, 2);

//            //     entity.Property(b => b.TotalSpending)
//            //         .HasPrecision(18, 2);

//            //     entity.HasOne(b => b.BillingAddress)
//            //         .WithMany()
//            //         .HasForeignKey("BillingAddressId")
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.HasOne(b => b.ShippingAddress)
//            //         .WithMany()
//            //         .HasForeignKey("ShippingAddressId")
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.Ignore(b => b.SyncedBuyerIds) // it creates a foreign key from Buyer to Buyer in the database (not needed)
//            //         .Ignore(b => b.FollowingUsersIds) // this should be a table of its own (see the FollowingEntity class)
//            //         .Ignore(b => b.Email) // not needed, it is in User
//            //         .Ignore(b => b.PhoneNumber); // not needed, it is in User
//            // });

//            // --- Buyer Wishlist Configuration ---
//            // We ignore the BuyerWishlist and BuyerWishlistItem classes in order to avoid having them created in the database because
//            // we use the BuyerWishlistItemsEntity instead for the sake of simplicity and to avoid having to change logic
//            // anywhere else apart from the repository layer
//            // modelBuilder.Ignore<BuyerWishlist>();
//            // modelBuilder.Ignore<BuyerWishlistItem>();

//            // modelBuilder.Entity<BuyerWishlistItemsEntity>(entity =>
//            // {
//            //     entity.HasKey(bwi => new { bwi.BuyerId, bwi.ProductId });

//            //     entity.HasOne<Buyer>()
//            //         .WithMany()
//            //         .HasForeignKey(bwi => bwi.BuyerId)
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.HasOne<Product>()
//            //         .WithMany()
//            //         .HasForeignKey(bwi => bwi.ProductId)
//            //         .OnDelete(DeleteBehavior.Restrict);
//            // });

//            // --- Buyer Linkage Configuration ---
//            // We ignore the BuyerLinkage class in order to avoid having it created in the database because
//            // we use the BuyerLinkageEntity instead to avoid having to change logic anywhere else apart from the repository layer
//            // modelBuilder.Ignore<BuyerLinkage>();

//            // // Buyer Linkage Entity
//            // modelBuilder.Entity<BuyerLinkageEntity>(entity =>
//            // {
//            //     entity.HasKey(bl => new { bl.RequestingBuyerId, bl.ReceivingBuyerId });

//            //     entity.HasOne<Buyer>()
//            //         .WithMany()
//            //         .HasForeignKey(bl => bl.RequestingBuyerId)
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.HasOne<Buyer>()
//            //         .WithMany()
//            //         .HasForeignKey(bl => bl.ReceivingBuyerId)
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.ToTable(t => t.HasCheckConstraint("CK_BuyerLinkage_DifferentBuyers", "[RequestingBuyerId] <> [ReceivingBuyerId]"));
//            // });

//            // --- Seller Configuration ---
//            // modelBuilder.Entity<Seller>(entity =>
//            // {
//            //     entity.HasKey(s => s.Id);

//            //     entity.HasOne(s => s.User)
//            //         .WithOne()
//            //         .HasForeignKey<Seller>(s => s.Id);

//            //     entity.Ignore(s => s.Email) // not needed, it is in User
//            //         .Ignore(s => s.PhoneNumber); // not needed, it is in User
//            // });

//            // --- Product Configuration ---
//            // modelBuilder.Entity<Product>(entity =>
//            // {
//            //     entity.HasKey(p => p.ProductId);

//            //     // Define the relationship using type parameters and FK only
//            //     entity.HasOne<Seller>()
//            //           .WithMany()
//            //           .HasForeignKey(p => p.SellerId)
//            //           .OnDelete(DeleteBehavior.Restrict);
//            // });

//            // --- Following Entity Configuration ---
//            // modelBuilder.Entity<FollowingEntity>(entity =>
//            // {
//            //     entity.HasKey(f => new { f.BuyerId, f.SellerId });

//            //     entity.HasOne<Buyer>()
//            //     .WithMany()
//            //     .HasForeignKey(f => f.BuyerId)
//            //     .OnDelete(DeleteBehavior.Restrict);

//            //     entity.HasOne<Seller>()
//            //     .WithMany()
//            //     .HasForeignKey(f => f.SellerId)
//            //     .OnDelete(DeleteBehavior.Restrict);
//            // });

//            // --- Seller Notification Entity Configuration ---
//            // modelBuilder.Ignore<Notification>();
//            // modelBuilder.Ignore<ContractRenewalAnswerNotification>();
//            // modelBuilder.Ignore<ContractRenewalWaitlistNotification>();
//            // modelBuilder.Ignore<OutbiddedNotification>();
//            // modelBuilder.Ignore<OrderShippingProgressNotification>();
//            // modelBuilder.Ignore<PaymentConfirmationNotification>();
//            // modelBuilder.Ignore<ProductRemovedNotification>();
//            // modelBuilder.Ignore<ProductAvailableNotification>();
//            // modelBuilder.Ignore<ContractRenewalRequestNotification>();
//            // modelBuilder.Ignore<ContractExpirationNotification>();

//            // modelBuilder.Entity<SellerNotificationEntity>(entity =>
//            // {
//            //     entity.HasKey(sn => sn.NotificationID);

//            //     entity.HasOne<Seller>()
//            //     .WithMany()
//            //     .HasForeignKey(sn => sn.SellerID)
//            //     .OnDelete(DeleteBehavior.Restrict);
//            // });

//            // // --- Order Notification Entity Configuration ---
//            // modelBuilder.Entity<OrderNotificationEntity>(entity =>
//            // {
//            //     entity.HasKey(on => on.NotificationID);

//            //     entity.Property(on => on.Category)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(on => on.Timestamp)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.HasOne<Buyer>()
//            //         .WithMany()
//            //         .HasForeignKey(on => on.RecipientID)
//            //         .OnDelete(DeleteBehavior.Restrict)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.HasOne<Order>()
//            //         .WithMany()
//            //         .HasForeignKey(on => on.OrderID)
//            //         .OnDelete(DeleteBehavior.Restrict)
//            //         .IsRequired(false); // to respect Maria's DB design

//            //     entity.HasOne<Product>()
//            //         .WithMany()
//            //         .HasForeignKey(on => on.ProductID)
//            //         .OnDelete(DeleteBehavior.Restrict)
//            //         .IsRequired(false); // to respect Maria's DB design

//            //     entity.HasOne<Contract>()
//            //         .WithMany()
//            //         .HasForeignKey(on => on.ContractID)
//            //         .OnDelete(DeleteBehavior.Restrict)
//            //         .IsRequired(false); // to respect Maria's DB design

//            //     entity.ToTable(t => t.HasCheckConstraint("NotificationCategoryConstraint", "[Category] IN ('CONTRACT_EXPIRATION', 'OUTBIDDED', 'ORDER_SHIPPING_PROGRESS', 'PRODUCT_AVAILABLE', 'PAYMENT_CONFIRMATION', 'PRODUCT_REMOVED', 'CONTRACT_RENEWAL_REQ', 'CONTRACT_RENEWAL_ANS', 'CONTRACT_RENEWAL_WAITLIST')"));
//            // });

//            // // --- Review Configuration ---
//            // modelBuilder.Entity<Review>(entity =>
//            // {
//            //     entity.HasKey(r => r.ReviewId);

//            //     entity.HasOne<Seller>()
//            //         .WithMany()
//            //         .HasForeignKey(r => r.SellerId)
//            //         .OnDelete(DeleteBehavior.Restrict);
//            // });

//            // // --- Order Summary Configuration ---
//            // modelBuilder.Entity<OrderSummary>(entity =>
//            // {
//            //     entity.HasKey(os => os.ID);
//            // });

//            // // --- Order History Configuration ---
//            // modelBuilder.Entity<OrderHistory>(entity =>
//            // {
//            //     entity.HasKey(oh => oh.OrderID);
//            // });

//            // --- Order Configuration ---
//            // modelBuilder.Entity<Order>(entity =>
//            // {
//            //     entity.HasKey(o => o.OrderID);

//            //     entity.HasOne<Product>()
//            //         .WithMany()
//            //         .HasForeignKey(o => o.ProductID)
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.HasOne<Buyer>()
//            //         .WithMany()
//            //         .HasForeignKey(o => o.BuyerID)
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.HasOne<OrderSummary>()
//            //         .WithMany()
//            //         .HasForeignKey(o => o.OrderSummaryID)
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.HasOne<OrderHistory>()
//            //         .WithMany()
//            //         .HasForeignKey(o => o.OrderHistoryID)
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.ToTable(t => t.HasCheckConstraint("PaymentMethodConstraint", "[PaymentMethod] IN ('card', 'wallet', 'cash')"));
//            // });

//            // // --- PDF Configuration ---
//            // modelBuilder.Entity<PDF>(entity =>
//            // {
//            //     entity.HasKey(p => p.PdfID);
//            //     entity.Ignore(p => p.ContractID); // ignored to respect Maria's DB design (but not deleted to avoid breaking changes)
//            // });

//            // // --- Predefined Contract Configuration ---
//            // modelBuilder.Entity<PredefinedContract>(entity =>
//            // {
//            //     entity.HasKey(pc => pc.ContractID);
//            //     entity.Ignore(pc => pc.ID); // ignored to respect Maria's DB design (but not deleted to avoid breaking changes)
//            // });

//            // // --- Contract Configuration ---
//            // modelBuilder.Entity<Contract>(entity =>
//            // {
//            //     entity.HasKey(c => c.ContractID);

//            //     entity.HasOne<Order>()
//            //         .WithMany()
//            //         .HasForeignKey(c => c.OrderID)
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.HasOne<PredefinedContract>()
//            //         .WithMany()
//            //         .HasForeignKey(pc => pc.PredefinedContractID)
//            //         .IsRequired(false) // nullable to respect Maria's DB design
//            //         .OnDelete(DeleteBehavior.SetNull); // SetNull because it is Nullable

//            //     entity.HasOne<PDF>()
//            //         .WithMany()
//            //         .HasForeignKey(p => p.PDFID)
//            //         .OnDelete(DeleteBehavior.Restrict);

//            //     entity.ToTable(t => t.HasCheckConstraint("ContractStatusConstraint", "[ContractStatus] IN ('ACTIVE', 'RENEWED', 'EXPIRED')"));
//            // });

//            // // --- Tracked Order Configuration ---
//            // modelBuilder.Entity<TrackedOrder>(entity =>
//            // {
//            //     entity.HasKey(to => to.TrackedOrderID);

//            //     entity.HasIndex(to => to.OrderID)
//            //         .IsUnique(); // to respect Maria's DB design

//            //     entity.HasOne<Order>()
//            //         .WithMany()
//            //         .HasForeignKey(to => to.OrderID)
//            //         .OnDelete(DeleteBehavior.Restrict) // set to on delete cascade by Maria, I put restrict to avoid breaking changes (can be changed later)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(to => to.EstimatedDeliveryDate)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(to => to.DeliveryAddress)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(to => to.CurrentStatus)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.ToTable(t => t.HasCheckConstraint("TrackedOrderConstraint", "[CurrentStatus] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')")); // to respect Maria's DB design
//            // });

//            // // --- Order Checkpoint Configuration ---
//            // modelBuilder.Entity<OrderCheckpoint>(entity =>
//            // {
//            //     entity.HasKey(oc => oc.CheckpointID);

//            //     entity.HasOne<TrackedOrder>()
//            //         .WithMany()
//            //         .HasForeignKey(oc => oc.TrackedOrderID)
//            //         .OnDelete(DeleteBehavior.Restrict)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(oc => oc.Timestamp)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(oc => oc.Description)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(oc => oc.Status)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.ToTable(oc => oc.HasCheckConstraint("OrderChekpointConstraint", "[Status] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')")); // to respect Maria's DB design
//            // });

//            // --- Waitlist Product Configuration --- CHANGED TO BORROWEDPRODUCT
//            // modelBuilder.Entity<WaitlistProductEntity>(entity =>
//            // {
//            //     entity.HasKey(wp => wp.WaitlistProductID);

//            //     entity.HasOne<Product>()
//            //         .WithMany()
//            //         .HasForeignKey(wp => wp.ProductID)
//            //         .OnDelete(DeleteBehavior.Restrict) // set to on delete cascade by Maria, I put restrict to avoid breaking changes (can be changed later)
//            //         .IsRequired(); // to respect Maria's DB design
//            // });

//            // // --- User Waitlist Configuration ---
//            // modelBuilder.Entity<UserWaitList>(entity =>
//            // {
//            //     entity.HasKey(uw => uw.UserWaitListID);

//            //     entity.HasOne<WaitlistProductEntity>()
//            //         .WithMany()
//            //         .HasForeignKey(uw => uw.ProductWaitListID)
//            //         .OnDelete(DeleteBehavior.Restrict)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.HasOne<Buyer>()
//            //         .WithMany()
//            //         .HasForeignKey(uw => uw.UserID)
//            //         .OnDelete(DeleteBehavior.Restrict) // set to on delete cascade by Maria, I put restrict to avoid breaking changes (can be changed later)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(uw => uw.JoinedTime)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(uw => uw.PositionInQueue)
//            //         .IsRequired(); // to respect Maria's DB design
//            // });

//            // // --- Dummy Card Configuration ---
//            // // Please do check the DummyCardEntity class for more information about this abomination of a table :))
//            // modelBuilder.Entity<DummyCardEntity>(entity =>
//            // {
//            //     entity.HasKey(dc => dc.ID);

//            //     entity.HasOne<Buyer>() // not in Maria's DB design, I added it to have code maintainability
//            //         .WithMany()
//            //         .HasForeignKey(dc => dc.BuyerId)
//            //         .OnDelete(DeleteBehavior.Restrict) // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)
//            //         .IsRequired();

//            //     entity.HasIndex(dc => dc.CardNumber); // because Golubiro Spioniro is stealing our cards :))

//            //     entity.Property(dc => dc.CardholderName)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(dc => dc.CardNumber)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(dc => dc.CVC)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(dc => dc.Month)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(dc => dc.Year)
//            //         .IsRequired(); // to respect Maria's DB design

//            //     entity.Property(dc => dc.Country)
//            //         .IsRequired(); // to respect Maria's DB design
//            // });

//            // // --- Dummy Wallet Configuration ---
//            // // Please do check the DummyWalletEntity class for more information about this abomination of a table :))
//            // modelBuilder.Entity<DummyWalletEntity>(entity =>
//            // {
//            //     entity.HasOne<Buyer>()
//            //         .WithOne()
//            //         .HasForeignKey<DummyWalletEntity>(dw => dw.BuyerId)
//            //         .OnDelete(DeleteBehavior.Restrict); // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)

//            //     entity.HasKey(dw => dw.ID);
//            // });

//            // // --- Buyer Cart Item Configuration ---
//            // modelBuilder.Entity<BuyerCartItemEntity>(entity =>
//            // {
//            //     entity.HasKey(bci => new { bci.BuyerId, bci.ProductId });

//            //     entity.Property(bci => bci.Quantity)
//            //         .HasDefaultValue(1); // to respect Maria's DB design

//            //     entity.HasOne<Buyer>()
//            //         .WithMany()
//            //         .HasForeignKey(bci => bci.BuyerId)
//            //         .OnDelete(DeleteBehavior.Restrict) // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)
//            //         .IsRequired();

//            //     entity.HasOne<Product>()
//            //         .WithMany()
//            //         .HasForeignKey(bci => bci.ProductId)
//            //         .OnDelete(DeleteBehavior.Restrict) // not specified in Maria's DB design, but I left restrict to avoid breaking changes (can be changed later)
//            //         .IsRequired();
//            // });
//        }
//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            optionsBuilder
//            .ConfigureWarnings(warnings =>
//                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
//        }

//    }
//}
