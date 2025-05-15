// <copyright file="ContractRenewalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Shared;

    /// <summary>
    /// Represents a repository for contract renewal operations.
    /// </summary>
    public class ContractRenewalRepository : IContractRenewalRepository
    {
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the ContractRenewalRepository class.
        /// </summary>
        /// <param name="dbContext">The database context instance.</param>
        public ContractRenewalRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Asynchronously adds a renewed contract to the database.
        /// The PDF was aleady added in the database and the PDFID was set in the ViewModel.
        /// </summary>
        /// <param name="contract">The renewed contract to add to the database.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddRenewedContractAsync(IContract contract)
        {
            await this.dbContext.Contracts.AddAsync(contract.ToContract());
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously checks whether a contract has already been renewed by verifying
        /// if there exists any contract in the database with the given contract ID
        /// as its RenewedFromContractID.
        /// </summary>
        /// <param name="contractId">The ID of the contract to check.</param>
        /// <returns>A task representing the asynchronous operation. The task result is true if the contract has been renewed; otherwise, false.</returns>
        public async Task<bool> HasContractBeenRenewedAsync(long contractId)
        {
            return await this.dbContext.Contracts.AnyAsync(contract => contract.RenewedFromContractID == contractId);
        }

        /// <summary>
        /// Asynchronously retrieves all contracts with status 'RENEWED' using the GetRenewedContracts stored procedure.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result is a list of all renewed contracts.</returns>
        public async Task<List<IContract>> GetRenewedContractsAsync()
        {
            List<Contract> contracts = await this.dbContext.Contracts.Where(contract => contract.ContractStatus == "RENEWED").ToListAsync();
            return contracts.Cast<IContract>().ToList();
        }
    }
}