using MarketMinds.Shared.Models;

namespace SharedClassLibrary.Service
{
    public interface IContractRenewalService
    {
        Task AddRenewedContractAsync(IContract contract);
        Task<List<IContract>> GetRenewedContractsAsync();
        Task<bool> HasContractBeenRenewedAsync(long contractId);
    }
}
