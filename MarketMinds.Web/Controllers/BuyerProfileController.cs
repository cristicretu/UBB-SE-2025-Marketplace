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
        private readonly IBuyerSellerFollowService _buyerSellerFollowService;
        private readonly ILogger<BuyerProfileController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerProfileController"/> class.
        /// </summary>
        /// <param name="buyerService">The buyer service.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="buyerLinkageService">The buyer linkage service.</param>
        /// <param name="buyerSellerFollowService">The buyer seller follow service.</param>
        /// <param name="logger">The logger.</param>
        public BuyerProfileController(
            IBuyerService buyerService,
            IUserService userService,
            IBuyerLinkageService buyerLinkageService,
            IBuyerSellerFollowService buyerSellerFollowService,
            ILogger<BuyerProfileController> logger)
        {
            _buyerService = buyerService ?? throw new ArgumentNullException(nameof(buyerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _buyerLinkageService = buyerLinkageService ?? throw new ArgumentNullException(nameof(buyerLinkageService));
            _buyerSellerFollowService = buyerSellerFollowService ?? throw new ArgumentNullException(nameof(buyerSellerFollowService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _logger.LogInformation("CONSTRUCTOR: BuyerProfileController initialized");
        }

        /// <summary>
        /// Checks if the current user is a buyer
        /// </summary>
        /// <returns>True if current user is a buyer, false otherwise</returns>
        private bool IsCurrentUserBuyer()
        {
            try
            {
                // Check UserSession role first (for backward compatibility)
                if (!string.IsNullOrEmpty(UserSession.CurrentUserRole))
                {
                    return UserSession.CurrentUserRole == "Buyer" || UserSession.CurrentUserRole == "2";
                }

                // Check claims-based role
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
                if (roleClaim != null)
                {
                    return roleClaim.Value == "Buyer";
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking if current user is buyer");
                return false;
            }
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
                        _logger.LogInformation("Processing linked buyer ID: {LinkedBuyerId}, User is null: {UserIsNull}", 
                            linkedBuyer.Id, linkedBuyer.User == null);
                        
                        string username = string.Empty;
                        string email = string.Empty;
                        
                        // If User object is not loaded, try to get it separately
                        if (linkedBuyer.User == null)
                        {
                            _logger.LogWarning("User object is null for linked buyer {LinkedBuyerId}, trying to get user by ID", linkedBuyer.Id);
                            var user = await _userService.GetUserByIdAsync(linkedBuyer.Id);
                            if (user != null)
                            {
                                username = user.Username ?? string.Empty;
                                email = user.Email ?? string.Empty;
                                _logger.LogInformation("Retrieved user data for buyer {LinkedBuyerId}: Username='{Username}', Email='{Email}'", 
                                    linkedBuyer.Id, username, email);
                            }
                            else
                            {
                                _logger.LogWarning("Could not retrieve user data for buyer {LinkedBuyerId}", linkedBuyer.Id);
                            }
                        }
                        else
                        {
                            username = linkedBuyer.User.Username ?? string.Empty;
                            email = linkedBuyer.User.Email ?? string.Empty;
                            _logger.LogInformation("User object available for buyer {LinkedBuyerId}: Username='{Username}', Email='{Email}'", 
                                linkedBuyer.Id, username, email);
                        }

                        var linkedBuyerInfo = new LinkedBuyerInfo
                        {
                            BuyerId = linkedBuyer.Id,
                            FirstName = linkedBuyer.FirstName ?? "Unknown",
                            LastName = linkedBuyer.LastName ?? "User",
                            Email = email,
                            Username = username,
                            Badge = linkedBuyer.Badge.ToString() ?? "None",
                            LinkedDate = DateTime.UtcNow // Default for now since we don't have this in the old system
                        };
                        
                        _logger.LogInformation("Created LinkedBuyerInfo for buyer {LinkedBuyerId}: Username='{Username}', Email='{Email}'", 
                            linkedBuyer.Id, linkedBuyerInfo.Username, linkedBuyerInfo.Email);
                        
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
        /// Loads following sellers information for a specific buyer
        /// </summary>
        /// <param name="buyerId">The buyer ID to get followed sellers for</param>
        /// <returns>List of followed seller information</returns>
        private async Task<List<FollowedSellerInfo>> LoadFollowingSellersAsync(int buyerId)
        {
            try
            {
                _logger.LogInformation("Loading following sellers for buyer ID: {BuyerId}", buyerId);

                // Get followed sellers directly from the service
                var followedSellers = await _buyerSellerFollowService.GetFollowedSellersAsync(buyerId);
                
                var followedSellerInfoList = new List<FollowedSellerInfo>();

                foreach (var seller in followedSellers)
                {
                    try
                    {
                        var followedSellerInfo = new FollowedSellerInfo
                        {
                            SellerId = seller.Id,
                            StoreName = seller.StoreName ?? "Unknown Store",
                            FirstName = seller.User?.Username ?? "Unknown", // Use Username as name since FirstName doesn't exist
                            LastName = string.Empty, // Seller model doesn't have LastName
                            Email = seller.User?.Email ?? string.Empty,
                            FollowedDate = DateTime.UtcNow // Default for now since we don't have this in the old system
                        };
                        followedSellerInfoList.Add(followedSellerInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process followed seller {SellerId} for buyer {BuyerId}", seller.Id, buyerId);
                        // Continue with other followed sellers
                    }
                }

                _logger.LogInformation("Loaded {Count} followed sellers for buyer ID: {BuyerId}", followedSellerInfoList.Count, buyerId);
                return followedSellerInfoList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading followed sellers for buyer ID: {BuyerId}", buyerId);
                return new List<FollowedSellerInfo>();
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

                // Comprehensive validation - all fields must be filled
                var validationErrors = new List<string>();

                // Personal Information validation
                if (string.IsNullOrWhiteSpace(model.FirstName))
                {
                    validationErrors.Add("First name is required.");
                    ModelState.AddModelError("FirstName", "First name is required.");
                }

                if (string.IsNullOrWhiteSpace(model.LastName))
                {
                    validationErrors.Add("Last name is required.");
                    ModelState.AddModelError("LastName", "Last name is required.");
                }

                if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    validationErrors.Add("Phone number is required.");
                    ModelState.AddModelError("PhoneNumber", "Phone number is required.");
                }

                // Billing Address validation
                if (string.IsNullOrWhiteSpace(model.BillingStreet))
                {
                    validationErrors.Add("Billing street address is required.");
                    ModelState.AddModelError("BillingStreet", "Billing street address is required.");
                }

                if (string.IsNullOrWhiteSpace(model.BillingCity))
                {
                    validationErrors.Add("Billing city is required.");
                    ModelState.AddModelError("BillingCity", "Billing city is required.");
                }

                if (string.IsNullOrWhiteSpace(model.BillingCountry))
                {
                    validationErrors.Add("Billing country is required.");
                    ModelState.AddModelError("BillingCountry", "Billing country is required.");
                }

                if (string.IsNullOrWhiteSpace(model.BillingPostalCode))
                {
                    validationErrors.Add("Billing postal code is required.");
                    ModelState.AddModelError("BillingPostalCode", "Billing postal code is required.");
                }

                // Shipping Address validation (only if not using same address)
                if (!model.UseSameAddress)
                {
                    if (string.IsNullOrWhiteSpace(model.ShippingStreet))
                    {
                        validationErrors.Add("Shipping street address is required when not using the same as billing address.");
                        ModelState.AddModelError("ShippingStreet", "Shipping street address is required.");
                    }

                    if (string.IsNullOrWhiteSpace(model.ShippingCity))
                    {
                        validationErrors.Add("Shipping city is required when not using the same as billing address.");
                        ModelState.AddModelError("ShippingCity", "Shipping city is required.");
                    }

                    if (string.IsNullOrWhiteSpace(model.ShippingCountry))
                    {
                        validationErrors.Add("Shipping country is required when not using the same as billing address.");
                        ModelState.AddModelError("ShippingCountry", "Shipping country is required.");
                    }

                    if (string.IsNullOrWhiteSpace(model.ShippingPostalCode))
                    {
                        validationErrors.Add("Shipping postal code is required when not using the same as billing address.");
                        ModelState.AddModelError("ShippingPostalCode", "Shipping postal code is required.");
                    }
                }

                // If there are validation errors, return to the form
                if (validationErrors.Any())
                {
                    _logger.LogWarning("UPDATE: Validation failed with {Count} errors: {Errors}", 
                        validationErrors.Count, string.Join("; ", validationErrors));
                    
                    TempData["ErrorMessage"] = "All fields are required. Please fill in all the information before updating your profile.";
                    return View("Index", model);
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

                // Load following sellers information
                var followedSellers = await LoadFollowingSellersAsync(buyer.Id);

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

                // Assign the followed sellers separately to avoid compilation issues
                viewModel.FollowingSellers = followedSellers;

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
                
                // Get linkage information if user is authenticated AND is a buyer
                BuyerLinkageInfo? linkageInfo = null;
                if (currentUserId > 0 && currentUserId != id && IsCurrentUserBuyer())
                {
                    // Get current user's buyer profile to get the buyer ID
                    var currentUser = await _userService.GetUserByIdAsync(currentUserId);
                    if (currentUser != null)
                    {
                        var currentBuyer = await _buyerService.GetBuyerByUser(currentUser);
                        if (currentBuyer != null)
                        {
                            _logger.LogInformation("Getting linkage status between current buyer {CurrentBuyerId} and target buyer {TargetBuyerId}", 
                                currentBuyer.Id, targetBuyer.Id);
                            linkageInfo = await _buyerLinkageService.GetLinkageStatusAsync(currentBuyer.Id, targetBuyer.Id);
                            _logger.LogInformation("Linkage status: {Status}, CanManageLink: {CanManageLink}", 
                                linkageInfo?.Status, linkageInfo?.CanManageLink);
                        }
                        else
                        {
                            _logger.LogWarning("Current user {UserId} does not have a buyer profile", currentUserId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Current user {UserId} not found", currentUserId);
                    }
                }
                else
                {
                    _logger.LogInformation("Not showing linkage info - CurrentUserId: {CurrentUserId}, TargetUserId: {TargetUserId}, IsCurrentUserBuyer: {IsCurrentUserBuyer}", 
                        currentUserId, id, IsCurrentUserBuyer());
                }

                // Load linked buyers information
                var linkedBuyers = await LoadLinkedBuyersAsync(targetBuyer.Id);

                // Load following sellers information
                var followedSellers = await LoadFollowingSellersAsync(targetBuyer.Id);

                // Create the view model with public information only
                var viewModel = new BuyerProfileViewModel
                {
                    BuyerId = targetBuyer.User?.Id ?? 0,
                    FirstName = targetBuyer.FirstName ?? string.Empty,
                    LastName = targetBuyer.LastName ?? string.Empty,
                    Email = targetBuyer.User?.Email ?? string.Empty,
                    Username = targetBuyer.User?.Username ?? string.Empty,
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

                // Assign the followed sellers separately to avoid compilation issues
                viewModel.FollowingSellers = followedSellers;

                // Set ViewBag to indicate this is a public read-only profile
                ViewBag.IsOwnProfile = false;
                ViewBag.CanEdit = false;
                ViewBag.LinkageInfo = linkageInfo;

                _logger.LogInformation("Setting ViewBag.LinkageInfo to: {LinkageInfo}", linkageInfo != null ? $"Status: {linkageInfo.Status}, CanManageLink: {linkageInfo.CanManageLink}" : "null");

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public buyer profile for ID: {BuyerId}", id);
                return View("UserNotFound");
            }
        }

        /// <summary>
        /// Sends a linkage request to another buyer
        /// </summary>
        /// <param name="targetBuyerId">The ID of the buyer to send request to</param>
        /// <returns>Redirects back to the public profile</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendRequest(int targetBuyerId)
        {
            _logger.LogInformation("SendRequest action called for target buyer ID: {TargetBuyerId}", targetBuyerId);

            try
            {
                int currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    _logger.LogWarning("User not authenticated for send request action");
                    return RedirectToAction("Login", "Account");
                }

                // Check if current user is a buyer
                if (!IsCurrentUserBuyer())
                {
                    _logger.LogWarning("Non-buyer user {UserId} attempted to send request to buyer {TargetBuyerId}", currentUserId, targetBuyerId);
                    TempData["ErrorMessage"] = "Only buyers can send link requests to other buyers.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                if (currentUserId == targetBuyerId)
                {
                    _logger.LogWarning("User {UserId} attempted to send request to themselves", currentUserId);
                    TempData["ErrorMessage"] = "You cannot send a link request to yourself.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                // Get current user's buyer profile
                var currentUser = await _userService.GetUserByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user {UserId} not found", currentUserId);
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                var currentBuyer = await _buyerService.GetBuyerByUser(currentUser);
                if (currentBuyer == null)
                {
                    _logger.LogWarning("Current user {UserId} does not have a buyer profile", currentUserId);
                    TempData["ErrorMessage"] = "Buyer profile not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                // Get target user's buyer profile
                var targetUser = await _userService.GetUserByIdAsync(targetBuyerId);
                if (targetUser == null)
                {
                    _logger.LogWarning("Target user {UserId} not found", targetBuyerId);
                    TempData["ErrorMessage"] = "Target user not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                var targetBuyer = await _buyerService.GetBuyerByUser(targetUser);
                if (targetBuyer == null)
                {
                    _logger.LogWarning("Target user {UserId} does not have a buyer profile", targetBuyerId);
                    TempData["ErrorMessage"] = "Target buyer profile not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                bool success = await _buyerLinkageService.CreateLinkageRequestAsync(currentBuyer.Id, targetBuyer.Id);

                if (success)
                {
                    _logger.LogInformation("Successfully sent link request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}", 
                        currentBuyer.Id, targetBuyer.Id);
                    TempData["SuccessMessage"] = "Link request sent successfully!";
                }
                else
                {
                    _logger.LogWarning("Failed to send link request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}", 
                        currentBuyer.Id, targetBuyer.Id);
                    TempData["ErrorMessage"] = "Failed to send link request. Please try again.";
                }

                return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending link request - Current: {CurrentUserId}, Target: {TargetBuyerId}", 
                    GetCurrentUserId(), targetBuyerId);
                TempData["ErrorMessage"] = "An error occurred while sending the link request. Please try again.";
                return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
            }
        }

        /// <summary>
        /// Accepts a pending linkage request from another buyer
        /// </summary>
        /// <param name="requestingBuyerId">The ID of the buyer who sent the request</param>
        /// <returns>Redirects back to the public profile</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AcceptRequest(int requestingBuyerId)
        {
            _logger.LogInformation("AcceptRequest action called for requesting buyer ID: {RequestingBuyerId}", requestingBuyerId);

            try
            {
                int currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    _logger.LogWarning("User not authenticated for accept request action");
                    return RedirectToAction("Login", "Account");
                }

                // Check if current user is a buyer
                if (!IsCurrentUserBuyer())
                {
                    _logger.LogWarning("Non-buyer user {UserId} attempted to accept request from buyer {RequestingBuyerId}", currentUserId, requestingBuyerId);
                    TempData["ErrorMessage"] = "Only buyers can manage link requests.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                if (currentUserId == requestingBuyerId)
                {
                    _logger.LogWarning("User {UserId} attempted to accept request from themselves", currentUserId);
                    TempData["ErrorMessage"] = "You cannot accept a request from yourself.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                // Get current user's buyer profile
                var currentUser = await _userService.GetUserByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user {UserId} not found", currentUserId);
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                var currentBuyer = await _buyerService.GetBuyerByUser(currentUser);
                if (currentBuyer == null)
                {
                    _logger.LogWarning("Current user {UserId} does not have a buyer profile", currentUserId);
                    TempData["ErrorMessage"] = "Buyer profile not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                // Get requesting user's buyer profile
                var requestingUser = await _userService.GetUserByIdAsync(requestingBuyerId);
                if (requestingUser == null)
                {
                    _logger.LogWarning("Requesting user {UserId} not found", requestingBuyerId);
                    TempData["ErrorMessage"] = "Requesting user not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                var requestingBuyer = await _buyerService.GetBuyerByUser(requestingUser);
                if (requestingBuyer == null)
                {
                    _logger.LogWarning("Requesting user {UserId} does not have a buyer profile", requestingBuyerId);
                    TempData["ErrorMessage"] = "Requesting buyer profile not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                bool success = await _buyerLinkageService.AcceptLinkageRequestAsync(currentBuyer.Id, requestingBuyer.Id);

                if (success)
                {
                    _logger.LogInformation("Successfully accepted link request from buyer {RequestingBuyerId} by buyer {CurrentBuyerId}", 
                        requestingBuyer.Id, currentBuyer.Id);
                    TempData["SuccessMessage"] = "Link request accepted! You are now linked with this buyer.";
                }
                else
                {
                    _logger.LogWarning("Failed to accept link request from buyer {RequestingBuyerId} by buyer {CurrentBuyerId}", 
                        requestingBuyer.Id, currentBuyer.Id);
                    TempData["ErrorMessage"] = "Failed to accept link request. Please try again.";
                }

                return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting link request - Current: {CurrentUserId}, Requesting: {RequestingBuyerId}", 
                    GetCurrentUserId(), requestingBuyerId);
                TempData["ErrorMessage"] = "An error occurred while accepting the link request. Please try again.";
                return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
            }
        }

        /// <summary>
        /// Rejects a pending linkage request from another buyer
        /// </summary>
        /// <param name="requestingBuyerId">The ID of the buyer who sent the request</param>
        /// <returns>Redirects back to the public profile</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RejectRequest(int requestingBuyerId)
        {
            _logger.LogInformation("RejectRequest action called for requesting buyer ID: {RequestingBuyerId}", requestingBuyerId);

            try
            {
                int currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    _logger.LogWarning("User not authenticated for reject request action");
                    return RedirectToAction("Login", "Account");
                }

                // Check if current user is a buyer
                if (!IsCurrentUserBuyer())
                {
                    _logger.LogWarning("Non-buyer user {UserId} attempted to reject request from buyer {RequestingBuyerId}", currentUserId, requestingBuyerId);
                    TempData["ErrorMessage"] = "Only buyers can manage link requests.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                if (currentUserId == requestingBuyerId)
                {
                    _logger.LogWarning("User {UserId} attempted to reject request from themselves", currentUserId);
                    TempData["ErrorMessage"] = "You cannot reject a request from yourself.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                // Get current user's buyer profile
                var currentUser = await _userService.GetUserByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user {UserId} not found", currentUserId);
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                var currentBuyer = await _buyerService.GetBuyerByUser(currentUser);
                if (currentBuyer == null)
                {
                    _logger.LogWarning("Current user {UserId} does not have a buyer profile", currentUserId);
                    TempData["ErrorMessage"] = "Buyer profile not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                // Get requesting user's buyer profile
                var requestingUser = await _userService.GetUserByIdAsync(requestingBuyerId);
                if (requestingUser == null)
                {
                    _logger.LogWarning("Requesting user {UserId} not found", requestingBuyerId);
                    TempData["ErrorMessage"] = "Requesting user not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                var requestingBuyer = await _buyerService.GetBuyerByUser(requestingUser);
                if (requestingBuyer == null)
                {
                    _logger.LogWarning("Requesting user {UserId} does not have a buyer profile", requestingBuyerId);
                    TempData["ErrorMessage"] = "Requesting buyer profile not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
                }

                bool success = await _buyerLinkageService.RejectLinkageRequestAsync(currentBuyer.Id, requestingBuyer.Id);

                if (success)
                {
                    _logger.LogInformation("Successfully rejected link request from buyer {RequestingBuyerId} by buyer {CurrentBuyerId}", 
                        requestingBuyer.Id, currentBuyer.Id);
                    TempData["SuccessMessage"] = "Link request rejected.";
                }
                else
                {
                    _logger.LogWarning("Failed to reject link request from buyer {RequestingBuyerId} by buyer {CurrentBuyerId}", 
                        requestingBuyer.Id, currentBuyer.Id);
                    TempData["ErrorMessage"] = "Failed to reject link request. Please try again.";
                }

                return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting link request - Current: {CurrentUserId}, Requesting: {RequestingBuyerId}", 
                    GetCurrentUserId(), requestingBuyerId);
                TempData["ErrorMessage"] = "An error occurred while rejecting the link request. Please try again.";
                return RedirectToAction(nameof(PublicProfile), new { id = requestingBuyerId });
            }
        }

        /// <summary>
        /// Cancels a pending linkage request that the current user sent
        /// </summary>
        /// <param name="targetBuyerId">The ID of the buyer who received the request</param>
        /// <returns>Redirects back to the public profile</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CancelRequest(int targetBuyerId)
        {
            _logger.LogInformation("CancelRequest action called for target buyer ID: {TargetBuyerId}", targetBuyerId);

            try
            {
                int currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    _logger.LogWarning("User not authenticated for cancel request action");
                    return RedirectToAction("Login", "Account");
                }

                // Check if current user is a buyer
                if (!IsCurrentUserBuyer())
                {
                    _logger.LogWarning("Non-buyer user {UserId} attempted to cancel request to buyer {TargetBuyerId}", currentUserId, targetBuyerId);
                    TempData["ErrorMessage"] = "Only buyers can manage link requests.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                if (currentUserId == targetBuyerId)
                {
                    _logger.LogWarning("User {UserId} attempted to cancel request to themselves", currentUserId);
                    TempData["ErrorMessage"] = "You cannot cancel a request to yourself.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                // Get current user's buyer profile
                var currentUser = await _userService.GetUserByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user {UserId} not found", currentUserId);
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                var currentBuyer = await _buyerService.GetBuyerByUser(currentUser);
                if (currentBuyer == null)
                {
                    _logger.LogWarning("Current user {UserId} does not have a buyer profile", currentUserId);
                    TempData["ErrorMessage"] = "Buyer profile not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                // Get target user's buyer profile
                var targetUser = await _userService.GetUserByIdAsync(targetBuyerId);
                if (targetUser == null)
                {
                    _logger.LogWarning("Target user {UserId} not found", targetBuyerId);
                    TempData["ErrorMessage"] = "Target user not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                var targetBuyer = await _buyerService.GetBuyerByUser(targetUser);
                if (targetBuyer == null)
                {
                    _logger.LogWarning("Target user {UserId} does not have a buyer profile", targetBuyerId);
                    TempData["ErrorMessage"] = "Target buyer profile not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                bool success = await _buyerLinkageService.CancelLinkageRequestAsync(currentBuyer.Id, targetBuyer.Id);

                if (success)
                {
                    _logger.LogInformation("Successfully cancelled link request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}", 
                        currentBuyer.Id, targetBuyer.Id);
                    TempData["SuccessMessage"] = "Link request cancelled.";
                }
                else
                {
                    _logger.LogWarning("Failed to cancel link request from buyer {CurrentBuyerId} to buyer {TargetBuyerId}", 
                        currentBuyer.Id, targetBuyer.Id);
                    TempData["ErrorMessage"] = "Failed to cancel link request. Please try again.";
                }

                return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling link request - Current: {CurrentUserId}, Target: {TargetBuyerId}", 
                    GetCurrentUserId(), targetBuyerId);
                TempData["ErrorMessage"] = "An error occurred while cancelling the link request. Please try again.";
                return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
            }
        }

        /// <summary>
        /// Removes an existing link between buyers
        /// </summary>
        /// <param name="targetBuyerId">The ID of the buyer to unlink from</param>
        /// <returns>Redirects back to the public profile</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveLink(int targetBuyerId)
        {
            _logger.LogInformation("RemoveLink action called for target buyer ID: {TargetBuyerId}", targetBuyerId);

            try
            {
                int currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    _logger.LogWarning("User not authenticated for remove link action");
                    return RedirectToAction("Login", "Account");
                }

                // Check if current user is a buyer
                if (!IsCurrentUserBuyer())
                {
                    _logger.LogWarning("Non-buyer user {UserId} attempted to remove link with buyer {TargetBuyerId}", currentUserId, targetBuyerId);
                    TempData["ErrorMessage"] = "Only buyers can manage buyer links.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                if (currentUserId == targetBuyerId)
                {
                    _logger.LogWarning("User {UserId} attempted to remove link with themselves", currentUserId);
                    TempData["ErrorMessage"] = "You cannot remove a link with yourself.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                // Get current user's buyer profile
                var currentUser = await _userService.GetUserByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user {UserId} not found", currentUserId);
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                var currentBuyer = await _buyerService.GetBuyerByUser(currentUser);
                if (currentBuyer == null)
                {
                    _logger.LogWarning("Current user {UserId} does not have a buyer profile", currentUserId);
                    TempData["ErrorMessage"] = "Buyer profile not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                // Get target user's buyer profile
                var targetUser = await _userService.GetUserByIdAsync(targetBuyerId);
                if (targetUser == null)
                {
                    _logger.LogWarning("Target user {UserId} not found", targetBuyerId);
                    TempData["ErrorMessage"] = "Target user not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                var targetBuyer = await _buyerService.GetBuyerByUser(targetUser);
                if (targetBuyer == null)
                {
                    _logger.LogWarning("Target user {UserId} does not have a buyer profile", targetBuyerId);
                    TempData["ErrorMessage"] = "Target buyer profile not found.";
                    return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
                }

                bool success = await _buyerLinkageService.UnlinkBuyersAsync(currentBuyer.Id, targetBuyer.Id);

                if (success)
                {
                    _logger.LogInformation("Successfully removed link between buyer {CurrentBuyerId} and buyer {TargetBuyerId}", 
                        currentBuyer.Id, targetBuyer.Id);
                    TempData["SuccessMessage"] = "Link removed successfully!";
                }
                else
                {
                    _logger.LogWarning("Failed to remove link between buyer {CurrentBuyerId} and buyer {TargetBuyerId}", 
                        currentBuyer.Id, targetBuyer.Id);
                    TempData["ErrorMessage"] = "Failed to remove link. Please try again.";
                }

                return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing link - Current: {CurrentUserId}, Target: {TargetBuyerId}", 
                    GetCurrentUserId(), targetBuyerId);
                TempData["ErrorMessage"] = "An error occurred while removing the link. Please try again.";
                return RedirectToAction(nameof(PublicProfile), new { id = targetBuyerId });
            }
        }

        /// <summary>
        /// Links the current buyer with another buyer (DEPRECATED - use SendRequest instead)
        /// </summary>
        /// <param name="targetBuyerId">The ID of the buyer to link with</param>
        /// <returns>Redirects back to the public profile</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Link(int targetBuyerId)
        {
            // Redirect to the new SendRequest action for backward compatibility
            return await SendRequest(targetBuyerId);
        }

        /// <summary>
        /// Unlinks the current buyer from another buyer (DEPRECATED - use RemoveLink instead)
        /// </summary>
        /// <param name="targetBuyerId">The ID of the buyer to unlink from</param>
        /// <returns>Redirects back to the public profile</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Unlink(int targetBuyerId)
        {
            // Redirect to the new RemoveLink action for backward compatibility
            return await RemoveLink(targetBuyerId);
        }
    }
}
