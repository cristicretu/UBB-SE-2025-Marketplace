using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebMarketplace.Controllers
{
    // [Authorize]
    public class RenewContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IPDFService _pdfService;
        private readonly IContractRenewalService _renewalService;
        private readonly INotificationContentService _notificationService;

        public RenewContractController(
            IContractService contractService,
            IPDFService pdfService,
            IContractRenewalService renewalService,
            INotificationContentService notificationService)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
            _renewalService = renewalService ?? throw new ArgumentNullException(nameof(renewalService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<IActionResult> Index()
        {
            int buyerId = await GetCurrentBuyerId();
            var allContracts = await _contractService.GetContractsByBuyerAsync(buyerId);
            var activeContracts = new List<IContract>();

            // Filter for active and renewed contracts
            foreach (var contract in allContracts)
            {
                if (contract.ContractStatus == "ACTIVE" || contract.ContractStatus == "RENEWED")
                {
                    activeContracts.Add(contract);
                }
            }

            ViewBag.BuyerId = buyerId;
            return View(activeContracts);
        }

        [HttpGet]
        public async Task<IActionResult> GetContractDetails(long contractId)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(contractId);
                var details = await _contractService.GetProductDetailsByContractIdAsync(contractId);
                int sellerId = await GetSellerIdByContractId(contractId);

                bool isRenewalPeriodValid = false;

                if (details.HasValue && details.Value.EndDate.HasValue)
                {
                    DateTime oldEndDate = details.Value.EndDate.Value;
                    DateTime currentDate = DateTime.Now.Date;
                    int daysUntilEnd = (oldEndDate - currentDate).Days;
                    isRenewalPeriodValid = daysUntilEnd <= 7 && daysUntilEnd >= 2;
                }

                bool isAlreadyRenewed = await _renewalService.HasContractBeenRenewedAsync(contractId);

                return Json(new
                {
                    success = true,
                    startDate = details?.StartDate?.ToString("MM/dd/yyyy"),
                    endDate = details?.EndDate?.ToString("MM/dd/yyyy"),
                    price = details?.price,
                    name = details?.name,
                    sellerId = sellerId,
                    renewalAllowed = !isAlreadyRenewed && isRenewalPeriodValid,
                    status = isRenewalPeriodValid ? "Available for renewal" : "Not available for renewal",
                    contractContent = contract?.ContractContent
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RenewContract(long contractId, DateTime newEndDate, int buyerId, int productId, int sellerId)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(contractId);
                if (contract == null)
                {
                    return Json(new { success = false, message = "Contract not found" });
                }

                // Check if already renewed
                bool isRenewed = await _renewalService.HasContractBeenRenewedAsync(contractId);
                if (isRenewed)
                {
                    return Json(new { success = false, message = "This contract has already been renewed." });
                }

                // Check validity period
                var details = await _contractService.GetProductDetailsByContractIdAsync(contractId);
                if (!details.HasValue || !details.Value.EndDate.HasValue)
                {
                    return Json(new { success = false, message = "Could not retrieve contract dates." });
                }

                DateTime oldEndDate = details.Value.EndDate.Value;
                DateTime currentDate = DateTime.Now.Date;
                int daysUntilEnd = (oldEndDate - currentDate).Days;
                bool isRenewalPeriodValid = true;

                if (!isRenewalPeriodValid)
                {
                    return Json(new { success = false, message = "Contract is not in a valid renewal period (between 2 and 7 days before end date)." });
                }

                // Check if new end date is after old end date
                if (newEndDate <= oldEndDate)
                {
                    return Json(new { success = false, message = "New end date must be after the current end date." });
                }

                // Check renewal limit
                bool canSellerApprove = contract.RenewalCount < 1;
                if (!canSellerApprove)
                {
                    return Json(new { success = false, message = "Renewal not allowed: seller limit exceeded." });
                }

                // Generate contract content and PDF
                string contractContent = $"Renewed Contract for Order {contract.OrderID}.\nOriginal Contract ID: {contract.ContractID}.\nNew End Date: {newEndDate:dd/MM/yyyy}";

                // For simplicity, we'll create a placeholder for PDF creation in web context
                byte[] pdfContent = System.Text.Encoding.UTF8.GetBytes(contractContent);
                int newPdfId = await _pdfService.InsertPdfAsync(pdfContent);

                // Create renewed contract
                var updatedContract = new Contract
                {
                    OrderID = contract.OrderID,
                    ContractStatus = "RENEWED",
                    ContractContent = contractContent,
                    RenewalCount = contract.RenewalCount + 1,
                    PredefinedContractID = contract.PredefinedContractID,
                    PDFID = newPdfId,
                    RenewedFromContractID = contract.ContractID,
                    AdditionalTerms = contract.AdditionalTerms ?? "Standard renewal terms apply"
                };

                await _renewalService.AddRenewedContractAsync(updatedContract);

                return Json(new { success = true, message = "Contract renewed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        private async Task<int> GetCurrentBuyerId()
        {
            // In a real application with authentication:
            // 1. Get the authenticated user's ID from claims
            // string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // 2. Query the database to find the buyer ID associated with this user

            // For now, we'll query the first contract and get its buyer
            var contracts = await _contractService.GetAllContractsAsync();
            if (contracts != null && contracts.Count > 0)
            {
                var firstContract = contracts[0];
                var buyerDetails = await _contractService.GetContractBuyerAsync(firstContract.ContractID);
                return buyerDetails.BuyerID;
            }

            // Fallback if no contracts exist
            return (int)UserSession.CurrentUserId;
        }

        private async Task<int> GetSellerIdByContractId(long contractId)
        {
            try
            {
                var sellerDetails = await _contractService.GetContractSellerAsync(contractId);
                return sellerDetails.SellerID;
            }
            catch (Exception)
            {
                // Fallback if seller cannot be determined
                return 5;
            }
        }
    }
}
