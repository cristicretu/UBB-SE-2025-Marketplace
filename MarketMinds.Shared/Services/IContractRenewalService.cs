using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services
{
    public interface IContractRenewalService
    {
        Task AddRenewedContractAsync(IContract contract);
        Task<List<IContract>> GetRenewedContractsAsync();
        Task<bool> HasContractBeenRenewedAsync(long contractId);
    }
}
