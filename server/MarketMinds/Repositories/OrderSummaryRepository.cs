// <copyright file="OrderSummaryRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Server.DataAccessLayer;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides database operations for order summary management.
    /// </summary>
    public class OrderSummaryRepository : IOrderSummaryRepository
    {
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderSummaryRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public OrderSummaryRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task UpdateOrderSummaryAsync(int id, double subtotal, double warrantyTax, double deliveryFee, double finalTotal, string fullName, string email, string phoneNumber, string address, string postalCode, string additionalInfo, string contractDetails)
        {
            OrderSummary? orderSummary = await this.dbContext.OrderSummary.FindAsync(id)
                                                ?? throw new KeyNotFoundException($"UpdateOrderSummaryAsync: OrderSummary with ID {id} not found");

            // Update the order summary
            orderSummary.Subtotal = subtotal;
            orderSummary.WarrantyTax = warrantyTax;
            orderSummary.DeliveryFee = deliveryFee;
            orderSummary.FinalTotal = finalTotal;
            orderSummary.FullName = fullName;
            orderSummary.Email = email;
            orderSummary.PhoneNumber = phoneNumber;
            orderSummary.Address = address;
            orderSummary.PostalCode = postalCode;
            orderSummary.AdditionalInfo = additionalInfo;
            orderSummary.ContractDetails = contractDetails;

            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<OrderSummary> GetOrderSummaryByIdAsync(int orderSummaryId)
        {
            OrderSummary? orderSummary = await this.dbContext.OrderSummary.FindAsync(orderSummaryId)
                                                ?? throw new KeyNotFoundException($"GetOrderSummaryByIdAsync: OrderSummary with ID {orderSummaryId} not found");

            return orderSummary;
        }

        public async Task<int> AddOrderSummaryAsync(OrderSummary orderSummary)
        {
            try
            {
                await this.dbContext.OrderSummary.AddAsync(orderSummary);
                await this.dbContext.SaveChangesAsync();
                return orderSummary.ID;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
