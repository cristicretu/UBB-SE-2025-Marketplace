// <copyright file="ContractRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Represents a repository for contract operations.
    /// </summary>
    public class ContractRepository : IContractRepository
    {
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public ContractRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Asynchronously retrieves a predefined contract by predefined contract type using the GetPredefinedContractByID stored procedure.
        /// </summary>
        /// <param name="predefinedContractType">The type of predefined contract to retrieve.</param>
        /// <returns>The predefined contract.</returns>
        /// <exception cref="Exception">Thrown when the predefined contract is not found.</exception>
        public async Task<IPredefinedContract> GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType predefinedContractType)
        {
            PredefinedContract? predefinedContract = await this.dbContext.PredefinedContracts
                .Where(predefinedContract => predefinedContract.ContractID == (int)predefinedContractType)
                .FirstOrDefaultAsync() ?? throw new Exception("GetPredefinedContractByPredefineContractTypeAsync: Predefined contract not found for predefined contract type: " + predefinedContractType);

            return predefinedContract;
        }

        /// <summary>
        /// Asynchronously retrieves a single contract using the GetContractByID stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve.</param>
        /// <returns>The contract.</returns>
        /// <exception cref="Exception">Thrown when the contract is not found.</exception>
        public async Task<IContract> GetContractByIdAsync(long contractId)
        {
            Contract? contract = await this.dbContext.Contracts
                .Where(contract => contract.ContractID == contractId)
                .FirstOrDefaultAsync() ?? throw new Exception("GetContractByIdAsync: Contract not found for contract ID: " + contractId);

            return contract;
        }

        /// <summary>
        /// Asynchronously retrieves all contracts using the GetAllContracts stored procedure.
        /// </summary>
        /// <returns>The list of contracts.</returns>
        public async Task<List<IContract>> GetAllContractsAsync()
        {
            List<Contract> contracts = await this.dbContext.Contracts.ToListAsync();

            return contracts.Cast<IContract>().ToList();
        }

        /// <summary>
        /// Asynchronously retrieves the renewal history for a contract using the GetContractHistory stored procedure.
        /// OBS: ContractHistory table does not exist in the database
        /// I will assume that I should take all contracts as long as I have an existing RenewedFromContractID
        /// Thus I should go back in time until I reach a contract with no RenewedFromContractID
        /// and return all those contracts in a list, which will be in REVERSE chronological order.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the history for.</param>
        /// <returns>The list of contracts.</returns>
        /// <exception cref="Exception">Thrown when the contract is not found.</exception>
        public async Task<List<IContract>> GetContractHistoryAsync(long contractId)
        {
            List<IContract> contractHistory = new List<IContract>();

            // Get the contract with the given contract ID
            Contract? contract = await this.dbContext.Contracts
                .Where(contract => contract.ContractID == contractId)
                .FirstOrDefaultAsync() ?? throw new Exception("GetContractHistoryAsync: Contract not found for contract ID: " + contractId);

            // Go back in time until I reach a contract with no RenewedFromContractID
            while (contract.RenewedFromContractID != null)
            {
                contractHistory.Add(contract);

                // Fetch the *previous* contract and reassign the contract variable
                // This contract will now refer to this *new* object instance of a contract
                contract = await this.dbContext.Contracts
                    .Where(previousContract => previousContract.ContractID == contract.RenewedFromContractID)
                    .FirstOrDefaultAsync() ?? throw new Exception("GetContractHistoryAsync: Contract not found for contract ID: " + contract.RenewedFromContractID);
            }

            // Add the final contract (the one with no RenewedFromContractID) to the history as well.
            contractHistory.Add(contract);

            return contractHistory;
        }

        /// <summary>
        /// Asynchronously inserts a new contract and updates the PDF file using the AddContract stored procedure.
        /// </summary>
        /// <param name="contract">The contract to insert.</param>
        /// <param name="pdfFile">The PDF file to update.</param>
        /// <returns>The new contract.</returns>
        public async Task<IContract> AddContractAsync(IContract contract, byte[] pdfFile)
        {
            bool pdfExists = await this.dbContext.PDFs
                .AnyAsync(pdf => pdf.PdfID == contract.PDFID);

            PDF pdfToUpdateOrInsert = new PDF
            {
                PdfID = contract.PDFID,
                File = pdfFile,
                ContractID = (int)contract.ContractID, // not used in the database but needed for the constructor
            };

            if (pdfExists) // if the pdf exists, update the pdf file
            {
                this.dbContext.PDFs.Update(pdfToUpdateOrInsert);
            }
            else // if the pdf does not exist, insert a new pdf file
            {
                pdfToUpdateOrInsert.PdfID = 0; // set the id to 0 to avoid conflict with the existing pdf
                this.dbContext.PDFs.Add(pdfToUpdateOrInsert);
            }

            // Create the entity to be added and tracked by Entity Framework
            Contract contractToAdd = contract.ToContract();

            // Ensure the correct PDFID is set on the entity being added
            contractToAdd.PDFID = pdfToUpdateOrInsert.PdfID;

            // Add the entity to the DbContext
            this.dbContext.Contracts.Add(contractToAdd);

            // Save changes, which generates the ContractID and updates contractToAdd and also updated the PDFID on the contractToAdd if needed (MAGIC)
            await this.dbContext.SaveChangesAsync();

            // Return the tracked entity, which now contains the database-generated ID
            return contractToAdd;
        }

        /// <summary>
        /// Asynchronously retrieves seller information for a given contract using the GetContractSeller stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the seller information for.</param>
        /// <returns>The seller information.</returns>
        /// <exception cref="Exception">Thrown when the contract, order or seller is not found.</exception>
        public async Task<(int SellerID, string SellerName)> GetContractSellerAsync(long contractId)
        {
            // Get the contract for the given contract ID
            Contract? contract = await this.dbContext.Contracts
                .Where(contract => contract.ContractID == contractId)
                .FirstOrDefaultAsync() ?? throw new Exception("GetContractSellerAsync: Contract not found for contract ID: " + contractId);

            // Get the order for the contract
            Order? order = await this.dbContext.Orders
                .Where(order => order.OrderID == contract.OrderID)
                .FirstOrDefaultAsync() ?? throw new Exception("GetContractSellerAsync: Order not found for order ID: " + contract.OrderID);

            // Get the product for the order
            Product? product = await this.dbContext.Products
                .Where(product => product.ProductId == order.ProductID)
                .FirstOrDefaultAsync() ?? throw new Exception("GetContractSellerAsync: Product not found for product ID: " + order.ProductID);

            // Get the seller for the product
            Seller? seller = await this.dbContext.Sellers
                .Where(seller => seller.Id == product.SellerId)
                .FirstOrDefaultAsync() ?? throw new Exception("GetContractSellerAsync: Seller not found for seller ID: " + product.SellerId);

            // return seller; // this could be done in the future, I did not do it now because I don't want to change the function signature / repo interface -Alex
            // Alternative return values, I don't know which one should be returned -Alex
            // return (seller.Id, seller.Username);
            // return (seller.Id, seller.Username + " " + seller.StoreName);
            return (seller.Id, seller.StoreName);
        }

        /// <summary>
        /// Asynchronously retrieves buyer information for a given contract using the GetContractBuyer stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the buyer information for.</param>
        /// <returns>The buyer information.</returns>
        /// <exception cref="Exception">Thrown when the contract, order or buyer is not found.</exception>
        public async Task<(int BuyerID, string BuyerName)> GetContractBuyerAsync(long contractId)
        {
            // Get the contract for the given contract ID
            Contract? contract = await this.dbContext.Contracts
                .Where(contract => contract.ContractID == contractId)
                .FirstOrDefaultAsync() ?? throw new Exception("GetContractBuyerAsync: Contract not found for contract ID: " + contractId);

            // Get the order for the contract
            Order? order = await this.dbContext.Orders
                .Where(order => order.OrderID == contract.OrderID)
                .FirstOrDefaultAsync() ?? throw new Exception("GetContractBuyerAsync: Order not found for order ID: " + contract.OrderID);

            // Get the buyer for the order
            Buyer? buyer = await this.dbContext.Buyers
                .Where(buyer => buyer.Id == order.BuyerID)
                .FirstOrDefaultAsync() ?? throw new Exception("GetContractBuyerAsync: Buyer not found for buyer ID: " + order.BuyerID);

            // return buyer; // this could be done in the future, I did not do it now because I don't want to change the function signature / repo interface -Alex
            return (buyer.Id, buyer.FirstName + " " + buyer.LastName);
        }

        /// <summary>
        /// Asynchronously retrieves order summary information for a contract using the GetOrderSummaryInformation stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the order summary information for.</param>
        /// <returns>The order summary information.</returns>
        /// <exception cref="Exception">Thrown when the contract, order or order summary is not found.</exception>
        public async Task<Dictionary<string, object>> GetOrderSummaryInformationAsync(long contractId)
        {
            Dictionary<string, object> orderSummary = new Dictionary<string, object>();

            // Get the contract for the given contract ID
            Contract? contract = await this.dbContext.Contracts
                .Where(contract => contract.ContractID == contractId)
                .FirstOrDefaultAsync() ?? throw new Exception("GetOrderSummaryInformationAsync: Contract not found for contract ID: " + contractId);

            // Get the order for the contract
            Order? order = await this.dbContext.Orders
                .Where(order => order.OrderID == contract.OrderID)
                .FirstOrDefaultAsync() ?? throw new Exception("GetOrderSummaryInformationAsync: Order not found for order ID: " + contract.OrderID);

            // Get the OrderSummary for the order
            OrderSummary? orderSummaryDb = await this.dbContext.OrderSummary
                .Where(orderSummary => orderSummary.ID == order.OrderSummaryID)
                .FirstOrDefaultAsync() ?? throw new Exception("GetOrderSummaryInformationAsync: Order summary not found for order ID: " + order.OrderID);

            // Populate the order summary dictionary
            // Optional fields can be null, so we need to check for null and return the default value
            orderSummary["ID"] = orderSummaryDb.ID;
            orderSummary["subtotal"] = orderSummaryDb.Subtotal;
            orderSummary["warrantyTax"] = orderSummaryDb.WarrantyTax;
            orderSummary["deliveryFee"] = orderSummaryDb.DeliveryFee;
            orderSummary["finalTotal"] = orderSummaryDb.FinalTotal;
            orderSummary["fullName"] = orderSummaryDb.FullName ?? string.Empty;
            orderSummary["email"] = orderSummaryDb.Email ?? string.Empty;
            orderSummary["phoneNumber"] = orderSummaryDb.PhoneNumber ?? string.Empty;
            orderSummary["address"] = orderSummaryDb.Address ?? string.Empty;
            orderSummary["postalCode"] = orderSummaryDb.PostalCode ?? string.Empty;
            orderSummary["additionalInfo"] = orderSummaryDb.AdditionalInfo ?? string.Empty;
            orderSummary["ContractDetails"] = orderSummaryDb.ContractDetails ?? string.Empty;

            return orderSummary;
        }

        /// <summary>
        /// Asynchronously retrieves the startDate and endDate for a contract from the Product table.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the product details for.</param>
        /// <returns>The product details.</returns>
        /// <exception cref="Exception">Thrown when the contract, order or product is not found.</exception>
        public async Task<(DateTime? StartDate, DateTime? EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            // Get the contract for the given contract ID
            Contract? contract = await this.dbContext.Contracts
                .Where(contract => contract.ContractID == contractId)
                .FirstOrDefaultAsync() ?? throw new Exception("GetProductDetailsByContractIdAsync: Contract not found for contract ID: " + contractId);

            // Get the order of the contract
            Order? order = await this.dbContext.Orders
                .Where(order => order.OrderID == contract.OrderID)
                .FirstOrDefaultAsync() ?? throw new Exception("GetProductDetailsByContractIdAsync: Order not found for order ID: " + contract.OrderID);

            // Get the product of the order
            Product? product = await this.dbContext.Products
                .Where(product => product.ProductId == order.ProductID)
                .FirstOrDefaultAsync() ?? throw new Exception("GetProductDetailsByContractIdAsync: Product not found for product ID: " + order.ProductID);

            // Return the product details
            return (product.StartDate?.DateTime, product.EndDate?.DateTime, product.Price, product.Name);
        }

        /// <summary>
        /// Asynchronously retrieves all contracts for a given buyer using the GetContractsByBuyer stored procedure.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer to retrieve the contracts for.</param>
        /// <returns>The list of contracts.</returns>
        public async Task<List<IContract>> GetContractsByBuyerAsync(int buyerId)
        {
            List<Contract> allContracts = new List<Contract>();

            // Get all orders for the buyer
            List<Order> orders = await this.dbContext.Orders
                .Where(order => order.BuyerID == buyerId)
                .ToListAsync();

            // For each order get all of its contracts
            // Relationship is one to many, that's why we have a list of contracts for each order
            foreach (Order order in orders)
            {
                List<Contract> contracts = await this.dbContext.Contracts
                    .Where(contract => contract.OrderID == order.OrderID)
                    .ToListAsync();

                allContracts.AddRange(contracts); // Add the contracts to the list of all contracts
            }

            return allContracts.Cast<IContract>().ToList(); // Cast the list of contracts to a list of IContract
        }

        /// <summary>
        /// Asynchronously retrieves the payment method and order date for a given contract using the GetOrderDetails stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the order details for.</param>
        /// <returns>The order details, with PaymentMethod potentially null.</returns>
        /// <exception cref="Exception">Thrown when the contract or order is not found.</exception>
        public async Task<(string PaymentMethod, DateTime OrderDate)> GetOrderDetailsAsync(long contractId)
        {
            Contract? contract = await this.dbContext.Contracts
                .Where(contract => contract.ContractID == contractId)
                .FirstOrDefaultAsync() ?? throw new Exception("GetOrderDetailsAsync: Contract not found for contract ID: " + contractId);

            Order? order = await this.dbContext.Orders
                .Where(order => order.OrderID == contract.OrderID)
                .FirstOrDefaultAsync() ?? throw new Exception("GetOrderDetailsAsync: Order not found for order ID: " + contract.OrderID);

            return (order.PaymentMethod, order.OrderDate.DateTime);
        }

        /// <summary>
        /// Asynchronously retrieves the delivery date for a given contract using the GetDeliveryDateByContractID stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the delivery date for.</param>
        /// <returns>The delivery date.</returns>
        /// <exception cref="Exception">Thrown when the contract or tracked order is not found.</exception>
        public async Task<DateTime?> GetDeliveryDateByContractIdAsync(long contractId)
        {
            Contract? contract = await this.dbContext.Contracts
                .Where(contract => contract.ContractID == contractId)
                .FirstOrDefaultAsync() ?? throw new Exception("GetDeliveryDateByContractIdAsync: Contract not found for contract ID: " + contractId);

            TrackedOrder? trackedOrder = await this.dbContext.TrackedOrders
                .Where(trackedOrder => trackedOrder.OrderID == contract.OrderID)
                .FirstOrDefaultAsync() ?? throw new Exception("GetDeliveryDateByContractIdAsync: Tracked order not found for order ID: " + contract.OrderID);

            return trackedOrder.EstimatedDeliveryDate.ToDateTime(TimeOnly.MinValue);
        }

        /// <summary>
        /// Asynchronously retrieves the PDF file for a given contract using the GetPdfByContractID stored procedure.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the PDF file for.</param>
        /// <returns>The PDF file.</returns>
        /// <exception cref="Exception">Thrown when the PDF is not found.</exception>
        public async Task<byte[]> GetPdfByContractIdAsync(long contractId)
        {
            int pdfId = await this.dbContext.PDFs
                .Where(pdf => pdf.ContractID == contractId)
                .Select(pdf => pdf.PdfID)
                .FirstOrDefaultAsync();

            if (pdfId == 0) // int cannot be null, so it will be 0 if not found
            {
                throw new Exception("GetPdfByContractIdAsync: PDF not found for contract ID: " + contractId);
            }

            return await this.dbContext.PDFs
                .Where(pdf => pdf.PdfID == pdfId)
                .Select(pdf => pdf.File)
                .FirstOrDefaultAsync() ?? throw new Exception("GetPdfByContractIdAsync: PDF file not found for PDF ID: " + pdfId);
        }
    }
}