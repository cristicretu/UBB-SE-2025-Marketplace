using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.ProxyRepository; // Added using for Repository namespace
using SharedClassLibrary.IRepository;

namespace SharedClassLibrary.Service
{
    public class ContractRenewalService : IContractRenewalService
    {
        private readonly IContractRenewalRepository contractRenewalRepository; // Added repository field

        // Added constructor for dependency injection
        public ContractRenewalService(IContractRenewalRepository contractRenewalRepository)
        {
            this.contractRenewalRepository = contractRenewalRepository ?? throw new ArgumentNullException(nameof(contractRenewalRepository));
        }

        // Implemented interface methods by calling repository methods
        public Task AddRenewedContractAsync(IContract contract)
        {
            // You might add business logic here before or after calling the repository
            return contractRenewalRepository.AddRenewedContractAsync(contract);
        }

        public Task<List<IContract>> GetRenewedContractsAsync()
        {
            // You might add business logic here
            return contractRenewalRepository.GetRenewedContractsAsync();
        }

        public async Task<bool> HasContractBeenRenewedAsync(long contractId)
        {
            Console.WriteLine($"Checking if contract {contractId} has been renewed");
            Console.WriteLine($"Repository is null: {this.contractRenewalRepository == null}");

            if (contractId <= 0)
            {
                throw new ArgumentException("Contract ID must be a positive number", nameof(contractId));
            }

            // Make sure the repository is not null before using it
            if (this.contractRenewalRepository == null)
            {
                throw new InvalidOperationException("Contract renewal repository is not initialized.");
            }

            return await this.contractRenewalRepository.HasContractBeenRenewedAsync(contractId);
        }

    }
}
