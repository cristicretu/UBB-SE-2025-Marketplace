using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;

namespace MarketPlace924.ViewModel
{
    /// <summary>
    /// Defines operations for handling contracts including retrieval, addition, and associated order information.
    /// </summary>
    public interface IContractViewModel
    {
        /// <summary>
        /// Asynchronously retrieves a contract by its unique identifier.
        /// </summary>
        /// <param name="contractId">The unique identifier of the contract.</param>
        /// <returns>A task that represents the asynchronous operation and returns the <see cref="IContract"/>.</returns>
        Task<IContract> GetContractByIdAsync(long contractId);

        /// <summary>
        /// Asynchronously retrieves all contracts.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation and returns a list of all <see cref="IContract"/> objects.</returns>
        Task<List<IContract>> GetAllContractsAsync();

        /// <summary>
        /// Asynchronously retrieves the history of the specified contract.
        /// </summary>
        /// <param name="contractId">The unique identifier of the contract.</param>
        /// <returns>A task that represents the asynchronous operation and returns a list of <see cref="IContract"/> objects representing the contract history.</returns>
        Task<List<IContract>> GetContractHistoryAsync(long contractId);

        /// <summary>
        /// Asynchronously retrieves the seller details associated with the specified contract.
        /// </summary>
        /// <param name="contractId">The unique identifier of the contract.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a tuple containing the seller's ID and name.
        /// </returns>
        Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId);

        /// <summary>
        /// Asynchronously retrieves the buyer details associated with the specified contract.
        /// </summary>
        /// <param name="contractId">The unique identifier of the contract.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a tuple containing the buyer's ID and name.
        /// </returns>
        Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId);

        /// <summary>
        /// Asynchronously retrieves the order summary information for the specified contract.
        /// </summary>
        /// <param name="contractId">The unique identifier of the contract.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a dictionary containing the order summary information.
        /// </returns>
        Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId);

        /// <summary>
        /// Asynchronously retrieves the product details associated with the specified contract.
        /// </summary>
        /// <param name="contractId">The unique identifier of the contract.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a tuple with the start date, end date, price, and name; or <c>null</c> if not found.
        /// </returns>
        Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId);

        /// <summary>
        /// Asynchronously retrieves all contracts for a given buyer.
        /// </summary>
        /// <param name="buyerId">The unique identifier of the buyer.</param>
        /// <returns>A task that represents the asynchronous operation and returns a list of <see cref="IContract"/> objects.</returns>
        Task<List<IContract>> GetContractsByBuyerAsync(int buyerId);

        /// <summary>
        /// Asynchronously adds a new contract along with its associated PDF file.
        /// </summary>
        /// <param name="contract">The contract object to add.</param>
        /// <param name="pdfFile">A byte array representing the PDF file associated with the contract.</param>
        /// <returns>A task that represents the asynchronous operation and returns the added <see cref="IContract"/>.</returns>
        Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile);

        /// <summary>
        /// Asynchronously retrieves a predefined contract based on the specified contract type.
        /// </summary>
        /// <param name="predefinedContractType">The predefined contract type.</param>
        /// <returns>A task that represents the asynchronous operation and returns the <see cref="IPredefinedContract"/>.</returns>
        Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType);

        /// <summary>
        /// Asynchronously retrieves the order details for the specified contract.
        /// </summary>
        /// <param name="contractId">The unique identifier of the contract.</param>
        /// <returns>
        /// A task that represents the asynchronous operation and returns a tuple containing the payment method (nullable) and order date.
        /// </returns>
        Task<(string? PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId);

        /// <summary>
        /// Asynchronously retrieves the delivery date for the specified contract.
        /// </summary>
        /// <param name="contractId">The unique identifier of the contract.</param>
        /// <returns>A task that represents the asynchronous operation and returns the delivery date or <c>null</c> if not available.</returns>
        Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId);

        /// <summary>
        /// Asynchronously retrieves the PDF file associated with the specified contract.
        /// </summary>
        /// <param name="contractId">The unique identifier of the contract.</param>
        /// <returns>A task that represents the asynchronous operation and returns a byte array representing the PDF file.</returns>
        Task<byte[]> GetPdfByContractIdAsync(long contractId);

        /// <summary>
        /// Asynchronously generates a contract document and saves it based on the provided contract and type.
        /// </summary>
        /// <param name="contract">The contract object for which to generate the document.</param>
        /// <param name="contractType">The type of the predefined contract.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task GenerateAndSaveContractAsync();
    }
}
