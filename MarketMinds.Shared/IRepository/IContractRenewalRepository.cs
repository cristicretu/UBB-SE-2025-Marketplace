using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IContractRenewalRepository
    {
        Task AddRenewedContractAsync(IContract contract);
        Task<List<IContract>> GetRenewedContractsAsync();
        Task<bool> HasContractBeenRenewedAsync(long contractId);
    }
}