using Microsoft.EntityFrameworkCore;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace Server.MarketMinds.Repositories.AccountRepository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly DataAccessLayer.ApplicationDbContext context;

        public AccountRepository(DataAccessLayer.ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            return user;
        }

        public async Task<List<UserOrder>> GetUserOrdersAsync(int userId)
        {
            var orders = await context.Orders
                .Where(o => o.SellerId == userId || o.BuyerId == userId)
                .OrderByDescending(o => o.Id)
                .Select(o => new UserOrder
                {
                    Id = o.Id,
                    ItemName = o.Name,
                    Description = o.Description,
                    Price = (float)o.Cost,
                    SellerId = o.SellerId,
                    BuyerId = o.BuyerId
                })
                .ToListAsync();

            return orders;
        }

        public async Task<double> GetBasketTotalAsync(int userId, int basketId)
        {
            // Validate parameters
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be a positive number", nameof(userId));
            }

            if (basketId <= 0)
            {
                throw new ArgumentException("Basket ID must be a positive number", nameof(basketId));
            }
            // Validate the basket belongs to the user
            var basket = await context.Baskets
                .FirstOrDefaultAsync(b => b.Id == basketId && b.BuyerId == userId);

            if (basket == null)
            {
                throw new ArgumentException($"Basket with ID {basketId} not found for user {userId}", nameof(basketId));
            }

            // Calculate the total cost of all items in the basket
            var basketItems = await context.BasketItems
                .Where(i => i.BasketId == basketId)
                .ToListAsync();

            if (basketItems == null || !basketItems.Any())
            {
                return 0; // Empty basket has zero cost
            }

            double totalCost = basketItems.Sum(item => item.Price * item.Quantity);
            return totalCost;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null || user.Id <= 0)
            {
                throw new ArgumentException("Valid user must be provided", nameof(user));
            }

            try
            {
                context.Entry(user).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<Order>> CreateOrderFromBasketAsync(int userId, int basketId, double discountAmount = 0)
        {
            // Validate parameters
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be a positive number", nameof(userId));
            }

            if (basketId <= 0)
            {
                throw new ArgumentException("Basket ID must be a positive number", nameof(basketId));
            }

            // Get user to make sure they exist
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found", nameof(userId));
            }

            // Load basket first
            var basket = await context.Baskets.FindAsync(basketId);
            if (basket == null || basket.BuyerId != userId)
            {
                throw new ArgumentException($"Basket with ID {basketId} not found for user {userId}", nameof(basketId));
            }

            // Load basket items separately without using navigation properties
            var basketItems = await context.BasketItems
                .Where(i => i.BasketId == basketId)
                .ToListAsync();

            if (basketItems == null || !basketItems.Any())
            {
                throw new InvalidOperationException("Cannot create order from empty basket");
            }

            // Load the corresponding products for each basket item
            var productIds = basketItems.Select(i => i.ProductId).Distinct().ToList();
            var products = await context.BuyProducts
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            // Create a dictionary for quick lookup
            var productDictionary = products.ToDictionary(p => p.Id);

            var createdOrders = new List<Order>();

            // Use a transaction to ensure all orders are created or none
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Group by seller
                    var itemsBySeller = basketItems
                        .GroupBy(item =>
                        {
                            // Get the product for this item
                            if (productDictionary.TryGetValue(item.ProductId, out var product))
                            {
                                return product.SellerId;
                            }
                            // In case product not found, use a fallback
                            return 0; // Default seller ID if product not found
                        });

                    double totalBasketCost = basketItems.Sum(item => item.Price * item.Quantity);

                    if (totalBasketCost <= 0 || discountAmount <= 0)
                    {
                        discountAmount = 0;
                    }
                    else if (discountAmount > totalBasketCost)
                    {
                        discountAmount = totalBasketCost;
                    }

                    foreach (var sellerGroup in itemsBySeller)
                    {
                        int sellerId = sellerGroup.Key;
                        // Skip orders for invalid seller IDs
                        if (sellerId <= 0)
                        {
                            continue;
                        }

                        var sellerItems = sellerGroup.ToList();

                        // Calculate total cost for this seller's items
                        double sellerTotalCost = sellerItems.Sum(item => item.Price * item.Quantity);

                        double sellerDiscount = 0;
                        if (totalBasketCost > 0 && discountAmount > 0)
                        {
                            sellerDiscount = (sellerTotalCost / totalBasketCost) * discountAmount;
                        }

                        double discountedSellerTotal = sellerTotalCost - sellerDiscount;

                        // Create detailed description listing all items
                        var itemDescriptions = new List<string>();
                        foreach (var item in sellerItems)
                        {
                            var product = productDictionary[item.ProductId];
                            itemDescriptions.Add($"{product.Title} (x{item.Quantity}) - ${item.Price * item.Quantity:F2}");
                        }

                        string discountDescription = sellerDiscount > 0
                            ? $" | Discount: ${sellerDiscount:F2}"
                            : string.Empty;

                        string detailedDescription = string.Join(", ", itemDescriptions) + discountDescription;

                        var order = new Order
                        {
                            Name = $"Order from {user.Username}",
                            Description = detailedDescription,
                            Cost = discountedSellerTotal,
                            SellerId = sellerId,
                            BuyerId = userId
                        };

                        // Add order to database
                        context.Orders.Add(order);
                        await context.SaveChangesAsync();

                        createdOrders.Add(order);
                    }

                    // Clear the basket after creating orders
                    context.BasketItems.RemoveRange(basketItems);
                    await context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();
                    return createdOrders;
                }
                catch (Exception ex)
                {
                    // Rollback transaction on any error
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}