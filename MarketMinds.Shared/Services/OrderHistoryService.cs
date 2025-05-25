using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;

using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Helper;

namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Service for managing order history operations.
    /// </summary>
    public class OrderHistoryService : IOrderHistoryService
    {
        private readonly IOrderHistoryRepository orderHistoryRepository;



        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryService"/> class with a specified database provider.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="databaseProvider">The database provider to use.</param>
        public OrderHistoryService()
        {
            this.orderHistoryRepository = new OrderHistoryProxyRepository(AppConfig.GetBaseApiUrl());
        }

        // Add this constructor for testing and DI
        public OrderHistoryService(IOrderHistoryRepository orderHistoryRepository)
        {
            this.orderHistoryRepository = orderHistoryRepository;
        }


        /// <inheritdoc/>
        public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryId)
        {
            if (orderHistoryId <= 0)
            {
                throw new ArgumentException("Order history ID must be positive", nameof(orderHistoryId));
            }

            return await orderHistoryRepository.GetProductsFromOrderHistoryAsync(orderHistoryId);
        }

        /// <inheritdoc/>
        public async Task<int> CreateOrderHistoryAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("User ID must be positive", nameof(userId));
                }
                var newOrderHistoryId = await orderHistoryRepository.CreateOrderHistoryAsync(userId);

                return newOrderHistoryId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}