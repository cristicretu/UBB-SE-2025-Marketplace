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

        // Add constructor injection for the repository
        public ContractService()
        {
            contractRpository = new ContractProxyRepository(AppConfig.GetBaseApiUrl());
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
                    string contractContent = $@"Contract ID: {contract.ContractID}\nOrder ID: {contract.OrderID}\nStatus: {contract.ContractStatus}\nContent: {contract.ContractContent}\nAdditional Terms: {contract.AdditionalTerms}";
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
