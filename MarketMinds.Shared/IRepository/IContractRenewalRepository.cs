using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.IRepository
{
    public interface IContractRenewalRepository
    {
        Task AddRenewedContractAsync(IContract contract);
        Task<List<IContract>> GetRenewedContractsAsync();
        Task<bool> HasContractBeenRenewedAsync(long contractId);
    }
}