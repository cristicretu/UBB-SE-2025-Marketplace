using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Server.DataAccessLayer
{
    public static class DatabaseSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Make sure the database is created and all migrations are applied
                await context.Database.MigrateAsync();

                // Create a test seller if none exists
                if (!await context.Sellers.AnyAsync())
                {
                    var user = new User
                    {
                        Username = "testseller",
                        Email = "seller@example.com",
                        PhoneNumber = "+1234567890",
                        UserType = 2 // Assuming 2 is for sellers
                    };

                    await context.Users.AddAsync(user);
                    await context.SaveChangesAsync();

                    var seller = new Seller
                    {
                        Id = user.Id, // Use the same ID as the user
                        StoreName = "Test Store",
                        StoreDescription = "A test store for development purposes",
                        StoreAddress = "123 Test St, Test City",
                        FollowersCount = 0,
                        TrustScore = 0
                    };

                    await context.Sellers.AddAsync(seller);
                    await context.SaveChangesAsync();
                }

                var sellerEntity = await context.Sellers.FirstOrDefaultAsync();
                if (sellerEntity == null)
                {
                    throw new Exception("No sellers found in the database. Please create a seller first.");
                }

                // Check if we already have any users
                if (!await context.Users.AnyAsync())
                {
                    // Add a test user
                    var user = new User
                    {
                        Username = "testuser",
                        Email = "test@example.com",
                        PhoneNumber = "1234567890",
                        UserType = 1, // Assuming 1 is a regular user
                        Balance = 1000.0,
                        FailedLogIns = 0,
                        IsBanned = false
                    };

                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                }

                // Check if we have any categories
                if (!await context.ProductCategories.AnyAsync())
                {
                    var categories = new List<Category>
                    {
                        new() { Name = "Electronics" },
                        new() { Name = "Books" },
                        new() { Name = "Tools" },
                        new() { Name = "Sports Equipment" },
                        new() { Name = "Clothing" }
                    };

                    await context.ProductCategories.AddRangeAsync(categories);
                    await context.SaveChangesAsync();
                }

                // Check if we have any conditions
                if (!await context.ProductConditions.AnyAsync())
                {
                    var conditions = new List<Condition>
                    {
                        new() { Name = "New", Description = "Brand new, never used" },
                        new() { Name = "Like New", Description = "Gently used, looks like new" },
                        new() { Name = "Good", Description = "Used but in good condition" },
                        new() { Name = "Fair", Description = "Shows signs of wear but still functional" },
                        new() { Name = "Poor", Description = "Heavily used, may have defects" }
                    };

                    await context.ProductConditions.AddRangeAsync(conditions);
                    await context.SaveChangesAsync();
                }

                // Check if we have any tags
                if (!await context.ProductTags.AnyAsync())
                {
                    var tags = new List<ProductTag>
                    {
                        new() { Title = "Popular" },
                        new() { Title = "Rare" },
                        new() { Title = "New Arrival" },
                        new() { Title = "Best Deal" },
                        new() { Title = "Eco-friendly" }
                    };

                    await context.ProductTags.AddRangeAsync(tags);
                    await context.SaveChangesAsync();
                }


                // Now seed some borrow products if we don't have any
                if (!await context.BorrowProducts.AnyAsync())
                {
                    var user = await context.Users.FirstOrDefaultAsync();
                    var categories = await context.ProductCategories.ToListAsync();
                    var conditions = await context.ProductConditions.ToListAsync();
                    var tags = await context.ProductTags.ToListAsync();

                    var borrowProducts = new List<BorrowProduct>
                    {
                        new()
                        {
                            Title = "Canon EOS R5 Camera",
                            Description = "Professional mirrorless camera with 45MP full-frame sensor. Perfect for photography enthusiasts.",
                            SellerId = sellerEntity.Id,
                            Category = categories[0], // Electronics
                            CategoryId = categories[0].Id,
                            Condition = conditions[1], // Like New
                            ConditionId = conditions[1].Id,
                            Price = 0, // Not used for borrow products, but required by base class
                            Stock = 1,  // Not used for borrow products, but required by base class
                            TimeLimit = DateTime.UtcNow.AddDays(14),
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddDays(14),
                            DailyRate = 25.99,
                            IsBorrowed = false,
                            Images = new List<BorrowProductImage>
                            {
                                new() { Url = "https://example.com/images/camera1.jpg" },
                                new() { Url = "https://example.com/images/camera2.jpg" }
                            },
                            ProductTags = new List<BorrowProductProductTag>
                            {
                                new() { Tag = tags[0] }, // Popular
                                new() { Tag = tags[2] }  // New Arrival
                            }
                        },
                        new()
                        {
                            Title = "Camping Tent - 4 Person",
                            Description = "Spacious 4-person tent, waterproof and easy to set up. Perfect for weekend getaways.",
                            SellerId = sellerEntity.Id,
                            Category = categories[3], // Sports Equipment
                            CategoryId = categories[3].Id,
                            Condition = conditions[2], // Good
                            ConditionId = conditions[2].Id,
                            Price = 0, // Not used for borrow products, but required by base class
                            Stock = 1,  // Not used for borrow products, but required by base class
                            TimeLimit = DateTime.UtcNow.AddDays(30),
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddDays(30),
                            DailyRate = 15.50,
                            IsBorrowed = false,
                            Images = new List<BorrowProductImage>
                            {
                                new() { Url = "https://example.com/images/tent1.jpg" }
                            },
                            ProductTags = new List<BorrowProductProductTag>
                            {
                                new() { Tag = tags[4] }  // Eco-friendly
                            }
                        },
                        new()
                        {
                            Title = "Professional Drone with 4K Camera",
                            Description = "DJI Mavic Air 2 with 4K/60fps video, 48MP photos, and 34-minute flight time.",
                            SellerId = sellerEntity.Id,
                            Category = categories[0], // Electronics
                            CategoryId = categories[0].Id,
                            Condition = conditions[0], // New
                            ConditionId = conditions[0].Id,
                            Price = 0, // Not used for borrow products, but required by base class
                            Stock = 1,  // Not used for borrow products, but required by base class
                            TimeLimit = DateTime.UtcNow.AddDays(7),
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddDays(7),
                            DailyRate = 45.99,
                            IsBorrowed = false,
                            Images = new List<BorrowProductImage>
                            {
                                new() { Url = "https://example.com/images/drone1.jpg" },
                                new() { Url = "https://example.com/images/drone2.jpg" },
                                new() { Url = "https://example.com/images/drone3.jpg" }
                            },
                            ProductTags = new List<BorrowProductProductTag>
                            {
                                new() { Tag = tags[0] }, // Popular
                                new() { Tag = tags[1] }, // Rare
                                new() { Tag = tags[2] }  // New Arrival
                            }
                        }
                    };

                    await context.BorrowProducts.AddRangeAsync(borrowProducts);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
