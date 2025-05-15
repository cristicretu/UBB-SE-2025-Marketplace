namespace MarketPlace924.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.ProxyRepository;
    using SharedClassLibrary.Service;
    using SharedClassLibrary.Shared;
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using SharedClassLibrary.Helper;

    public class ContractRenewViewModel : IContractRenewViewModel
    {
        // Changed repository interfaces to service interfaces
        private readonly IContractService contractService;
        private readonly IPDFService pdfService;
        private readonly IContractRenewalService renewalService;
        private readonly INotificationContentService notificationContentService;
        private readonly IFileSystem fileSystem;
        private readonly IDateTimeProvider dateTimeProvider;

        public List<IContract> BuyerContracts { get; private set; }

        public IContract SelectedContract { get; private set; } = null!;

        // Constructor with dependency injection for testing - updated parameter types
        public ContractRenewViewModel(string connectionString)
        {
            // Assign injected services to fields
            this.contractService = new ContractService(new ContractProxyRepository(AppConfig.GetBaseApiUrl()));
            this.pdfService = new PDFService();
            this.renewalService = new ContractRenewalService(new ContractRenewalProxyRepository(AppConfig.GetBaseApiUrl()));
            this.notificationContentService = new NotificationContentService();
            this.fileSystem = new FileSystemWrapper();
            this.dateTimeProvider = new DateTimeProvider();
            BuyerContracts = new List<IContract>();
        }

        /// <summary>
        /// Loads all contracts for the given buyer and filters them to include only those with status "ACTIVE" or "RENEWED".
        /// </summary>
        /// <param name="buyerID">The ID of the buyer to load contracts for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadContractsForBuyerAsync(int buyerID)
        {
            // Load all contracts for the buyer using the service
            var allContracts = await contractService.GetContractsByBuyerAsync(buyerID); // Changed from contractModel

            // Filter the contracts to include only those with status "ACTIVE" or "RENEWED"
            BuyerContracts = allContracts.Where(c => c.ContractStatus == "ACTIVE" || c.ContractStatus == "RENEWED").ToList();
        }

        /// <summary>
        /// Retrieves and sets the selected contract by its ID.
        /// </summary>
        /// <param name="contractID">The ID of the contract to select.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SelectContractAsync(long contractID)
        {
            SelectedContract = await contractService.GetContractByIdAsync(contractID); // Changed from contractModel
        }

        /// <summary>
        /// Retrieves the start and end dates of the product associated with a given contract.
        /// </summary>
        // Change DateTime to DateTime? to match the interface and repository
        public async Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            return await contractService.GetProductDetailsByContractIdAsync(contractId); // Changed from contractModel
        }

        /// <summary>
        /// Checks whether the current date is within the valid renewal period (between 2 and 7 days before contract end).
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task<bool> IsRenewalPeriodValidAsync()
        {
            if (SelectedContract == null)
            {
                return false;
            }

            // dates is now (DateTime? StartDate, DateTime? EndDate, double price, string name)?
            var dates = await GetProductDetailsByContractIdAsync(SelectedContract.ContractID);
            // Check if the tuple itself is null OR if EndDate within the tuple is null
            if (!dates.HasValue || !dates.Value.EndDate.HasValue)
            {
                return false;
            }

            // Access EndDate safely using .Value as we've checked HasValue
            DateTime oldEndDate = dates.Value.EndDate.Value;
            DateTime currentDate = dateTimeProvider.Now.Date;
            int daysUntilEnd = (oldEndDate - currentDate).Days;

            return daysUntilEnd <= 7 && daysUntilEnd >= 2;
        }

        /// <summary>
        /// Simulates a check to determine if a product is available.
        /// </summary>
        /// <param name="productId">The ID of the product to check.</param>
        /// <returns>True if the product is available; false otherwise.</returns>
        public virtual bool IsProductAvailable(int productId)
        {
            return true;
        }

        /// <summary>
        /// Simulates a check to determine if the seller can approve a renewal based on the renewal count.
        /// </summary>
        /// <param name="renewalCount">The current renewal count of the contract.</param>
        /// <returns>True if the seller can approve the renewal; false otherwise.</returns>
        public virtual bool CanSellerApproveRenewal(int renewalCount)
        {
            return renewalCount < 1;
        }

        /// <summary>
        /// Checks whether the currently selected contract has already been renewed.
        /// </summary>
        /// <returns>True if the contract has been renewed; false otherwise.</returns>
        public async Task<bool> HasContractBeenRenewedAsync()
        {
            if (SelectedContract == null)
            {
                return false;
            }

            return await renewalService.HasContractBeenRenewedAsync(SelectedContract.ContractID); // Changed from renewalModel
        }

        /// <summary>
        /// Generates a PDF document containing the contract content.
        /// </summary>
        /// <param name="contract">The contract object to include in the PDF.</param>
        /// <param name="content">The content of the contract to include in the PDF.</param>
        /// <returns>The byte array representing the generated PDF.</returns>
        public virtual byte[] GenerateContractPdf(IContract contract, string content)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    page.Content().Text(content);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Submits a request to renew the selected contract if all business rules are satisfied.
        /// Also generates and saves a new PDF and sends notifications.
        /// </summary>
        /// <param name="newEndDate">The new end date for the contract.</param>
        /// <param name="buyerID">The ID of the buyer submitting the renewal request.</param>
        /// <param="productID">The ID of the product associated with the contract.</param>
        /// <param name="sellerID">The ID of the seller associated with the contract.</param>
        /// <returns>A tuple containing a boolean indicating success and a message describing the result.</returns>
        public virtual async Task<(bool Success, string Message)> SubmitRenewalRequestAsync(DateTime newEndDate, int buyerID, int productID, int sellerID)
        {
            try
            {
                // Ensure a contract is selected before proceeding
                if (SelectedContract == null)
                {
                    return (false, "No contract selected.");
                }

                // Check if the contract was already renewed using the service
                if (await HasContractBeenRenewedAsync()) // This method now uses renewalService internally
                {
                    return (false, "This contract has already been renewed.");
                }

                // Validate the current date is within the renewal window (2 to 7 days before end)
                if (!await IsRenewalPeriodValidAsync()) // This method uses GetProductDetailsByContractIdAsync which now uses contractService
                {
                    return (false, "Contract is not in a valid renewal period (between 2 and 7 days before end date).");
                }

                // Get the current contract's product dates using the service
                var oldDates = await GetProductDetailsByContractIdAsync(SelectedContract.ContractID); // This method now uses contractService
                // Check if the tuple itself is null OR if EndDate within the tuple is null
                if (!oldDates.HasValue || !oldDates.Value.EndDate.HasValue)
                {
                    return (false, "Could not retrieve current contract dates.");
                }

                // Ensure the new end date is after the old one (access EndDate safely)
                if (newEndDate <= oldDates.Value.EndDate.Value)
                {
                    return (false, "New end date must be after the current end date.");
                }

                // Check if product is available for renewal
                if (!IsProductAvailable(productID))
                {
                    return (false, "Product is not available.");
                }

                // Check if seller allows renewal (based on renewal count)
                if (!CanSellerApproveRenewal(SelectedContract.RenewalCount))
                {
                    return (false, "Renewal not allowed: seller limit exceeded.");
                }

                // Build the updated contract content text
                string contractContent = $"Renewed Contract for Order {SelectedContract.OrderID}.\nOriginal Contract ID: {SelectedContract.ContractID}.\nNew End Date: {newEndDate:dd/MM/yyyy}";

                // Generate the contract PDF
                byte[] pdfBytes = GenerateContractPdf(SelectedContract, contractContent);

                // Insert the new PDF into the database and get its ID
                int newPdfId = await this.pdfService.InsertPdfAsync(pdfBytes);

                // Save PDF locally in Downloads folder
                string downloadsPath = fileSystem.GetDownloadsPath();
                string fileName = $"RenewedContract_{SelectedContract.ContractID}_to_{newEndDate:yyyyMMdd}.pdf";
                string filePath = fileSystem.CombinePath(downloadsPath, fileName);
                await fileSystem.WriteAllBytesAsync(filePath, pdfBytes);

                // Prepare and insert the new renewed contract into the database using the service
                var updatedContract = new Contract
                {
                    OrderID = SelectedContract.OrderID,
                    ContractStatus = "RENEWED",
                    ContractContent = contractContent,
                    RenewalCount = SelectedContract.RenewalCount + 1,
                    PredefinedContractID = SelectedContract.PredefinedContractID,
                    PDFID = newPdfId,
                    RenewedFromContractID = SelectedContract.ContractID
                };

                await renewalService.AddRenewedContractAsync(updatedContract); // Changed from renewalModel

                // Send notifications to seller, buyer, and waitlist
                var now = dateTimeProvider.Now;
                notificationContentService.AddNotification(new ContractRenewalRequestNotification(sellerID, now, (int)SelectedContract.ContractID));
                notificationContentService.AddNotification(new ContractRenewalAnswerNotification(buyerID, now, (int)SelectedContract.ContractID, true));
                notificationContentService.AddNotification(new ContractRenewalWaitlistNotification(999, now, productID));

                return (true, "Contract renewed successfully!");
            }
            catch (Exception ex)
            {
                // Catch any unexpected errors and return an appropriate message
                return (false, $"Unexpected error: {ex.Message}");
            }
        }
    }

    // Interface for file system operations to enable testing
    public interface IFileSystem
    {
        string GetDownloadsPath();
        string CombinePath(string path1, string path2);
        Task WriteAllBytesAsync(string path, byte[] bytes);
    }

    // Concrete implementation of IFileSystem that uses actual file system
    public class FileSystemWrapper : IFileSystem
    {
        public string GetDownloadsPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        public string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public async Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            await File.WriteAllBytesAsync(path, bytes);
        }
    }

    // Interface for DateTime operations to enable testing
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }

    // Concrete implementation of IDateTimeProvider that uses actual DateTime
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
