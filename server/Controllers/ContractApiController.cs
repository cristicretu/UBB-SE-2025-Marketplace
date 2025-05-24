// <copyright file="ContractApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using global::MarketMinds.Shared.Models.DTOs;

    /// <summary>
    /// API controller for managing contract data.
    /// </summary>
    [Route("api/contracts")]
    [ApiController]
    public class ContractApiController : ControllerBase
    {
        private readonly IContractRepository contractRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractApiController"/> class.
        /// </summary>
        /// <param name="contractRepository">The contract repository dependency.</param>
        public ContractApiController(IContractRepository contractRepository)
        {
            this.contractRepository = contractRepository;
        }

        /// <summary>
        /// Asynchronously inserts a new contract and updates the PDF file.
        /// </summary>
        /// <param name="request">The request containing the contract and PDF file.</param>
        /// <returns>The new contract.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(IContract), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IContract>> AddContract([FromBody] AddContractRequest request)
        {
            if (request == null || request.Contract == null || request.PdfFile == null)
            {
                return this.BadRequest("Valid contract and PDF file are required.");
            }

            try
            {
                var newContract = await this.contractRepository.AddContractAsync(request.Contract, request.PdfFile);
                return this.CreatedAtAction(nameof(this.GetContractById), new { contractId = newContract.ContractID }, newContract);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding contract: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves all contracts.
        /// </summary>
        /// <returns>The list of contracts.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<IContract>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<IContract>>> GetAllContracts()
        {
            try
            {
                var contracts = await this.contractRepository.GetAllContractsAsync();
                return this.Ok(contracts);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving all contracts: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves buyer information for a given contract.
        /// </summary>
        /// <param name="contractId">The ID of the contract.</param>
        /// <returns>The buyer information.</returns>
        [HttpGet("{contractId}/buyer")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetContractBuyer(long contractId)
        {
            try
            {
                var buyerInfo = await this.contractRepository.GetContractBuyerAsync(contractId);

                return this.Ok(new { BuyerId = buyerInfo.BuyerID, BuyerName = buyerInfo.BuyerName });
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving buyer info for contract ID {contractId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves a single contract by ID.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve.</param>
        /// <returns>The contract.</returns>
        [HttpGet("{contractId}")]
        [ProducesResponseType(typeof(IContract), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IContract>> GetContractById(long contractId)
        {
            try
            {
                var contract = await this.contractRepository.GetContractByIdAsync(contractId);
                if (contract == null)
                {
                    return this.NotFound($"Contract with ID {contractId} not found.");
                }

                return this.Ok(contract);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving contract with ID {contractId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves the renewal history for a contract.
        /// </summary>
        /// <param name="contractId">The ID of the contract to retrieve the history for.</param>
        /// <returns>The list of contracts in the history.</returns>
        [HttpGet("{contractId}/history")]
        [ProducesResponseType(typeof(List<IContract>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<IContract>>> GetContractHistory(long contractId)
        {
            try
            {
                var history = await this.contractRepository.GetContractHistoryAsync(contractId);
                return this.Ok(history);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving contract history for ID {contractId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves all contracts for a given buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>The list of contracts.</returns>
        [HttpGet("buyer/{buyerId}")]
        [ProducesResponseType(typeof(List<IContract>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<IContract>>> GetContractsByBuyer(int buyerId)
        {
            try
            {
                var contracts = await this.contractRepository.GetContractsByBuyerAsync(buyerId);
                return this.Ok(contracts);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving contracts for buyer ID {buyerId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves seller information for a given contract.
        /// </summary>
        /// <param name="contractId">The ID of the contract.</param>
        /// <returns>The seller information.</returns>
        [HttpGet("{contractId}/seller")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetContractSeller(long contractId)
        {
            try
            {
                var sellerInfo = await this.contractRepository.GetContractSellerAsync(contractId);
                return this.Ok(new { SellerId = sellerInfo.SellerID, SellerName = sellerInfo.SellerName });
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving seller info for contract ID {contractId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves the delivery date for a contract.
        /// </summary>
        /// <param name="contractId">The ID of the contract.</param>
        /// <returns>The delivery date.</returns>
        [HttpGet("{contractId}/delivery-date")]
        [ProducesResponseType(typeof(DateTime?), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DateTime?>> GetDeliveryDateByContractId(long contractId)
        {
            try
            {
                var deliveryDate = await this.contractRepository.GetDeliveryDateByContractIdAsync(contractId);
                return this.Ok(deliveryDate);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving delivery date for contract ID {contractId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves the payment method and order date for a contract.
        /// </summary>
        /// <param name="contractId">The ID of the contract.</param>
        /// <returns>The order details.</returns>
        [HttpGet("{contractId}/order-details")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetOrderDetails(long contractId)
        {
            try
            {
                var orderDetails = await this.contractRepository.GetOrderDetailsAsync(contractId);

                return this.Ok(new { PaymentMethod = orderDetails.PaymentMethod, OrderDate = orderDetails.OrderDate });
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving order details for contract ID {contractId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves order summary information for a contract.
        /// </summary>
        /// <param name="contractId">The ID of the contract.</param>
        /// <returns>The order summary information.</returns>
        [HttpGet("{contractId}/order-summary")]
        [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Dictionary<string, object>>> GetOrderSummaryInformation(long contractId)
        {
            try
            {
                var orderSummary = await this.contractRepository.GetOrderSummaryInformationAsync(contractId);
                return this.Ok(orderSummary);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving order summary for contract ID {contractId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves the PDF file for a contract.
        /// </summary>
        /// <param name="contractId">The ID of the contract.</param>
        /// <returns>The PDF file as a byte array.</returns>
        [HttpGet("{contractId}/pdf")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<byte[]>> GetPdfByContractId(long contractId)
        {
            try
            {
                var pdfFile = await this.contractRepository.GetPdfByContractIdAsync(contractId);
                return this.Ok(pdfFile);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving PDF for contract ID {contractId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously adds a new PDF record.
        /// </summary>
        /// <param name="pdf">The PDF entity to add.</param>
        /// <returns>The added PDF entity with its ID.</returns>
        [HttpPost("pdf")]
        [ProducesResponseType(typeof(PDF), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PDF>> AddPdf([FromBody] PDF pdf)
        {
            if (pdf == null)
            {
                return this.BadRequest("Valid PDF data is required.");
            }

            try
            {
                var addedPdf = await this.contractRepository.AddPdfAsync(pdf);
                return this.CreatedAtAction(nameof(this.GetPdfByContractId), new { contractId = addedPdf.ContractID }, addedPdf);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves a predefined contract by predefined contract type.
        /// </summary>
        /// <param name="predefinedContractType">The type of predefined contract to retrieve.</param>
        /// <returns>The predefined contract.</returns>
        [HttpGet("predefined/{predefinedContractType}")]
        [ProducesResponseType(typeof(IPredefinedContract), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IPredefinedContract>> GetPredefinedContractByPredefineContractType(PredefinedContractType predefinedContractType)
        {
            try
            {
                var contract = await this.contractRepository.GetPredefinedContractByPredefineContractTypeAsync(predefinedContractType);
                return this.Ok(contract);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving predefined contract: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves product details for a contract.
        /// </summary>
        /// <param name="contractId">The ID of the contract.</param>
        /// <returns>The product details.</returns>
        [HttpGet("{contractId}/product-details")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetProductDetailsByContractId(long contractId)
        {
            try
            {
                var productDetails = await this.contractRepository.GetProductDetailsByContractIdAsync(contractId);
                if (productDetails.HasValue)
                {
    
                    return this.Ok(new { 
                        StartDate = productDetails.Value.StartDate,
                        EndDate = productDetails.Value.EndDate,
                        Price = productDetails.Value.price,
                        Name = productDetails.Value.name
                    });
                }
                return this.Ok(null);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving product details for contract ID {contractId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously downloads the PDF file for a contract.
        /// </summary>
        /// <param name="contractId">The ID of the contract.</param>
        /// <returns>The PDF file as a downloadable attachment.</returns>
        [HttpGet("{contractId}/download-pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DownloadContractPdf(long contractId)
        {
            try
            {
                Console.WriteLine($"ContractApiController: Attempting to download PDF for ContractID: {contractId}");
                var pdfFile = await this.contractRepository.GetPdfByContractIdAsync(contractId);

                if (pdfFile == null || pdfFile.Length == 0)
                {
                    Console.WriteLine($"ContractApiController: PDF not found or empty for ContractID: {contractId}. Returning NotFound.");
                    return this.NotFound($"PDF not found for contract ID {contractId}");
                }

                Console.WriteLine($"ContractApiController: PDF found for ContractID: {contractId}. Length: {pdfFile.Length}. Returning FileResult.");
                return this.File(pdfFile, "application/pdf", $"contract_{contractId}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ContractApiController: Error downloading PDF for ContractID: {contractId}. Error: {ex.Message}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while downloading PDF for contract ID {contractId}: {ex.Message}");
            }
        }
    }
}
