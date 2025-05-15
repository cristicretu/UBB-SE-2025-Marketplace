using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.ProxyRepository; // Add this using directive
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Helper;

namespace SharedClassLibrary.Service
{
    // Make the class public and implement the interface
    public class ContractService : IContractService
    {
        private readonly IContractRepository contractRpository;

        // Add constructor injection for the repository
        public ContractService(string connectionString)
        {
            contractRpository = new ContractProxyRepository(AppConfig.GetBaseApiUrl()) ?? throw new ArgumentNullException(nameof(connectionString));
        }
        public ContractService(IContractRepository contractRepository)
        {
            contractRpository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
        }

        // Implement the interface methods by calling the repository
        public Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId)
        {
            return contractRpository.GetContractBuyerAsync(contractId);
        }

        public Task<IContract> GetContractByIdAsync(long contractId)
        {
            return contractRpository.GetContractByIdAsync(contractId);
        }

        public Task<List<IContract>> GetContractHistoryAsync(long contractId)
        {
            return contractRpository.GetContractHistoryAsync(contractId);
        }

        public Task<List<IContract>> GetContractsByBuyerAsync(int buyerId)
        {
            return contractRpository.GetContractsByBuyerAsync(buyerId);
        }

        public Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            return contractRpository.GetContractSellerAsync(contractId);
        }

        public Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId)
        {
            return contractRpository.GetDeliveryDateByContractIdAsync(contractId);
        }

        public Task<(string? PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            return contractRpository.GetOrderDetailsAsync(contractId);
        }

        public Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            return contractRpository.GetOrderSummaryInformationAsync(contractId);
        }

        public Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            return contractRpository.GetPdfByContractIdAsync(contractId);
        }

        public Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            return contractRpository.GetPredefinedContractByPredefineContractTypeAsync(predefinedContractType);
        }

        public Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            return contractRpository.GetProductDetailsByContractIdAsync(contractId);
        }

        // Implement the newly added methods
        public Task<List<IContract>> GetAllContractsAsync()
        {
            return contractRpository.GetAllContractsAsync();
        }

        public Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile)
        {
            // Add null check for pdfFile if necessary, though repository might handle it
            if (pdfFile == null)
            {
                throw new ArgumentNullException(nameof(pdfFile));
            }
            return contractRpository.AddContractAsync(contract, pdfFile);
        }
    }
}
