using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.Service
{
    public interface IContractService
    {
        Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId);
        Task<IContract> GetContractByIdAsync(long contractId);
        Task<List<IContract>> GetContractHistoryAsync(long contractId);
        Task<List<IContract>> GetContractsByBuyerAsync(int buyerId);
        Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId);
        Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId);
        Task<(string? PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId);
        Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId);
        Task<byte[]> GetPdfByContractIdAsync(long contractId);
        Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType);
        Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId);
        // Add the missing methods
        Task<List<IContract>> GetAllContractsAsync();
        Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile);
    }
}
