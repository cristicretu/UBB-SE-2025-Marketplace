using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IContractRepository
    {
        Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile);
        Task<List<IContract>> GetAllContractsAsync();
        Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId);
        Task<IContract> GetContractByIdAsync(long contractId);
        Task<List<IContract>> GetContractHistoryAsync(long contractId);
        Task<List<IContract>> GetContractsByBuyerAsync(int buyerId);
        Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId);
        Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId);
        Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId);
        Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId);
        Task<byte[]> GetPdfByContractIdAsync(long contractId);
        Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType);
        // Make StartDate and EndDate nullable in the interface to match the implementation
        Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId);
        Task<PDF> AddPdfAsync(PDF pdf);
        Task UpdateContractPdfIdAsync(long contractId, int pdfId);
        Task UpdateContractStatusAsync(long contractId, string status);
    }
}