using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository; // Add this using directive
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Helper;
using MarketMinds.Shared.Services; // For PdfGenerator

namespace MarketMinds.Shared.Services
{
    // Make the class public and implement the interface
    public class ContractService : IContractService
    {
        private readonly IContractRepository contractRpository;
        private readonly IOrderSummaryService _orderSummaryService;
        private readonly IOrderService _orderService;

        // Add constructor injection for the repository
        public ContractService(IOrderSummaryService orderSummaryService, IOrderService orderService)
        {
            contractRpository = new ContractProxyRepository(AppConfig.GetBaseApiUrl());
            _orderSummaryService = orderSummaryService;
            _orderService = orderService;
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

        public async Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            try
            {
                var contract = await contractRpository.GetPredefinedContractByPredefineContractTypeAsync(predefinedContractType);
                if (contract == null)
                {
                    throw new Exception("Contract not found");
                }
                return contract;
            }
            catch (Exception)
            {
                // Return a default contract if none is found
                return new PredefinedContract
                {
                    ContractID = (int)predefinedContractType,
                    ContractContent = $@"DEFAULT {predefinedContractType} AGREEMENT
Contract ID: {{ContractID}}
Order Reference: {{OrderID}}

THIS {predefinedContractType} AGREEMENT (the ""Agreement"") is made and entered into on {{AgreementDate}} (the ""Effective Date""),

BETWEEN:
{{SellerName}} (""Seller""), a registered vendor on the MarketMinds Marketplace,

AND:
{{BuyerName}} (""Buyer""), a registered user on the MarketMinds Marketplace.

PRODUCT DETAILS:
Description: {{ProductDescription}}
Price: ${{Price}}
Subtotal: ${{subtotal}}
Warranty Fee: ${{warrantyTax}}
Delivery Fee: ${{deliveryFee}}
Final Total: ${{finalTotal}}
Payment Method: {{PaymentMethod}}
Expected Delivery Date: {{DeliveryDate}}

1. TERMS AND CONDITIONS
   1.1 This is a default contract template.
   1.2 Please replace this with a proper contract template in the database.

2. PAYMENT
   2.1 The Buyer agrees to pay the Final Total amount stated above.
   2.2 Payment will be processed through the MarketMinds payment system.

3. DELIVERY
   3.1 The Seller agrees to deliver the Product according to the delivery terms specified."
                };
            }
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

        public async Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile)
        {
            try
            {
                // If no PDF file is provided, create a real PDF using QuestPDF
                if (pdfFile == null || pdfFile.Length == 0)
                {
                    string contractTitle = "Contract Agreement";

                    // Fetch necessary data
                    OrderSummary orderSummary = null;
                    MarketMinds.Shared.Models.Order order = null; // Explicitly namespace Order if it clashes

                    if (contract.OrderID > 0) 
                    {
                        try 
                        {
                            orderSummary = await _orderSummaryService.GetOrderSummaryByIdAsync(contract.OrderID);
                            order = await _orderService.GetOrderByIdAsync(contract.OrderID);
                        }
                        catch(Exception ex)
                        {
                            // Log this error, but proceed with defaults for PDF generation if fetching fails
                            System.Diagnostics.Debug.WriteLine($"Error fetching order/summary details for PDF: {ex.Message}");
                        }
                    }

                    // Prepare dynamic values, with defaults if data fetching failed or not applicable
                    string currentContractID = contract.ContractID.ToString(); // Will be 0 if new and ID not yet generated
                    string currentOrderID = contract.OrderID.ToString();
                    string agreementDate = order?.OrderDate.ToString("MM/dd/yyyy") ?? DateTime.Now.ToString("MM/dd/yyyy");
                    string sellerName = "Alexe Razvan"; // Per your example, using a specific name now.
                    string buyerName = orderSummary?.FullName ?? "N/A";
                    string buyerEmailVal = orderSummary?.Email ?? "N/A";
                    string buyerPhoneVal = orderSummary?.PhoneNumber ?? "N/A";
                    string buyerAddressVal = orderSummary?.Address ?? "N/A";
                    string buyerPostalCodeVal = orderSummary?.PostalCode ?? "N/A";
                    string productDescription = orderSummary?.AdditionalInfo ?? contract.ContractContent ?? "Product/Service as per order";
                    string price = orderSummary?.FinalTotal.ToString("F2") ?? "0.00"; // Using FinalTotal as stand-in for Price
                    string subtotalVal = orderSummary?.Subtotal.ToString("F2") ?? "0.00";
                    string warrantyTaxVal = orderSummary?.WarrantyTax.ToString("F2") ?? "0.00";
                    string deliveryFeeVal = orderSummary?.DeliveryFee.ToString("F2") ?? "0.00";
                    string finalTotalVal = orderSummary?.FinalTotal.ToString("F2") ?? "0.00";
                    string paymentMethodVal = order?.PaymentMethod ?? "N/A";
                    string expectedDeliveryDate = order?.OrderDate.AddDays(7).ToString("MM/dd/yyyy") ?? "To be confirmed";
                    string generatedOnDate = DateTime.Now.ToString("MM/dd/yyyy"); // For footer-like text

                    string contractContent = $@"PURCHASE AGREEMENT
Contract ID: {currentContractID}
Order Reference: {currentOrderID}

THIS PURCHASE AGREEMENT (the ""Agreement"") is made and entered into on {agreementDate} (the ""Effective Date""),

BETWEEN:
{sellerName} (""Seller""), a registered vendor on the MarketMinds Marketplace,

AND:
{buyerName} (""Buyer""), a registered user on the MarketMinds Marketplace.

BUYER CONTACT DETAILS:
Full Name: {buyerName}
Email: {buyerEmailVal}
Phone Number: {buyerPhoneVal}
Address: {buyerAddressVal}
Postal Code: {buyerPostalCodeVal}

PRODUCT DETAILS:
Description: {productDescription}
Purchase Price: ${price}
Subtotal: ${subtotalVal}
Warranty Fee: ${warrantyTaxVal}
Delivery Fee: ${deliveryFeeVal}
Final Total: ${finalTotalVal}
Payment Method: {paymentMethodVal}
Expected Delivery Date: {expectedDeliveryDate}

1. PURCHASE AND SALE
1.1 The Seller agrees to sell and the Buyer agrees to purchase the Product described above according to the terms and conditions outlined in this Agreement.
1.2 The Buyer acknowledges having had the opportunity to inspect the Product's description and specifications prior to purchase.


Contract Document

2. PAYMENT
2.1 The Buyer agrees to pay the Final Total amount stated above.
2.2 Payment has been processed through the MarketMinds payment system via the Payment Method indicated above.
2.3 All prices are inclusive of applicable taxes unless otherwise specified.

3. DELIVERY
3.1 The Seller shall arrange for delivery of the Product to the Buyer's specified address.
3.2 The expected delivery date is as specified above, subject to shipping conditions and availability.
3.3 Risk of loss shall transfer to the Buyer upon delivery.

4. WARRANTY
4.1 The Seller warrants that the Product is free from material defects and functions as advertised.
4.2 This warranty is valid for 30 days from the date of delivery.
4.3 The warranty does not cover damage resulting from misuse, accidents, or normal wear and tear.

5. RETURNS AND REFUNDS
5.1 The Buyer may return the Product within 14 days of delivery if it does not meet the specifications advertised.
5.2 Returns must be in original condition with all packaging and accessories.
5.3 Refunds will be processed within 10 business days of the Seller receiving the returned Product.


Contract Document

6. LIMITATION OF LIABILITY
6.1 The Seller's liability is limited to the Purchase Price of the Product.
6.2 Neither party shall be liable for indirect, special, or consequential damages.

7. GOVERNING LAW
7.1 This Agreement shall be governed by and construed in accordance with the laws of the jurisdiction in which the Seller is based.

8. ADDITIONAL TERMS
The parties hereby indicate their acceptance of this Agreement by their signatures below or by their electronic acceptance through the MarketMinds platform.

Agreement Status: APPROVED

SELLER: {sellerName}
BUYER: {buyerName}
DATE: {agreementDate}

Generated on: {generatedOnDate} Page 3 of 3
";
                    pdfFile = PdfGenerator.GenerateContractPdf(contractTitle, contractContent);
                }

                // Create a PDF record first
                var pdf = new PDF
                {
                    File = pdfFile
                };

                // Add the PDF to get its ID
                var pdfResponse = await contractRpository.AddPdfAsync(pdf);
                if (pdfResponse == null)
                {
                    throw new Exception("Failed to create PDF record");
                }

                // Set the PDFID on the contract
                contract.PDFID = pdfResponse.PdfID;

                // Now add the contract
                return await contractRpository.AddContractAsync(contract, pdfFile);
            }
            catch (Exception ex)
            {
                throw new Exception($"AddContractAsync: {ex.Message}", ex);
            }
        }
    }
}
