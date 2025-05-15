using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class UpdateProfileController : Controller
    {
        private readonly ISellerService _sellerService;
        private readonly IUserService _userService;

        public UpdateProfileController(ISellerService sellerService, IUserService userService)
        {
            _sellerService = sellerService;
            _userService = userService;
        }

        /// <summary>
        /// Gets the current user ID (placeholder - would be replaced with actual authentication)
        /// </summary>
        /// <returns>The current user ID</returns>
        private int GetCurrentUserId()
        {
            // This would be replaced with actual user authentication 
            return UserSession.CurrentUserId ?? 1;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            try
            {
                var userId = GetCurrentUserId();
                var user = new SharedClassLibrary.Domain.User(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                Seller currentSeller = await _sellerService.GetSellerByUser(user);
                if (currentSeller == null)
                {
                    return RedirectToAction("Create", "Seller");
                }

                // Create a view model with the seller's data
                var model = new UpdateProfileViewModel
                {
                    Username = currentSeller.Username,
                    StoreName = currentSeller.StoreName,
                    Email = currentSeller.Email,
                    PhoneNumber = currentSeller.PhoneNumber,
                    Address = currentSeller.StoreAddress,
                    Description = currentSeller.StoreDescription
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(UpdateProfileViewModel model)
        {
            List<string> validationErrors = ValidateFields(model);
            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                var user = new SharedClassLibrary.Domain.User(userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                Seller currentSeller = await _sellerService.GetSellerByUser(user);

                if (currentSeller == null)
                {
                    return NotFound("Seller profile not found");
                }

                // Update seller information
                currentSeller.StoreName = model.StoreName;
                currentSeller.StoreAddress = model.Address;
                currentSeller.StoreDescription = model.Description;
                currentSeller.User.Username = model.Username;
                currentSeller.User.Email = model.Email;
                currentSeller.User.PhoneNumber = model.PhoneNumber;

                await _sellerService.UpdateSeller(currentSeller);

                // Clear error messages from model since the update was successful
                model.StoreNameError = null;
                model.EmailError = null;
                model.PhoneNumberError = null;
                model.AddressError = null;
                model.DescriptionError = null;

                // Add success flag to ViewData to show modal
                ViewData["ShowSuccessModal"] = true;

                // Clear ModelState to prevent error messages from appearing
                ModelState.Clear();

                // Return to the same page with updated model
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating profile: {ex.Message}");
                return View(model);
            }
        }


        private List<string> ValidateFields(UpdateProfileViewModel model)
        {
            List<string> errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(model.StoreName))
            {
                errorMessages.Add("Store name is required.");
                model.StoreNameError = "Store name is required.";
            }

            if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains('@'))
            {
                errorMessages.Add("Valid email is required.");
                model.EmailError = "Valid email is required.";
            }

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                errorMessages.Add("Phone number is required.");
                model.PhoneNumberError = "Phone number is required.";
            }

            if (string.IsNullOrWhiteSpace(model.Address))
            {
                errorMessages.Add("Address is required.");
                model.AddressError = "Address is required.";
            }

            if (string.IsNullOrWhiteSpace(model.Description))
            {
                errorMessages.Add("Description is required.");
                model.DescriptionError = "Description is required.";
            }

            return errorMessages;
        }

    }
}
