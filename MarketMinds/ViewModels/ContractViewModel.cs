using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Service; // Add this using directive
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Windows.Storage;
using Windows.System;

namespace MarketPlace924.ViewModel
{
    public class ContractViewModel : IContractViewModel
    {
        // Change the type from IContractRepository to IContractService
        private readonly IContractService contractService;

        /// <summary>
        /// Constructor for the ContractViewModel
        /// </summary>
        /// <param name="contractService" type="IContractService">The contract service instance</param>
        // Update the constructor to accept IContractService
        public ContractViewModel(string connectionString)
        {
            // Assign the injected service instance
            this.contractService = new ContractService(connectionString);
        }

        /// <summary>
        /// Get a contract by its ID
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The contract with the given ID></returns>
        public async Task<IContract> GetContractByIdAsync(long contractId)
        {
            // Call the method on the service
            return await contractService.GetContractByIdAsync(contractId);
        }

        /// <summary>
        /// Get all contracts
        /// </summary>
        /// <returns The list of contracts></returns>
        // Uncomment the method
        public async Task<List<IContract>> GetAllContractsAsync()
        {
            // Call the method on the service
            return await contractService.GetAllContractsAsync();
            // Remove the NotImplementedException
            // throw new NotImplementedException("GetAllContractsAsync is not implemented in IContractService.");
        }

        /// <summary>
        /// Get the history of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The list of contracts that are related to the given contract></returns>
        public async Task<List<IContract>> GetContractHistoryAsync(long contractId)
        {
            // Call the method on the service
            return await contractService.GetContractHistoryAsync(contractId);
        }

        /// <summary>
        /// Add a contract to the database
        /// </summary>
        /// <param name="contract" type="Contract">The contract to add</param>
        /// <param name="pdfFile" type="byte[]">The PDF file of the contract</param>
        /// <returns The added contract></returns>
        // Uncomment the method
        public async Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile)
        {
            if (pdfFile == null)
            {
                throw new ArgumentNullException(nameof(pdfFile));
            }
            // Call the method on the service
            return await contractService.AddContractAsync(contract, pdfFile);
            // Remove the NotImplementedException
            // throw new NotImplementedException("AddContractAsync is not implemented in IContractService.");
        }

