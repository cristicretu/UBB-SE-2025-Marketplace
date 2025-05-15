using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Helper;
using SharedClassLibrary.Service;
using System;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using NuGet.Packaging;

namespace WebMarketplace.Controllers
{
    public class ContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IPDFService _pdfService;

        public ContractController(IContractService contractService, IPDFService pdfService)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));

            // Set QuestPDF license to Community
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateContract(long contractId)
        {
            try
            {
                // Get the contract
                var contract = await _contractService.GetContractByIdAsync(contractId);
                if (contract == null)
                {
                    TempData["Error"] = "Contract not found";
                    return RedirectToAction("Index", "BuyerProfile");
                }

                // Get the predefined contract template based on order information
                // You may need to determine the contract type based on the order details
                // For now, we're using BorrowingContract as an example
                var predefinedContractType = DetermineContractType(contract);
                var predefinedContract = await _contractService.GetPredefinedContractByPredefineContractTypeAsync(predefinedContractType);

                if (predefinedContract == null || string.IsNullOrEmpty(predefinedContract.ContractContent))
                {
                    TempData["Error"] = "Predefined contract template not found";
                    return RedirectToAction("Index", "BuyerProfile");
                }

                // Generate field replacements
                var fieldReplacements = await GetFieldReplacements(contract);

                // Generate PDF
                var pdfBytes = GenerateContractPdf(contract, predefinedContract, fieldReplacements);

                // Save to downloads folder
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string fileName = $"Contract_{contract.ContractID}.pdf";
                string filePath = Path.Combine(downloadsPath, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                TempData["Success"] = "Contract generated and saved to Downloads folder";

                // Return file for download
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating contract: {ex.Message}";
                return RedirectToAction("Index", "BuyerProfile");
            }
        }
        // Helper method to determine contract type based on order details
        private PredefinedContractType DetermineContractType(IContract contract)
        {
            // Add logic here to determine contract type based on order details
            // For example, check the product type, presence of start/end dates, etc.

            // This is simplified logic - replace with your business rules
            if (contract.ContractContent != null && contract.ContractContent.Contains("borrow"))
                return PredefinedContractType.BorrowingContract;
            else if (contract.ContractContent != null && contract.ContractContent.Contains("purchase"))
                return PredefinedContractType.SellingContract;
            else
                return PredefinedContractType.BuyingContract;
        }
        private byte[] GenerateContractPdf(
                IContract contract,
                IPredefinedContract predefinedContract,
                Dictionary<string, string> fieldReplacements)
        {
            // Validate inputs
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (predefinedContract == null) throw new ArgumentNullException(nameof(predefinedContract));

            // Replace format variables in the content
            string content = predefinedContract.ContractContent;
            foreach (var pair in fieldReplacements)
            {
                content = content.Replace("{" + pair.Key + "}", pair.Value);
            }

            // Replace specific placeholders
            content = content.Replace("{ContractID}", contract.ContractID.ToString());
            content = content.Replace("{OrderID}", contract.OrderID.ToString());
            content = content.Replace("{ContractStatus}", contract.ContractStatus);
            content = content.Replace("{AdditionalTerms}", contract.AdditionalTerms);

            // Generate PDF document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(textStyle => textStyle.FontSize(12).FontFamily("Arial"));

                    // Header section
                    page.Header().Element(header =>
                    {
                        header
                            .PaddingBottom(10)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Column(column =>
                            {
                                column.Item()
                                      .Text("Contract Document")
                                      .SemiBold()
                                      .FontSize(20)
                                      .AlignCenter();
                            });
                    });

                    // Content section
                    page.Content().Element(contentContainer =>
                    {
                        contentContainer
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                // Add a title for the content section
                                column.Item()
                                      .Text("Contract Details")
                                      .FontSize(16)
                                      .Bold()
                                      .Underline();

                                // Add the main content
                                column.Item()
                                      .Text(content)
                                      .FontSize(12);

                                // Add additional details
                                column.Item()
                                      .Text("Additional Information:")
                                      .FontSize(14)
                                      .Bold();

                                column.Item()
                                      .Text($"Contract ID: {contract.ContractID}")
                                      .FontSize(12);

                                column.Item()
                                      .Text($"Order ID: {contract.OrderID}")
                                      .FontSize(12);

                                column.Item()
                                      .Text($"Contract Status: {contract.ContractStatus}")
                                      .FontSize(12);

                                column.Item()
                                      .Text($"Agreement Date: {fieldReplacements.GetValueOrDefault("AgreementDate", "N/A")}")
                                      .FontSize(12);

                                column.Item()
                                      .Text($"Due Date: {fieldReplacements.GetValueOrDefault("DueDate", "N/A")}")
                                      .FontSize(12);

                                column.Item()
                                      .Text($"Buyer: {fieldReplacements.GetValueOrDefault("BuyerName", "N/A")}")
                                      .FontSize(12);

                                column.Item()
                                      .Text($"Seller: {fieldReplacements.GetValueOrDefault("SellerName", "N/A")}")
                                      .FontSize(12);

                                column.Item()
                                      .Text($"Product Description: {fieldReplacements.GetValueOrDefault("ProductDescription", "N/A")}")
                                      .FontSize(12);

                                column.Item()
                                      .Text($"Price: {fieldReplacements.GetValueOrDefault("Price", "N/A")}")
                                      .FontSize(12);

                                column.Item()
                                      .Text($"Payment Method: {fieldReplacements.GetValueOrDefault("PaymentMethod", "N/A")}")
                                      .FontSize(12);

                                column.Item()
                                      .Text($"Delivery Date: {fieldReplacements.GetValueOrDefault("DeliveryDate", "N/A")}")
                                      .FontSize(12);
                            });
                    });

                    // Footer section
                    page.Footer().Element(footer =>
                    {
                        footer
                        .PaddingTop(10)
                        .BorderTop(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Column(column =>
                            column.Item().Row(row =>
                            {
                                row.RelativeItem()
                                   .Text($"Generated on: {DateTime.Now.ToShortDateString()}")
                                   .FontSize(10)
                                   .FontColor(Colors.Grey.Medium);

                                row.ConstantItem(100)
                                   .AlignRight()
                                   .Text(text =>
                                   {
                                       text.DefaultTextStyle(x => x.FontColor(Colors.Grey.Medium).FontSize(10));
                                       text.Span("Page ");
                                       text.CurrentPageNumber();
                                       text.Span(" of ");
                                       text.TotalPages();
                                   });
                            }));
                    });
                });
            });

            return document.GeneratePdf();
        }

        private async Task<Dictionary<string, string>> GetFieldReplacements(IContract contract)
        {
            var fieldReplacements = new Dictionary<string, string>();

            var productDetails = await _contractService.GetProductDetailsByContractIdAsync(contract.ContractID);
            var buyerDetails = await _contractService.GetContractBuyerAsync(contract.ContractID);
            var sellerDetails = await _contractService.GetContractSellerAsync(contract.ContractID);
            var orderDetails = await _contractService.GetOrderDetailsAsync(contract.ContractID);
            var orderSummaryData = await _contractService.GetOrderSummaryInformationAsync(contract.ContractID);
            var deliveryDate = await _contractService.GetDeliveryDateByContractIdAsync(contract.ContractID);

            if (productDetails.HasValue)
            {
                DateTime? startDate = productDetails.Value.StartDate;
                DateTime? endDate = productDetails.Value.EndDate;

                if (startDate.HasValue && endDate.HasValue)
                {
                    var loanPeriod = (endDate.Value - startDate.Value).TotalDays;
                    fieldReplacements["StartDate"] = startDate.Value.ToShortDateString();
                    fieldReplacements["EndDate"] = endDate.Value.ToShortDateString();
                    fieldReplacements["LoanPeriod"] = loanPeriod.ToString();
                    fieldReplacements["AgreementDate"] = startDate.Value.ToShortDateString();
                    fieldReplacements["DueDate"] = endDate.Value.ToShortDateString();
                }
                else
                {
                    fieldReplacements["StartDate"] = startDate.HasValue ? startDate.Value.ToShortDateString() : "N/A";
                    fieldReplacements["EndDate"] = endDate.HasValue ? endDate.Value.ToShortDateString() : "N/A";
                    fieldReplacements["LoanPeriod"] = "N/A";
                    fieldReplacements["AgreementDate"] = startDate.HasValue ? startDate.Value.ToShortDateString() : "N/A";
                    fieldReplacements["DueDate"] = endDate.HasValue ? endDate.Value.ToShortDateString() : "N/A";
                }

                fieldReplacements["ProductDescription"] = productDetails.Value.name;
                fieldReplacements["Price"] = productDetails.Value.price.ToString();
                fieldReplacements["BuyerName"] = buyerDetails.BuyerName;
                fieldReplacements["SellerName"] = sellerDetails.SellerName;
                fieldReplacements["PaymentMethod"] = orderDetails.PaymentMethod ?? "N/A";
                fieldReplacements["LateFee"] = orderSummaryData.TryGetValue("warrantyTax", out var tax) && tax != null ? tax.ToString() : "N/A";
                fieldReplacements["DeliveryDate"] = deliveryDate.HasValue ? deliveryDate.Value.ToShortDateString() : "N/A";
            }
            else
            {
                // Default values if product details not found
                fieldReplacements["StartDate"] = "N/A";
                fieldReplacements["EndDate"] = "N/A";
                fieldReplacements["LoanPeriod"] = "N/A";
                fieldReplacements["ProductDescription"] = "N/A";
                fieldReplacements["Price"] = "N/A";
                fieldReplacements["BuyerName"] = buyerDetails.BuyerName;
                fieldReplacements["SellerName"] = sellerDetails.SellerName;
                fieldReplacements["PaymentMethod"] = orderDetails.PaymentMethod ?? "N/A";
                fieldReplacements["LateFee"] = orderSummaryData.TryGetValue("warrantyTax", out var tax) && tax != null ? tax.ToString() : "N/A";
                fieldReplacements["DeliveryDate"] = deliveryDate.HasValue ? deliveryDate.Value.ToShortDateString() : "N/A";
                fieldReplacements["AgreementDate"] = "N/A";
                fieldReplacements["DueDate"] = "N/A";
            }

            return fieldReplacements;
        }
    }
}
