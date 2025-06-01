using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace MarketMinds.Web.Controllers
{
    /// <summary>
    /// Controller for managing wallet operations
    /// </summary>
    public class WalletController : Controller
    {
        private readonly IDummyWalletService _dummyWalletService;

        public WalletController(IDummyWalletService dummyWalletService)
        {
            _dummyWalletService = dummyWalletService;
        }

        /// <summary>
        /// Gets the current user ID from authentication claims
        /// </summary>
        /// <returns>The current user ID or 0 if not authenticated</returns>
        private int GetCurrentUserId()
        {
            // Get the user ID from claims (proper authentication approach)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            // Try custom claim as fallback
            var customIdClaim = User.FindFirst("UserId");
            if (customIdClaim != null && int.TryParse(customIdClaim.Value, out int customUserId))
            {
                return customUserId;
            }

            // Fallback to UserSession (for backward compatibility)
            if (UserSession.CurrentUserId.HasValue)
            {
                return UserSession.CurrentUserId.Value;
            }

            // If no authentication found, return 0 to indicate unauthorized
            return 0;
        }

        /// <summary>
        /// Adds funds to the user's wallet
        /// </summary>
        /// <param name="amount">The amount to add to the wallet</param>
        /// <returns>Redirect to BuyerProfile with success/error message</returns>
        [HttpPost]
        public async Task<IActionResult> AddFunds(double amount)
        {
            try
            {
                // Validate user authentication
                int userId = GetCurrentUserId();
                if (userId == 0)
                {
                    TempData["ErrorMessage"] = "User not authenticated. Please log in again.";
                    return RedirectToAction("Index", "BuyerProfile");
                }

                // Validate amount
                if (amount <= 0)
                {
                    TempData["ErrorMessage"] = "Please enter a valid amount greater than €0.00.";
                    return RedirectToAction("Index", "BuyerProfile");
                }

                if (amount > 10000)
                {
                    TempData["ErrorMessage"] = "Maximum amount allowed is €10,000.00.";
                    return RedirectToAction("Index", "BuyerProfile");
                }

                // Get current wallet balance
                double currentBalance = await _dummyWalletService.GetWalletBalanceAsync(userId);

                // Calculate new balance
                double newBalance = currentBalance + amount;

                // Check if new balance would exceed reasonable limits (optional safety check)
                if (newBalance > 100000) // Maximum wallet balance of €100,000
                {
                    TempData["ErrorMessage"] = "Adding this amount would exceed the maximum wallet balance of €100,000.00.";
                    return RedirectToAction("Index", "BuyerProfile");
                }

                // Update wallet balance
                await _dummyWalletService.UpdateWalletBalance(userId, newBalance);

                // Set success message
                TempData["SuccessMessage"] = $"Successfully added €{amount:F2} to your wallet. New balance: €{newBalance:F2}";

                System.Diagnostics.Debug.WriteLine($"Wallet funds added successfully. User: {userId}, Amount: €{amount:F2}, New Balance: €{newBalance:F2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding funds to wallet: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while adding funds to your wallet. Please try again.";
            }

            return RedirectToAction("Index", "BuyerProfile");
        }

        /// <summary>
        /// API endpoint to get the current user's wallet balance
        /// </summary>
        /// <returns>JSON with wallet balance information</returns>
        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var walletBalance = await _dummyWalletService.GetWalletBalanceAsync(userId);

                return Json(new
                {
                    success = true,
                    balance = walletBalance,
                    formattedBalance = $"{walletBalance:F2} €"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error retrieving wallet balance: {ex.Message}" });
            }
        }
    }
}