// <copyright file="DummyCardRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DataModels;
    using Server.DBConnection;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// A repository for the DummyCardEntity class.
    /// </summary>
    public class DummyCardRepository : IDummyCardRepository
    {
        // private readonly string connectionString;
        // private readonly IDatabaseProvider databaseProvider;
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyCardRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public DummyCardRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Deletes a card from the database.
        /// </summary>
        /// <param name="cardNumber">The card number of the card to be deleted.</param>
        /// <returns> A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when no card with the given number is found.</exception>
        public async Task DeleteCardAsync(string cardNumber)
        {
            DummyCardEntity cardToDelete = await this.dbContext.DummyCards.FirstOrDefaultAsync(dummyCard => dummyCard.CardNumber == cardNumber)
                                                ?? throw new Exception($"DeleteCardAsync: No card with number: {cardNumber}");
            this.dbContext.DummyCards.Remove(cardToDelete);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the balance of a card in the database.
        /// </summary>
        /// <param name="cardNumber">The number of the card to be updated.</param>
        /// <param name="balance">The balance amount the card to be updated to.</param>
        /// <returns> A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when no card with the given number is found.</exception>
        public async Task UpdateCardBalanceAsync(string cardNumber, double balance)
        {
            DummyCardEntity cardToUpdate = await this.dbContext.DummyCards.FirstOrDefaultAsync(dummyCard => dummyCard.CardNumber == cardNumber)
                                                ?? throw new Exception($"UpdateCardBalanceAsync: No card with number: {cardNumber}");
            cardToUpdate.Balance = balance;
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the balance of a card from the database.
        /// </summary>
        /// <param name="cardNumber">The number of the card of which to get the balance from.</param>
        /// <returns> The balance of the card.</returns>
        /// <exception cref="Exception">Thrown when no card with the given number is found.</exception>
        public async Task<double> GetCardBalanceAsync(string cardNumber)
        {
            DummyCardEntity cardToGet = await this.dbContext.DummyCards.FirstOrDefaultAsync(dummyCard => dummyCard.CardNumber == cardNumber)
                                                ?? throw new Exception($"GetCardBalanceAsync: No card with number: {cardNumber}");
            return cardToGet.Balance;
        }
    }
}
