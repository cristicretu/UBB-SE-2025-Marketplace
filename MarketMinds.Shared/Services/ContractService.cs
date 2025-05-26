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
        private readonly ISellerService _sellerService; // Added for fetching seller name
        private readonly IPDFService _pdfService; // Added for direct PDF insertion

        // Add constructor injection for the repository
        public ContractService(
            IContractRepository contractRepository, 
            IOrderSummaryService orderSummaryService, 
            IOrderService orderService, 
            ISellerService sellerService,
            IPDFService pdfService) // Modified constructor
        {
            this.contractRpository = contractRepository;
            _orderSummaryService = orderSummaryService;
            _orderService = orderService;
            _sellerService = sellerService; // Store injected ISellerService
            _pdfService = pdfService; // Store injected IPDFService
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
                Contract contractEntity = contract.ToContract(); // Work with a concrete instance

                if (pdfFile == null || pdfFile.Length == 0) // Logic for generating new rich PDF
                {
                    // Step A: Generate PDF content first
                    string contractTitle = "Contract Document";
                    string currentOrderID = contractEntity.OrderID.ToString();
                    string agreementDate = DateTime.Now.ToString("MM/dd/yyyy");
                    
                    // Get order and summary details
                    OrderSummary orderSummary = null;
                    MarketMinds.Shared.Models.Order order = null;
                    string actualSellerName = "MarketMinds Marketplace Vendor";

                    if (contractEntity.OrderID > 0)
                    {
                        try
                        {
                            orderSummary = await _orderSummaryService.GetOrderSummaryByIdAsync(contractEntity.OrderID);
                            order = await _orderService.GetOrderByIdAsync(contractEntity.OrderID);
                            
                            if (order != null && order.SellerId > 0)
                            {
                                var seller = await _sellerService.GetSellerByIdAsync(order.SellerId);
                                if (seller != null)
                                {
                                    actualSellerName = seller.StoreName ?? seller.Username ?? actualSellerName;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error fetching order/summary/seller details for PDF: {ex.Message}");
                        }
                    }

                    string buyerName = orderSummary?.FullName ?? "N/A";
                    string buyerEmailVal = orderSummary?.Email ?? "N/A";
                    string buyerPhoneVal = orderSummary?.PhoneNumber ?? "N/A";
                    string buyerAddressVal = orderSummary?.Address ?? "N/A";
                    string buyerPostalCodeVal = orderSummary?.PostalCode ?? "N/A";
                    string productDescription = orderSummary?.AdditionalInfo ?? contractEntity.ContractContent ?? "Product/Service as per order";
                    string price = orderSummary?.FinalTotal.ToString("F2") ?? "0.00";
                    string subtotalVal = orderSummary?.Subtotal.ToString("F2") ?? "0.00";
                    string warrantyTaxVal = orderSummary?.WarrantyTax.ToString("F2") ?? "0.00";
                    string deliveryFeeVal = orderSummary?.DeliveryFee.ToString("F2") ?? "0.00";
                    string finalTotalVal = orderSummary?.FinalTotal.ToString("F2") ?? "0.00";
                    string paymentMethodVal = order?.PaymentMethod ?? "N/A";
                    string expectedDeliveryDate = order?.OrderDate.AddDays(7).ToString("MM/dd/yyyy") ?? "To be confirmed";
                    string generatedOnDate = DateTime.Now.ToString("MM/dd/yyyy");

                    string contractContent = $@"PURCHASE AGREEMENT
Order Reference: {currentOrderID}

THIS PURCHASE AGREEMENT (the ""Agreement"") is made and entered into on {agreementDate} (the ""Effective Date""),

BETWEEN:
{actualSellerName} (""Seller""), a registered vendor on the MarketMinds Marketplace,

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

6. LIMITATION OF LIABILITY
6.1 The Seller's liability is limited to the Purchase Price of the Product.
6.2 Neither party shall be liable for indirect, special, or consequential damages.

7. GOVERNING LAW
7.1 This Agreement shall be governed by and construed in accordance with the laws of the jurisdiction in which the Seller is based.

8. ADDITIONAL TERMS
The parties hereby indicate their acceptance of this Agreement by their signatures below or by their electronic acceptance through the MarketMinds platform.

Agreement Status: APPROVED

SELLER: {actualSellerName}
BUYER: {buyerName}
DATE: {agreementDate}

Generated on: {generatedOnDate}";

                    // Step B: Generate PDF and get its ID
                    byte[] generatedPdfBytes = PdfGenerator.GenerateContractPdf(contractTitle, contractContent);
                    int newPdfId = await _pdfService.InsertPdfAsync(generatedPdfBytes);
                    if (newPdfId == 0)
                    {
                        throw new Exception("Failed to generate and save PDF record or retrieve its ID.");
                    }

                    // Step C: Create contract with the valid PDF ID
                    contractEntity.PDFID = newPdfId;
                    contractEntity.ContractContent = contractContent;
                    
                    // Add the contract with the valid PDF ID
                    return await contractRpository.AddContractAsync(contractEntity, generatedPdfBytes);
                }
                else // pdfFile IS provided
                {
                    // Create a PDF record first
                    int linkedPdfId = await _pdfService.InsertPdfAsync(pdfFile);
                    if (linkedPdfId == 0)
                    {
                        throw new Exception("Failed to save provided PDF file using PDFService.");
                    }
                    
                    // Set the PDF ID on the contract
                    contractEntity.PDFID = linkedPdfId;
                    
                    // Add the contract with the valid PDF ID
                    return await contractRpository.AddContractAsync(contractEntity, pdfFile);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddContractAsync: {ex.ToString()}");
                throw new Exception($"AddContractAsync: {ex.Message}", ex);
            }
        }
    }
}
