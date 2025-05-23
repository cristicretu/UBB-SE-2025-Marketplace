using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using System.Diagnostics;
using WebMarketplace.Models;
using MarketMinds.Shared.Services.UserService;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebMarketplace.Controllers
{
    /// <summary>
    /// Controller for buyer profile operations with improved stability
    /// </summary>
    public class BuyerProfileController : Controller
    {
        private readonly IBuyerService _buyerService;
        private readonly IUserService _userService;
        private readonly IBuyerLinkageService _buyerLinkageService;
        private readonly ILogger<BuyerProfileController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerProfileController"/> class.
        /// </summary>
        /// <param name="buyerService">The buyer service.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="buyerLinkageService">The buyer linkage service.</param>
        /// <param name="logger">The logger.</param>
        public BuyerProfileController(
            IBuyerService buyerService,
            IUserService userService,
            IBuyerLinkageService buyerLinkageService,
            ILogger<BuyerProfileController> logger)
        {
            _buyerService = buyerService ?? throw new ArgumentNullException(nameof(buyerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _buyerLinkageService = buyerLinkageService ?? throw new ArgumentNullException(nameof(buyerLinkageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _logger.LogInformation("CONSTRUCTOR: BuyerProfileController initialized");
        }

        /// <summary>
        /// Gets the current user ID from authentication claims
        /// </summary>
        /// <returns>The current user ID</returns>
        private int GetCurrentUserId()
        {
            _logger.LogInformation("DEBUG: GetCurrentUserId() called");
            
            // Get the user ID from claims (proper authentication approach)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogInformation("DEBUG: Got user ID {UserId} from NameIdentifier claim", userId);
                return userId;
            }

            // Try custom claim as fallback
            var customIdClaim = User.FindFirst("UserId");
            if (customIdClaim != null && int.TryParse(customIdClaim.Value, out int customUserId))
            {
                _logger.LogInformation("DEBUG: Got user ID {UserId} from custom UserId claim", customUserId);
                return customUserId;
            }

            // Fallback to UserSession (for backward compatibility)
            if (UserSession.CurrentUserId.HasValue)
            {
                _logger.LogWarning("DEBUG: Falling back to UserSession.CurrentUserId: {UserId}", UserSession.CurrentUserId.Value);
                return UserSession.CurrentUserId.Value;
            }

            // If no authentication found, return 0 to indicate unauthorized
            _logger.LogWarning("DEBUG: No valid user ID found in claims or session");
            return 0;
        }

        /// <summary>
        /// Loads linked buyers information for a specific buyer
        /// </summary>
        /// <param name="buyerId">The buyer ID to get linked buyers for</param>
        /// <returns>List of linked buyer information</returns>
        private async Task<List<LinkedBuyerInfo>> LoadLinkedBuyersAsync(int buyerId)
        {
            try
            {
                _logger.LogInformation("Loading linked buyers for buyer ID: {BuyerId}", buyerId);

                // Get linked buyers directly from the service
                var linkedBuyers = await _buyerLinkageService.GetLinkedBuyersAsync(buyerId);
                
                var linkedBuyerInfoList = new List<LinkedBuyerInfo>();

                foreach (var linkedBuyer in linkedBuyers)
                {
                    try
                    {
                        var linkedBuyerInfo = new LinkedBuyerInfo
                        {
                            BuyerId = linkedBuyer.Id,
                            FirstName = linkedBuyer.FirstName ?? "Unknown",
                            LastName = linkedBuyer.LastName ?? "User",
                            Email = linkedBuyer.User?.Email ?? string.Empty,
                            Badge = linkedBuyer.Badge.ToString() ?? "None",
                            LinkedDate = DateTime.UtcNow // Default for now since we don't have this in the old system
                        };
                        linkedBuyerInfoList.Add(linkedBuyerInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process linked buyer {LinkedBuyerId} for buyer {BuyerId}", linkedBuyer.Id, buyerId);
                        // Continue with other linked buyers
                    }
                }

                _logger.LogInformation("Loaded {Count} linked buyers for buyer ID: {BuyerId}", linkedBuyerInfoList.Count, buyerId);
                return linkedBuyerInfoList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading linked buyers for buyer ID: {BuyerId}", buyerId);
                return new List<LinkedBuyerInfo>();
            }
        }

        /// <summary>
        /// Simple test endpoint to verify that the controller is working
        /// </summary>
        /// <returns>A simple text response</returns>
        public IActionResult Test()
        {
            _logger.LogInformation("DEBUG: Test() method called");
            return Content("BuyerProfileController is working!");
        }

        /// <summary>
        /// Displays the buyer profile page
        /// </summary>
        /// <returns>The buyer profile view</returns>
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Redirecting from Index to Manage action");
            return RedirectToAction(nameof(Manage));
        }

        /// <summary>
        /// Updates the buyer profile
        /// </summary>
        /// <param name="model">The buyer profile view model</param>
        /// <returns>Redirects to the updated buyer profile</returns>
        [HttpPost]
        public async Task<IActionResult> Update(BuyerProfileViewModel model)
        {
            _logger.LogInformation("DEBUG: ================================");
            _logger.LogInformation("DEBUG: Update() method called - START");
            _logger.LogInformation("DEBUG: ================================");
            
            try
            {
                _logger.LogInformation("UPDATE: Starting profile update process");
                
                // Check if model is null
                if (model == null)
                {
                    _logger.LogError("DEBUG: Update() - Model is NULL!");
                    return BadRequest("Model is null");
                }
                
                // Log all received model data in detail
                _logger.LogInformation("DEBUG: Update() - Received model data:");
                _logger.LogInformation("DEBUG: Update() - BuyerId: {BuyerId}", model.BuyerId);
                _logger.LogInformation("DEBUG: Update() - FirstName: '{FirstName}'", model.FirstName);
                _logger.LogInformation("DEBUG: Update() - LastName: '{LastName}'", model.LastName);
                _logger.LogInformation("DEBUG: Update() - Email: '{Email}'", model.Email);
                _logger.LogInformation("DEBUG: Update() - PhoneNumber: '{PhoneNumber}'", model.PhoneNumber);
                _logger.LogInformation("DEBUG: Update() - UseSameAddress: {UseSameAddress}", model.UseSameAddress);
                _logger.LogInformation("DEBUG: Update() - Billing Address:");
                _logger.LogInformation("DEBUG: Update() -   Street: '{Street}'", model.BillingStreet);
                _logger.LogInformation("DEBUG: Update() -   City: '{City}'", model.BillingCity);
                _logger.LogInformation("DEBUG: Update() -   Country: '{Country}'", model.BillingCountry);
                _logger.LogInformation("DEBUG: Update() -   PostalCode: '{PostalCode}'", model.BillingPostalCode);
                _logger.LogInformation("DEBUG: Update() - Shipping Address:");
                _logger.LogInformation("DEBUG: Update() -   Street: '{Street}'", model.ShippingStreet);
                _logger.LogInformation("DEBUG: Update() -   City: '{City}'", model.ShippingCity);
                _logger.LogInformation("DEBUG: Update() -   Country: '{Country}'", model.ShippingCountry);
                _logger.LogInformation("DEBUG: Update() -   PostalCode: '{PostalCode}'", model.ShippingPostalCode);
                
                // Custom validation: Remove shipping address validation errors if UseSameAddress is true
                if (model.UseSameAddress)
                {
                    _logger.LogInformation("UPDATE: UseSameAddress is true, removing shipping address validation errors");
                    ModelState.Remove("ShippingStreet");
                    ModelState.Remove("ShippingCity");
                    ModelState.Remove("ShippingCountry");
                    ModelState.Remove("ShippingPostalCode");
                }
                
                _logger.LogInformation("DEBUG: Update() - Checking ModelState validity...");
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("UPDATE: ModelState is invalid");
                    foreach (var error in ModelState)
                    {
                        _logger.LogWarning("UPDATE: ModelState error - Key: {Key}, Errors: {Errors}", 
                            error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                    }
                    _logger.LogInformation("DEBUG: Update() - Returning view with model due to validation errors");
                    return View("Index", model);
                }

                _logger.LogInformation("DEBUG: Update() - ModelState is valid, proceeding...");
                
                int userId = GetCurrentUserId();
                _logger.LogInformation("UPDATE: Updating profile for user ID: {UserId}", userId);

                if (userId == 0)
                {
                    _logger.LogError("DEBUG: Update() - User ID is 0, user not authenticated");
                    return RedirectToAction("Login", "Account");
                }

                // Log the incoming model data
                _logger.LogInformation("UPDATE: Received data - FirstName: {FirstName}, LastName: {LastName}, PhoneNumber: {PhoneNumber}", 
                    model.FirstName, model.LastName, model.PhoneNumber);
                _logger.LogInformation("UPDATE: Billing Address - Street: {Street}, City: {City}, Country: {Country}, PostalCode: {PostalCode}", 
                    model.BillingStreet, model.BillingCity, model.BillingCountry, model.BillingPostalCode);
                _logger.LogInformation("UPDATE: Shipping Address - Street: {Street}, City: {City}, Country: {Country}, PostalCode: {PostalCode}, UseSameAddress: {UseSameAddress}", 
                    model.ShippingStreet, model.ShippingCity, model.ShippingCountry, model.ShippingPostalCode, model.UseSameAddress);

                // Validate shipping address values when UseSameAddress is false
                if (!model.UseSameAddress)
                {
                    _logger.LogInformation("DEBUG: Update() - Validating separate shipping address fields...");
                    if (string.IsNullOrWhiteSpace(model.ShippingStreet) || 
                        string.IsNullOrWhiteSpace(model.ShippingCity) || 
                        string.IsNullOrWhiteSpace(model.ShippingCountry) || 
                        string.IsNullOrWhiteSpace(model.ShippingPostalCode))
                    {
                        _logger.LogWarning("UPDATE: Shipping address has empty fields when UseSameAddress is false");
                        ModelState.AddModelError("", "All shipping address fields are required when not using the same as billing address.");
                        return View("Index", model);
                    }
                    _logger.LogInformation("DEBUG: Update() - Shipping address validation passed");
                }

                // Create a basic User object instead of fetching all users
                _logger.LogInformation("DEBUG: Update() - Creating User object...");
                var user = new User(userId);

                // Add timeout to prevent hanging
                _logger.LogInformation("UPDATE: Getting buyer by user...");
                var buyerTask = _buyerService.GetBuyerByUser(user);
                if (await Task.WhenAny(buyerTask, Task.Delay(5000)) != buyerTask)
                {
                    _logger.LogWarning("UPDATE: GetBuyerByUser operation timed out during update");
                    TempData["ErrorMessage"] = "Operation timed out while updating your profile. Please try again.";
                    return View("Index", model);
                }

                var buyer = await buyerTask;
                if (buyer == null)
                {
                    _logger.LogWarning("UPDATE: No buyer found for user ID: {UserId}", userId);
                    return NotFound();
                }

                _logger.LogInformation("UPDATE: Found buyer with ID: {BuyerId}", buyer.Id);

                // Log the current buyer data before update
                _logger.LogInformation("UPDATE: Current buyer data BEFORE update - FirstName: {FirstName}, LastName: {LastName}, PhoneNumber: {PhoneNumber}, UseSameAddress: {UseSameAddress}", 
                    buyer.FirstName, buyer.LastName, buyer.User?.PhoneNumber, buyer.UseSameAddress);
                
                if (buyer.BillingAddress != null)
                {
                    _logger.LogInformation("UPDATE: Current billing address BEFORE update - Street: {Street}, City: {City}, Country: {Country}, PostalCode: {PostalCode}, AddressId: {AddressId}", 
                        buyer.BillingAddress.StreetLine, buyer.BillingAddress.City, buyer.BillingAddress.Country, buyer.BillingAddress.PostalCode, buyer.BillingAddress.Id);
                }
                else
                {
                    _logger.LogWarning("UPDATE: Billing address is NULL before update");
                }
                
                if (buyer.ShippingAddress != null)
                {
                    _logger.LogInformation("UPDATE: Current shipping address BEFORE update - Street: {Street}, City: {City}, Country: {Country}, PostalCode: {PostalCode}, AddressId: {AddressId}", 
                        buyer.ShippingAddress.StreetLine, buyer.ShippingAddress.City, buyer.ShippingAddress.Country, buyer.ShippingAddress.PostalCode, buyer.ShippingAddress.Id);
                }
                else
                {
                    _logger.LogWarning("UPDATE: Shipping address is NULL before update");
                }

                // Update buyer information
                _logger.LogInformation("UPDATE: Updating buyer information...");
                buyer.FirstName = model.FirstName;
                buyer.LastName = model.LastName;
                
                // Ensure User object exists before setting phone number
                if (buyer.User == null)
                {
                    _logger.LogWarning("UPDATE: buyer.User is null, creating new User object");
                    buyer.User = new User(userId);
                }
                buyer.User.PhoneNumber = model.PhoneNumber;
                buyer.UseSameAddress = model.UseSameAddress;

                _logger.LogInformation("UPDATE: Updated buyer info - FirstName: {FirstName}, LastName: {LastName}, PhoneNumber: {PhoneNumber}, UseSameAddress: {UseSameAddress}", 
                    buyer.FirstName, buyer.LastName, buyer.User.PhoneNumber, buyer.UseSameAddress);

                // Update billing address
                _logger.LogInformation("UPDATE: Updating billing address...");
                if (buyer.BillingAddress == null)
                {
                    _logger.LogInformation("UPDATE: Creating new billing address");
                    buyer.BillingAddress = new Address();
                }

                // Update the existing billing address object instead of creating a new one
                var oldBillingStreet = buyer.BillingAddress.StreetLine;
                var oldBillingCity = buyer.BillingAddress.City;
                var oldBillingCountry = buyer.BillingAddress.Country;
                var oldBillingPostal = buyer.BillingAddress.PostalCode;
                
                buyer.BillingAddress.StreetLine = model.BillingStreet;
                buyer.BillingAddress.City = model.BillingCity;
                buyer.BillingAddress.Country = model.BillingCountry;
                buyer.BillingAddress.PostalCode = model.BillingPostalCode;

                _logger.LogInformation("UPDATE: Billing address CHANGED from:");
                _logger.LogInformation("UPDATE:   OLD - Street: '{OldStreet}', City: '{OldCity}', Country: '{OldCountry}', PostalCode: '{OldPostal}'", 
                    oldBillingStreet, oldBillingCity, oldBillingCountry, oldBillingPostal);
                _logger.LogInformation("UPDATE:   NEW - Street: '{NewStreet}', City: '{NewCity}', Country: '{NewCountry}', PostalCode: '{NewPostal}', AddressId: {AddressId}", 
                    buyer.BillingAddress.StreetLine, buyer.BillingAddress.City, buyer.BillingAddress.Country, buyer.BillingAddress.PostalCode, buyer.BillingAddress.Id);

                // Update shipping address 
                if (model.UseSameAddress)
                {
                    _logger.LogInformation("UPDATE: Using same address - copying billing address values to shipping address");
                    
                    // Ensure shipping address exists as a separate entity
                    if (buyer.ShippingAddress == null)
                    {
                        _logger.LogInformation("UPDATE: Creating new shipping address for same-as-billing");
                        buyer.ShippingAddress = new Address();
                    }
                    
                    // Check if shipping address is the same object as billing address (this would be a problem)
                    if (ReferenceEquals(buyer.ShippingAddress, buyer.BillingAddress))
                    {
                        _logger.LogWarning("UPDATE: Shipping and billing addresses are the same object! Creating separate shipping address.");
                        buyer.ShippingAddress = new Address();
                    }
                    
                    // Copy values from billing to shipping (don't make them the same object)
                    var oldShippingStreet = buyer.ShippingAddress.StreetLine;
                    var oldShippingCity = buyer.ShippingAddress.City;
                    var oldShippingCountry = buyer.ShippingAddress.Country;
                    var oldShippingPostal = buyer.ShippingAddress.PostalCode;
                    
                    buyer.ShippingAddress.StreetLine = buyer.BillingAddress.StreetLine;
                    buyer.ShippingAddress.City = buyer.BillingAddress.City;
                    buyer.ShippingAddress.Country = buyer.BillingAddress.Country;
                    buyer.ShippingAddress.PostalCode = buyer.BillingAddress.PostalCode;
                    
                    _logger.LogInformation("UPDATE: Shipping address CHANGED from (same as billing):");
                    _logger.LogInformation("UPDATE:   OLD - Street: '{OldStreet}', City: '{OldCity}', Country: '{OldCountry}', PostalCode: '{OldPostal}'", 
                        oldShippingStreet, oldShippingCity, oldShippingCountry, oldShippingPostal);
                    _logger.LogInformation("UPDATE:   NEW - Street: '{NewStreet}', City: '{NewCity}', Country: '{NewCountry}', PostalCode: '{NewPostal}', AddressId: {AddressId}", 
                        buyer.ShippingAddress.StreetLine, buyer.ShippingAddress.City, buyer.ShippingAddress.Country, buyer.ShippingAddress.PostalCode, buyer.ShippingAddress.Id);
                }
                else
                {
                    _logger.LogInformation("UPDATE: Updating separate shipping address...");
                    if (buyer.ShippingAddress == null)
                    {
                        _logger.LogInformation("UPDATE: Creating new shipping address");
                        buyer.ShippingAddress = new Address();
                    }

                    // Check if shipping address is the same object as billing address (this would be a problem)
                    if (ReferenceEquals(buyer.ShippingAddress, buyer.BillingAddress))
                    {
                        _logger.LogWarning("UPDATE: Shipping and billing addresses are the same object! Creating separate shipping address.");
                        buyer.ShippingAddress = new Address();
                    }

                    // Update the existing shipping address object instead of creating a new one
                    var oldShippingStreet = buyer.ShippingAddress.StreetLine;
                    var oldShippingCity = buyer.ShippingAddress.City;
                    var oldShippingCountry = buyer.ShippingAddress.Country;
                    var oldShippingPostal = buyer.ShippingAddress.PostalCode;
                    
                    buyer.ShippingAddress.StreetLine = model.ShippingStreet;
                    buyer.ShippingAddress.City = model.ShippingCity;
                    buyer.ShippingAddress.Country = model.ShippingCountry;
                    buyer.ShippingAddress.PostalCode = model.ShippingPostalCode;

                    _logger.LogInformation("UPDATE: Shipping address CHANGED from (separate):");
                    _logger.LogInformation("UPDATE:   OLD - Street: '{OldStreet}', City: '{OldCity}', Country: '{OldCountry}', PostalCode: '{OldPostal}'", 
                        oldShippingStreet, oldShippingCity, oldShippingCountry, oldShippingPostal);
                    _logger.LogInformation("UPDATE:   NEW - Street: '{NewStreet}', City: '{NewCity}', Country: '{NewCountry}', PostalCode: '{NewPostal}', AddressId: {AddressId}", 
                        buyer.ShippingAddress.StreetLine, buyer.ShippingAddress.City, buyer.ShippingAddress.Country, buyer.ShippingAddress.PostalCode, buyer.ShippingAddress.Id);
                }

                // Add timeout for save operation
                _logger.LogInformation("UPDATE: ===== CALLING _buyerService.SaveInfo() =====");
                _logger.LogInformation("UPDATE: About to save buyer with the following data:");
                _logger.LogInformation("UPDATE: Buyer ID: {BuyerId}", buyer.Id);
                _logger.LogInformation("UPDATE: FirstName: '{FirstName}', LastName: '{LastName}'", buyer.FirstName, buyer.LastName);
                _logger.LogInformation("UPDATE: PhoneNumber: '{PhoneNumber}', UseSameAddress: {UseSameAddress}", buyer.User?.PhoneNumber, buyer.UseSameAddress);
                _logger.LogInformation("UPDATE: Billing Address - Street: '{Street}', City: '{City}', Country: '{Country}', PostalCode: '{PostalCode}', Id: {Id}", 
                    buyer.BillingAddress?.StreetLine, buyer.BillingAddress?.City, buyer.BillingAddress?.Country, buyer.BillingAddress?.PostalCode, buyer.BillingAddress?.Id);
                _logger.LogInformation("UPDATE: Shipping Address - Street: '{Street}', City: '{City}', Country: '{Country}', PostalCode: '{PostalCode}', Id: {Id}", 
                    buyer.ShippingAddress?.StreetLine, buyer.ShippingAddress?.City, buyer.ShippingAddress?.Country, buyer.ShippingAddress?.PostalCode, buyer.ShippingAddress?.Id);
                
                var saveTask = _buyerService.SaveInfo(buyer);
                if (await Task.WhenAny(saveTask, Task.Delay(5000)) != saveTask)
                {
                    _logger.LogWarning("UPDATE: SaveInfo operation timed out");
                    TempData["ErrorMessage"] = "Operation timed out while saving your profile. Please try again.";
                    return View("Index", model);
                }

                await saveTask;
                _logger.LogInformation("UPDATE: ===== _buyerService.SaveInfo() COMPLETED =====");

                // Verify what was actually saved by re-loading the buyer
                _logger.LogInformation("UPDATE: ===== VERIFYING SAVED DATA =====");
                var verifyUser = new User(userId);
                var verifyBuyer = await _buyerService.GetBuyerByUser(verifyUser);
                
                _logger.LogInformation("UPDATE: VERIFICATION - Buyer data after save - FirstName: {FirstName}, LastName: {LastName}, PhoneNumber: {PhoneNumber}, UseSameAddress: {UseSameAddress}", 
                    verifyBuyer.FirstName, verifyBuyer.LastName, verifyBuyer.User?.PhoneNumber, verifyBuyer.UseSameAddress);
                
                if (verifyBuyer.BillingAddress != null)
                {
                    _logger.LogInformation("UPDATE: VERIFICATION - Billing address after save - Street: '{Street}', City: '{City}', Country: '{Country}', PostalCode: '{PostalCode}', Id: {Id}", 
                        verifyBuyer.BillingAddress.StreetLine, verifyBuyer.BillingAddress.City, verifyBuyer.BillingAddress.Country, verifyBuyer.BillingAddress.PostalCode, verifyBuyer.BillingAddress.Id);
                }
                else
                {
                    _logger.LogWarning("UPDATE: VERIFICATION - Billing address is NULL after save");
                }
                
                if (verifyBuyer.ShippingAddress != null)
                {
                    _logger.LogInformation("UPDATE: VERIFICATION - Shipping address after save - Street: '{Street}', City: '{City}', Country: '{Country}', PostalCode: '{PostalCode}', Id: {Id}", 
                        verifyBuyer.ShippingAddress.StreetLine, verifyBuyer.ShippingAddress.City, verifyBuyer.ShippingAddress.Country, verifyBuyer.ShippingAddress.PostalCode, verifyBuyer.ShippingAddress.Id);
                }
                else
                {
                    _logger.LogWarning("UPDATE: VERIFICATION - Shipping address is NULL after save");
                }

                // Log what was actually saved to verify the update
                _logger.LogInformation("UPDATE: FINAL SAVED VALUES - Buyer ID: {BuyerId}", buyer.Id);
                _logger.LogInformation("UPDATE: FINAL SAVED VALUES - FirstName: {FirstName}, LastName: {LastName}, PhoneNumber: {PhoneNumber}, UseSameAddress: {UseSameAddress}", 
                    buyer.FirstName, buyer.LastName, buyer.User?.PhoneNumber, buyer.UseSameAddress);
                _logger.LogInformation("UPDATE: FINAL SAVED VALUES - Billing Address - Street: {Street}, City: {City}, Country: {Country}, PostalCode: {PostalCode}", 
                    buyer.BillingAddress?.StreetLine, buyer.BillingAddress?.City, buyer.BillingAddress?.Country, buyer.BillingAddress?.PostalCode);
                _logger.LogInformation("UPDATE: FINAL SAVED VALUES - Shipping Address - Street: {Street}, City: {City}, Country: {Country}, PostalCode: {PostalCode}", 
                    buyer.ShippingAddress?.StreetLine, buyer.ShippingAddress?.City, buyer.ShippingAddress?.Country, buyer.ShippingAddress?.PostalCode);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                _logger.LogInformation("UPDATE: Profile update completed successfully for user ID: {UserId}, redirecting to Manage", userId);
                
                _logger.LogInformation("DEBUG: ================================");
                _logger.LogInformation("DEBUG: Update() method called - END (SUCCESS)");
                _logger.LogInformation("DEBUG: ================================");
                
                return RedirectToAction(nameof(Manage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UPDATE: Error updating buyer profile for user ID: {UserId}", GetCurrentUserId());
                _logger.LogError("DEBUG: Exception details - Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                    ex.GetType().Name, ex.Message, ex.StackTrace);
                TempData["ErrorMessage"] = $"Failed to update profile: {ex.Message}";
                
                _logger.LogInformation("DEBUG: ================================");
                _logger.LogInformation("DEBUG: Update() method called - END (ERROR)");
                _logger.LogInformation("DEBUG: ================================");
                
                return View("Index", model);
            }
        }

        /// <summary>
        /// Manages the buyer profile (private, editable version)
        /// </summary>
        /// <returns>The manage buyer profile view</returns>
        [Authorize]
        public async Task<IActionResult> Manage()
        {
            _logger.LogInformation("Loading private editable Buyer Profile page");
            
            int userId = GetCurrentUserId();
            _logger.LogInformation("Got user ID {UserId} from claims", userId);
            
            if (userId == 0)
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Create a basic User object instead of fetching all users
                var user = new User(userId);
                _logger.LogInformation("Created user object with ID: {UserId}", user.Id);

                // Get buyer information
                Buyer buyer = await _buyerService.GetBuyerByUser(user);
                _logger.LogInformation("GetBuyerByUser() completed successfully");

                if (buyer == null)
                {
                    _logger.LogError("GetBuyerByUser() returned null");
                    return Content("Error: No buyer profile found for this user.");
                }

                _logger.LogInformation("Found buyer: {BuyerId}, loading view model", buyer.Id);

                // Load linked buyers information
                var linkedBuyers = await LoadLinkedBuyersAsync(buyer.Id);

                // Create the view model
                var viewModel = new BuyerProfileViewModel
                {
                    BuyerId = buyer.User?.Id ?? 0,
                    FirstName = buyer.FirstName ?? string.Empty,
                    LastName = buyer.LastName ?? string.Empty,
                    Email = buyer.User?.Email ?? string.Empty,
                    PhoneNumber = buyer.User?.PhoneNumber ?? string.Empty,

                    // Billing address with null safety
                    BillingStreet = buyer.BillingAddress?.StreetLine ?? string.Empty,
                    BillingCity = buyer.BillingAddress?.City ?? string.Empty,
                    BillingCountry = buyer.BillingAddress?.Country ?? string.Empty,
                    BillingPostalCode = buyer.BillingAddress?.PostalCode ?? string.Empty,

                    // Shipping address with null safety
                    ShippingStreet = buyer.ShippingAddress?.StreetLine ?? string.Empty,
                    ShippingCity = buyer.ShippingAddress?.City ?? string.Empty,
                    ShippingCountry = buyer.ShippingAddress?.Country ?? string.Empty,
                    ShippingPostalCode = buyer.ShippingAddress?.PostalCode ?? string.Empty,

                    UseSameAddress = buyer.UseSameAddress,
                    Badge = buyer.Badge.ToString() ?? "None",
                    Discount = buyer.Discount,
                    LinkedBuyers = linkedBuyers
                };

                // Set ViewBag to indicate this is the editable version
                ViewBag.IsOwnProfile = true;
                ViewBag.CanEdit = true;

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading buyer profile for user ID: {UserId}", userId);
                return Content($"Error loading your profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows a public buyer profile (read-only version)
        /// </summary>
        /// <param name="id">The buyer user ID</param>
        /// <returns>The public buyer profile view</returns>
        [Route("BuyerProfile/{id:int}")]
        public async Task<IActionResult> PublicProfile(int id)
        {
            _logger.LogInformation("Loading public Buyer Profile page for buyer ID: {BuyerId}", id);

            try
            {
                // Get the target user by ID
                var targetUser = await _userService.GetUserByIdAsync(id);
                if (targetUser == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return View("UserNotFound");
                }

                // Get buyer information for the target user
                Buyer targetBuyer = await _buyerService.GetBuyerByUser(targetUser);
                if (targetBuyer == null)
                {
                    _logger.LogWarning("No buyer profile found for user ID: {UserId}", id);
                    return View("UserNotFound");
                }

                _logger.LogInformation("Found buyer: {BuyerId}, loading public view model", targetBuyer.Id);

                // Get current user ID for linkage checking
                int currentUserId = GetCurrentUserId();
                
                // Get linkage information if user is authenticated
                BuyerLinkageInfo? linkageInfo = null;
                if (currentUserId > 0 && currentUserId != id)
                {
                    linkageInfo = await _buyerLinkageService.GetLinkageStatusAsync(currentUserId, id);
                }

                // Load linked buyers information
                var linkedBuyers = await LoadLinkedBuyersAsync(targetBuyer.Id);

                // Create the view model with public information only
                var viewModel = new BuyerProfileViewModel
                {
                    BuyerId = targetBuyer.User?.Id ?? 0,
                    FirstName = targetBuyer.FirstName ?? string.Empty,
                    LastName = targetBuyer.LastName ?? string.Empty,
                    Email = targetBuyer.User?.Email ?? string.Empty,
                    Badge = targetBuyer.Badge.ToString() ?? "None",
                    // Don't expose private information like addresses and phone number
                    PhoneNumber = string.Empty,
                    BillingStreet = string.Empty,
                    BillingCity = string.Empty,
                    BillingCountry = string.Empty,
                    BillingPostalCode = string.Empty,
                    ShippingStreet = string.Empty,
                    ShippingCity = string.Empty,
                    ShippingCountry = string.Empty,
                    ShippingPostalCode = string.Empty,
                    UseSameAddress = false,
                    Discount = 0, // Don't expose discount information
                    LinkedBuyers = linkedBuyers
                };

                // Set ViewBag to indicate this is a public read-only profile
                ViewBag.IsOwnProfile = false;
                ViewBag.CanEdit = false;
                ViewBag.LinkageInfo = linkageInfo;

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public buyer profile for ID: {BuyerId}", id);
                return View("UserNotFound");
            }
        }

        /// <summary>
        /// Links the current buyer with another buyer
        /// </summary>
        /// <param name="targetBuyerId">The ID of the buyer to link with</param>
        /// <returns>Redirects back to the public profile</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Link(int targetBuyerId)
        {
            _logger.LogInformation("Link action called for target buyer ID: {TargetBuyerId}", targetBuyerId);

            try
            {
                int currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    _logger.LogWarning("User not authenticated for link action");
                    return RedirectToAction("Login", "Account");
                }

                if (currentUserId == targetBuyerId)
                {
                    _logger.LogWarning("User {UserId} attempted to link to themselves", currentUserId);
                    TempData["ErrorMessage"] = "You cannot link to yourself.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                bool success = await _buyerLinkageService.LinkBuyersAsync(currentUserId, targetBuyerId);

                if (success)
                {
                    _logger.LogInformation("Successfully linked buyer {CurrentUserId} with buyer {TargetBuyerId}", 
                        currentUserId, targetBuyerId);
                    TempData["SuccessMessage"] = "Successfully linked with buyer!";
                }
                else
                {
                    _logger.LogWarning("Failed to link buyer {CurrentUserId} with buyer {TargetBuyerId}", 
                        currentUserId, targetBuyerId);
                    TempData["ErrorMessage"] = "Failed to link with buyer. Please try again.";
                }

                return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking buyers - Current: {CurrentUserId}, Target: {TargetBuyerId}", 
                    GetCurrentUserId(), targetBuyerId);
                TempData["ErrorMessage"] = "An error occurred while linking. Please try again.";
                return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
            }
        }

        /// <summary>
        /// Unlinks the current buyer from another buyer
        /// </summary>
        /// <param name="targetBuyerId">The ID of the buyer to unlink from</param>
        /// <returns>Redirects back to the public profile</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Unlink(int targetBuyerId)
        {
            _logger.LogInformation("Unlink action called for target buyer ID: {TargetBuyerId}", targetBuyerId);

            try
            {
                int currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    _logger.LogWarning("User not authenticated for unlink action");
                    return RedirectToAction("Login", "Account");
                }

                if (currentUserId == targetBuyerId)
                {
                    _logger.LogWarning("User {UserId} attempted to unlink from themselves", currentUserId);
                    TempData["ErrorMessage"] = "You cannot unlink from yourself.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                bool success = await _buyerLinkageService.UnlinkBuyersAsync(currentUserId, targetBuyerId);

                if (success)
                {
                    _logger.LogInformation("Successfully unlinked buyer {CurrentUserId} from buyer {TargetBuyerId}", 
                        currentUserId, targetBuyerId);
                    TempData["SuccessMessage"] = "Successfully unlinked from buyer!";
                }
                else
                {
                    _logger.LogWarning("Failed to unlink buyer {CurrentUserId} from buyer {TargetBuyerId}", 
                        currentUserId, targetBuyerId);
                    TempData["ErrorMessage"] = "Failed to unlink from buyer. Please try again.";
                }

                return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking buyers - Current: {CurrentUserId}, Target: {TargetBuyerId}", 
                    GetCurrentUserId(), targetBuyerId);
                TempData["ErrorMessage"] = "An error occurred while unlinking. Please try again.";
                return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
            }
        }
    }
}