        /// <summary>
        /// Get the seller of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The ID and name of the seller></returns>
        public async Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            // Call the method on the service
            return await contractService.GetContractSellerAsync(contractId);
        }

        /// <summary>
        /// Get the buyer of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The ID and name of the buyer></returns>
        public async Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId)
        {
            // Call the method on the service
            return await contractService.GetContractBuyerAsync(contractId);
        }

        /// <summary>
        /// Get the order summary information of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The order summary information></returns>
        public async Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            // Call the method on the service
            return await contractService.GetOrderSummaryInformationAsync(contractId);
        }

        /// <summary>
        /// Get the product details of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The product details></returns>
        public async Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            // Call the method on the service
            return await contractService.GetProductDetailsByContractIdAsync(contractId);
        }

        /// <summary>
        /// Get the contracts of a buyer
        /// </summary>
        /// <param name="buyerId" type="int">The ID of the buyer</param>
        /// <returns The list of contracts of the buyer></returns>
        public async Task<List<IContract>> GetContractsByBuyerAsync(int buyerId)
        {
            // Call the method on the service
            return await contractService.GetContractsByBuyerAsync(buyerId);
        }

        /// <summary>
        /// Get the predefined contract by its type
        /// </summary>
        /// <param name="predefinedContractType" type="PredefinedContractType">The type of the predefined contract</param>
        /// <returns The predefined contract with the given type></returns>
        public async Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            // Call the method on the service
            return await contractService.GetPredefinedContractByPredefineContractTypeAsync(predefinedContractType);
        }

        /// <summary>
        /// Get the order details of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The order details></returns>
        public async Task<(string? PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            // Call the method on the service
            return await contractService.GetOrderDetailsAsync(contractId);
        }

        /// <summary>
        /// Get the delivery date of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The delivery date></returns>
        public async Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId)
        {
            // Call the method on the service
            return await contractService.GetDeliveryDateByContractIdAsync(contractId);
        }

        /// <summary>
        /// Get the PDF of a contract
        /// </summary>
        /// <param name="contractId" type="long">The ID of the contract</param>
        /// <returns The PDF of the contract></returns>
        public async Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            // Call the method on the service
            return await contractService.GetPdfByContractIdAsync(contractId);
        }

        // ... existing private methods GenerateContractPdf, GetFieldReplacements, GenerateAndSaveContractAsync ...
        // These private methods already call the public async methods of this ViewModel,
        // which now correctly delegate to the contractService. No changes needed here.
        private byte[] GenerateContractPdf(
                IContract contract,
                IPredefinedContract predefinedContract,
                Dictionary<string, string> fieldReplacements)
        {
            // Validate inputs.
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }
            if (predefinedContract == null)
            {
                throw new ArgumentNullException(nameof(predefinedContract));
            }

            // Replace format variables in the content.
            string content = predefinedContract.ContractContent;
            foreach (var pair in fieldReplacements)
            {
                content = content.Replace("{" + pair.Key + "}", pair.Value);
            }

            // Replace specific placeholders.
            content = content.Replace("{ContractID}", contract.ContractID.ToString());
            content = content.Replace("{OrderID}", contract.OrderID.ToString());
            content = content.Replace("{ContractStatus}", contract.ContractStatus);
            content = content.Replace("{AdditionalTerms}", contract.AdditionalTerms);

            // Set the QuestPDF license.
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(textStyle => textStyle.FontSize(12).FontFamily("Segoe UI"));

                    // Header section with title.
                    page.Header().Element(header =>
                    {
                        // Apply container-wide styling and combine multiple elements inside a Column
                        header
                            .PaddingBottom(10)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Column(column =>
                            {
                                // The Column itself is the single child of the header container.
                                column.Item()
                                      .Text("Contract Document")
                                      .SemiBold()
                                      .FontSize(20)
                                      .AlignCenter();
                            });
                    });

                    // Content section.
                    page.Content().Element(contentContainer =>
                    {
                        // Apply padding and wrap the text in a Column container.
                        contentContainer
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                column.Item()
                                      .Text(content);
                                // .TextAlignment(TextAlignment.Justify);
                            });
                    });

                    // Footer section with generation date and page numbers.
                    page.Footer().Element(footer =>
                    {
                        footer
                        .PaddingTop(10)
                        .BorderTop(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Column(column =>
                            column.Item().Row(row =>
                            {
                                // Left part: Generation date.
                                row.RelativeItem()
                                   .Text($"Generated on: {DateTime.Now.ToShortDateString()}")
                                   .FontSize(10)
                                   .FontColor(Colors.Grey.Medium);

                                // Right part: Page numbering.
                                row.ConstantItem(100)
                                   .AlignRight()
                                   .Text(text =>
                                   {
                                       text.DefaultTextStyle(x => x.FontColor(Colors.Grey.Medium)
                                                                    .FontSize(10));
                                       text.Span("Page ");
                                       text.CurrentPageNumber();
                                       text.Span(" of ");
                                       text.TotalPages();
                                   });
                            }));
                    });
                });
            });

            // Generate and return the PDF as a byte array.
            return document.GeneratePdf();
        }

        private async Task<Dictionary<string, string>> GetFieldReplacements(IContract contract)
        {
            var fieldReplacements = new Dictionary<string, string>();

            // Retrieve the product dates asynchronously.
            var productDetails = await GetProductDetailsByContractIdAsync(contract.ContractID);
            var buyerDetails = await GetContractBuyerAsync(contract.ContractID);
            var sellerDetails = await GetContractSellerAsync(contract.ContractID);
            // orderDetails now returns (string? PaymentMethod, DateTime OrderDate)
            var orderDetails = await GetOrderDetailsAsync(contract.ContractID);
            var orderSummaryData = await GetOrderSummaryInformationAsync(contract.ContractID);
            var deliveryDate = await GetDeliveryDateByContractIdAsync(contract.ContractID);

            if (productDetails.HasValue)
            {
                // Assign nullable DateTime? values
                DateTime? startDate = productDetails.Value.StartDate;
                DateTime? endDate = productDetails.Value.EndDate;

                // Check if both dates have values before calculating period or formatting
                if (startDate.HasValue && endDate.HasValue)
                {
                    var loanPeriod = (endDate.Value - startDate.Value).TotalDays;
                    fieldReplacements["StartDate"] = startDate.Value.ToShortDateString();
                    fieldReplacements["EndDate"] = endDate.Value.ToShortDateString();
                    fieldReplacements["LoanPeriod"] = loanPeriod.ToString();
                    fieldReplacements["AgreementDate"] = startDate.Value.ToShortDateString(); // Assuming AgreementDate is StartDate
                    fieldReplacements["DueDate"] = endDate.Value.ToShortDateString(); // Assuming DueDate is EndDate
                }
                else // Handle cases where one or both dates might be null
                {
                    fieldReplacements["StartDate"] = startDate.HasValue ? startDate.Value.ToShortDateString() : "N/A";
                    fieldReplacements["EndDate"] = endDate.HasValue ? endDate.Value.ToShortDateString() : "N/A";
                    fieldReplacements["LoanPeriod"] = "N/A";
                    fieldReplacements["AgreementDate"] = startDate.HasValue ? startDate.Value.ToShortDateString() : "N/A";
                    fieldReplacements["DueDate"] = endDate.HasValue ? endDate.Value.ToShortDateString() : "N/A";
                }

                // Add other details (assuming price/name are non-null as per repository comment)
                fieldReplacements["ProductDescription"] = productDetails.Value.name;
                fieldReplacements["Price"] = productDetails.Value.price.ToString();
                fieldReplacements["BuyerName"] = buyerDetails.BuyerName;
                fieldReplacements["SellerName"] = sellerDetails.SellerName;
                // Handle potentially null PaymentMethod
                fieldReplacements["PaymentMethod"] = orderDetails.PaymentMethod ?? "N/A"; // Use null-coalescing operator
                // Ensure warrantyTax exists and handle potential conversion errors if necessary
                fieldReplacements["LateFee"] = orderSummaryData.TryGetValue("warrantyTax", out var tax) && tax != null ? tax.ToString() : "N/A";
                // Handle nullable delivery date
                fieldReplacements["DeliveryDate"] = deliveryDate.HasValue ? deliveryDate.Value.ToShortDateString() : "N/A";
            }
            else // Handle case where productDetails itself is null
            {
                fieldReplacements["StartDate"] = "N/A";
                fieldReplacements["EndDate"] = "N/A";
                fieldReplacements["LoanPeriod"] = "N/A";
                fieldReplacements["ProductDescription"] = "N/A";
                fieldReplacements["Price"] = "N/A";
                fieldReplacements["BuyerName"] = buyerDetails.BuyerName; // Consider if these should be fetched even if product details are missing
                fieldReplacements["SellerName"] = sellerDetails.SellerName;
                // Handle potentially null PaymentMethod even if product details are missing
                fieldReplacements["PaymentMethod"] = orderDetails.PaymentMethod ?? "N/A";
                fieldReplacements["LateFee"] = orderSummaryData.TryGetValue("warrantyTax", out var tax) && tax != null ? tax.ToString() : "N/A";
                fieldReplacements["DeliveryDate"] = deliveryDate.HasValue ? deliveryDate.Value.ToShortDateString() : "N/A";
                fieldReplacements["AgreementDate"] = "N/A";
                fieldReplacements["DueDate"] = "N/A";
            }

            return fieldReplacements;
        }

        /// <summary>
        /// Generate and save a contract asynchronously
        /// </summary>
        /// <param name="contract" type="Contract">The contract to generate and save</param>
        /// <param name="contractType" type="PredefinedContractType">The type of the predefined contract</param>
        /// <returns The task></returns>
        public async Task GenerateAndSaveContractAsync()
        {
            IContract contract = new Contract();
            PredefinedContractType contractType = PredefinedContractType.BorrowingContract; // Example contract type
            // Check if the contract is null.
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            var predefinedContract = await GetPredefinedContractByPredefineContractTypeAsync(contractType);

            var fieldReplacements = await GetFieldReplacements(contract);

            // Generate the PDF (synchronously) using the generated replacements.
            var pdfBytes = GenerateContractPdf(contract, predefinedContract, fieldReplacements);

            // Determine the Downloads folder path.
            string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string fileName = $"Contract_{contract.ContractID}.pdf";
            string filePath = System.IO.Path.Combine(downloadsPath, fileName);

            // Save the PDF file asynchronously.
            await File.WriteAllBytesAsync(filePath, pdfBytes);

            // Open the saved PDF file using Windows.Storage and Windows.System APIs.
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            await Launcher.LaunchFileAsync(file);
        }
    }
}
