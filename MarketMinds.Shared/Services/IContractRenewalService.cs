using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.Service
{
    public interface IContractRenewalService
    {
        Task AddRenewedContractAsync(IContract contract);
        Task<List<IContract>> GetRenewedContractsAsync();
        Task<bool> HasContractBeenRenewedAsync(long contractId);
    }
}
