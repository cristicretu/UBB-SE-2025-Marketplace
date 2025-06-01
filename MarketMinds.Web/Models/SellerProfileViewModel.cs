using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.UserService;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Shared.Services.AuctionProductsService;

namespace WebMarketplace.Models
{
    /// <summary>
    /// View model for the Seller Profile page.
    /// </summary>
    public class SellerProfileViewModel : INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly ISellerService _sellerService;
        private readonly IAuctionProductService _auctionProductService;
        private readonly IBorrowProductsService _borrowProductsService;
        private readonly IBuyProductsService _buyProductsService;
        private User _user;
        private Seller _seller;
        private List<Product> _allProducts = new List<Product>();
        private List<Product> _filteredProducts = new List<Product>();
        private string _searchText = string.Empty;
        private bool _isSortedByPrice;
        private List<Buyer> _followersList = new List<Buyer>();
        private int _totalProductCount;
        private int _allProductsCount;

        public SellerProfileViewModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerProfileViewModel"/> class.
        /// </summary>
        /// <param name="user">The user associated with the seller.</param>
        /// <param name="userService">Service for user-related operations.</param>
        /// <param name="sellerService">Service for seller-related operations.</param>
        /// <param name="auctionProductService">Service for auction products.</param>
        /// <param name="borrowProductsService">Service for borrow products.</param>
        /// <param name="buyProductsService">Service for buy products.</param>
        public SellerProfileViewModel(User user, IUserService userService, ISellerService sellerService,
            IAuctionProductService auctionProductService, IBorrowProductsService borrowProductsService,
            IBuyProductsService buyProductsService)
        {
            _user = user;
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _sellerService = sellerService ?? throw new ArgumentNullException(nameof(sellerService));
            _auctionProductService = auctionProductService ?? throw new ArgumentNullException(nameof(auctionProductService));
            _borrowProductsService = borrowProductsService ?? throw new ArgumentNullException(nameof(borrowProductsService));
            _buyProductsService = buyProductsService ?? throw new ArgumentNullException(nameof(buyProductsService));
        }

        /// <summary>
        /// Event that's raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the seller.
        /// </summary>
        public Seller Seller
        {
            get => _seller;
            set
            {
                _seller = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the user associated with the seller.
        /// </summary>
        public User User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the filtered products.
        /// </summary>
        public List<Product> Products
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        [Display(Name = "Search Products")]
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    FilterProducts(value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the products are sorted by price.
        /// </summary>
        public bool IsSortedByPrice
        {
            get => _isSortedByPrice;
            set
            {
                if (_isSortedByPrice != value)
                {
                    _isSortedByPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of followers for this seller.
        /// </summary>
        public List<Buyer> FollowersList
        {
            get => _followersList;
            set
            {
                _followersList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the total count of products for pagination.
        /// </summary>
        public int TotalProductCount
        {
            get => _totalProductCount;
            set
            {
                _totalProductCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the total count of ALL products (before any filtering).
        /// </summary>
        public int AllProductsCount
        {
            get => _allProductsCount;
            set
            {
                _allProductsCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Filters products based on search text.
        /// </summary>
        /// <param name="searchText">The search text to filter by.</param>
        public void FilterProducts(string searchText)
        {
            Products = string.IsNullOrEmpty(searchText)
                ? _allProducts.ToList()
                : _allProducts
                    .Where(p => p.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                               p.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
        }

        /// <summary>
        /// Sorts the products by price.
        /// </summary>
        public void SortProducts()
        {
            IsSortedByPrice = !IsSortedByPrice;
            SortProductsWithDirection(IsSortedByPrice);
        }

        /// <summary>
        /// Sorts the products by price in the specified direction.
        /// </summary>
        /// <param name="ascending">True for ascending order, false for descending order.</param>
        public void SortProductsWithDirection(bool ascending)
        {
            if (ascending)
            {
                // Sort ascending by the appropriate price property for each product type
                Products = Products.OrderBy(p => GetProductPrice(p)).ToList();
            }
            else
            {
                // Sort descending by the appropriate price property for each product type
                Products = Products.OrderByDescending(p => GetProductPrice(p)).ToList();
            }
        }

        /// <summary>
        /// Gets the appropriate price value for a product based on its type.
        /// </summary>
        /// <param name="product">The product to get the price for.</param>
        /// <returns>The price value for sorting.</returns>
        private decimal GetProductPrice(Product product)
        {
            if (product is MarketMinds.Shared.Models.BuyProduct buyProduct)
            {
                return (decimal)buyProduct.Price;
            }
            else if (product is MarketMinds.Shared.Models.AuctionProduct auctionProduct)
            {
                return (decimal)auctionProduct.CurrentPrice;
            }
            else if (product is MarketMinds.Shared.Models.BorrowProduct borrowProduct)
            {
                return (decimal)borrowProduct.DailyRate;
            }
            else
            {
                return (decimal)product.Price; // Fallback to base price
            }
        }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="offset">The offset for pagination.</param>
        /// <param name="count">The number of products per page.</param>
        /// <param name="search">The search term for filtering products.</param>
        /// <param name="sortAscending">Whether to sort products by price in ascending order.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync(int offset = 0, int count = 0, string? search = null, bool? sortAscending = null)
        {
            try
            {
                if (User != null && User.UserType == (int)UserRole.Seller)
                {
                    Seller = await _sellerService.GetSellerByUser(User);

                    // Add debug logging if possible
                    Console.WriteLine($"Seller loaded: {Seller?.StoreName ?? "NULL"}");

                    if (Seller != null)
                    {
                        await LoadProducts(offset, count, search, sortAscending);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InitializeAsync: {ex.Message}");
                // Initialize empty Seller object to avoid null reference
                Seller = new Seller(User);
            }
        }


        /// <summary>
        /// Loads the seller's products with pagination, filtering, and sorting.
        /// </summary>
        /// <param name="offset">The offset for pagination.</param>
        /// <param name="count">The number of products per page.</param>
        /// <param name="search">The search term for filtering products.</param>
        /// <param name="sortAscending">Whether to sort products by price in ascending order.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadProducts(int offset = 0, int count = 0, string? search = null, bool? sortAscending = null)
        {
            if (Seller != null)
            {
                try
                {
                    int sellerId = Seller.Id;
                    int userId = Seller.User?.Id ?? 0;
                    Console.WriteLine($"DEBUG: Loading products for Seller ID: {sellerId}, Count: {count}, User ID: {userId} with pagination (offset: {offset}, count: {count}, search: '{search}')");

                    int borrowCount = 0;
                    int auctionCount = 0;
                    int buyCount = 0;

                    // Use server-side filtering with sellerId parameter - THE SMART PAGINATION!
                    Console.WriteLine($"DEBUG: About to call GetFilteredProductCount for Buy products with sellerId: {sellerId}, search: '{search}'");
                    try 
                    {
                        buyCount = ((MarketMinds.Shared.Services.BuyProductsService.BuyProductsService)_buyProductsService).GetFilteredProductCount(null, null, null, search, sellerId);
                        Console.WriteLine($"DEBUG: Buy products count result: {buyCount}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DEBUG: Exception in BuyProducts GetFilteredProductCount: {ex.Message}");
                    }
                    
                    Console.WriteLine($"DEBUG: About to call GetFilteredAuctionProductCountAsync with sellerId: {sellerId}, search: '{search}'");
                    try 
                    {
                        auctionCount = await _auctionProductService.GetFilteredAuctionProductCountAsync(null, null, null, search, sellerId);
                        Console.WriteLine($"DEBUG: Auction products count result: {auctionCount}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DEBUG: Exception in AuctionProducts GetFilteredAuctionProductCountAsync: {ex.Message}");
                    }
                    
                    Console.WriteLine($"DEBUG: About to call GetFilteredProductCount for Borrow products with sellerId: {sellerId}, search: '{search}'");
                    try 
                    {
                        borrowCount = _borrowProductsService.GetFilteredProductCount(conditionIds: null, categoryIds: null, maxPrice: null, searchTerm: search, sellerId: sellerId);
                        Console.WriteLine($"DEBUG: Borrow products count result: {borrowCount}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DEBUG: Exception in BorrowProducts GetFilteredProductCount: {ex.Message}");
                    }

                    // Calculate total counts without search filter
                    AllProductsCount = ((MarketMinds.Shared.Services.BuyProductsService.BuyProductsService)_buyProductsService).GetFilteredProductCount(null, null, null, null, sellerId) +
                                      await _auctionProductService.GetFilteredAuctionProductCountAsync(null, null, null, null, sellerId) +
                                      _borrowProductsService.GetFilteredProductCount(conditionIds: null, categoryIds: null, maxPrice: null, searchTerm: null, sellerId: sellerId);
                    
                    TotalProductCount = buyCount + auctionCount + borrowCount;
                    Console.WriteLine($"DEBUG: Total counts - Buy: {buyCount}, Auction: {auctionCount}, Borrow: {borrowCount}");
                    Console.WriteLine($"DEBUG: AllProductsCount: {AllProductsCount}, TotalProductCount: {TotalProductCount}");

                    var allSellerProducts = new List<Product>();

                    // If no pagination (count = 0), load all products using server-side filtering
                    if (count <= 0)
                    {
                        // Load all filtered products using server-side methods with sellerId
                        // var buyProducts = ((BuyProductsService.BuyProductsService)_buyProductsService).GetFilteredProducts(0, 0, null, null, null, search, sellerId);
                        // var auctionProducts = await _auctionProductService.GetFilteredAuctionProductsAsync(0, 0, null, null, null, search, sellerId);
                        var borrowProducts = _borrowProductsService.GetFilteredProducts(0, 0, null, null, null, search, sellerId);

                        // allSellerProducts.AddRange(buyProducts.Cast<Product>());
                        // allSellerProducts.AddRange(auctionProducts.Cast<Product>());
                        // allSellerProducts.AddRange(borrowProducts.Cast<Product>());

                        Console.WriteLine($"DEBUG: Loaded all products without pagination - Total: {allSellerProducts.Count}");
                    }
                    else
                    {
                        // Implement smart pagination across product types using server-side methods
                        // Order: Buy -> Auction -> Borrow
                        allSellerProducts = await LoadProductsWithServerSidePagination(sellerId, offset, count, search, buyCount, auctionCount, borrowCount);
                    }

                    // Apply sorting if requested
                    if (sortAscending.HasValue)
                    {
                        if (sortAscending.Value)
                        {
                            allSellerProducts = allSellerProducts.OrderBy(p => GetProductPrice(p)).ToList();
                            Console.WriteLine($"DEBUG: Applied ascending price sort to {allSellerProducts.Count} products");
                        }
                        else
                        {
                            allSellerProducts = allSellerProducts.OrderByDescending(p => GetProductPrice(p)).ToList();
                            Console.WriteLine($"DEBUG: Applied descending price sort to {allSellerProducts.Count} products");
                        }
                    }

                    _filteredProducts = allSellerProducts;
                    _allProducts = allSellerProducts; // For client-side operations if needed

                    Console.WriteLine($"DEBUG: Final result - showing {_filteredProducts.Count} products");
                    OnPropertyChanged(nameof(Products));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading products: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    TotalProductCount = 0;
                    AllProductsCount = 0;
                    _filteredProducts = new List<Product>();
                    _allProducts = new List<Product>();
                }
            }
        }

        /// <summary>
        /// Loads products with server-side pagination across different product types (Buy -> Auction -> Borrow).
        /// Uses the GetFilteredProducts methods with sellerId parameter - THE REAL SMART PAGINATION!
        /// </summary>
        /// <param name="sellerId">The seller ID to filter by.</param>
        /// <param name="offset">The starting offset for pagination.</param>
        /// <param name="count">The number of products to load.</param>
        /// <param name="search">The search term for filtering.</param>
        /// <param name="buyCount">Total count of buy products.</param>
        /// <param name="auctionCount">Total count of auction products.</param>
        /// <param name="borrowCount">Total count of borrow products.</param>
        /// <returns>A list of paginated products.</returns>
        private async Task<List<Product>> LoadProductsWithServerSidePagination(int sellerId, int offset, int count, string? search,
            int buyCount, int auctionCount, int borrowCount)
        {
            var result = new List<Product>();
            int remainingCount = count;
            int currentOffset = offset;

            Console.WriteLine($"DEBUG: LoadProductsWithServerSidePagination - sellerId: {sellerId}, offset: {offset}, count: {count}");
            Console.WriteLine($"DEBUG: Product counts - Buy: {buyCount}, Auction: {auctionCount}, Borrow: {borrowCount}");

            // Phase 1: Buy Products
            if (currentOffset < buyCount && remainingCount > 0)
            {
                int buyOffset = currentOffset;
                int buyTake = Math.Min(remainingCount, buyCount - currentOffset);
                
                Console.WriteLine($"DEBUG: Loading buy products - offset: {buyOffset}, count: {buyTake}");
                var buyProducts = ((MarketMinds.Shared.Services.BuyProductsService.BuyProductsService)_buyProductsService).GetFilteredProducts(buyOffset, buyTake, null, null, null, search, sellerId);
                result.AddRange(buyProducts.Cast<Product>());
                
                remainingCount -= buyProducts.Count();
                currentOffset = Math.Max(0, currentOffset - buyCount);
                
                Console.WriteLine($"DEBUG: Loaded {buyProducts.Count()} buy products, remaining: {remainingCount}");
            }
            else
            {
                currentOffset = Math.Max(0, currentOffset - buyCount);
            }

            // Phase 2: Auction Products
            if (currentOffset < auctionCount && remainingCount > 0)
            {
                int auctionOffset = currentOffset;
                int auctionTake = Math.Min(remainingCount, auctionCount - currentOffset);
                
                Console.WriteLine($"DEBUG: Loading auction products - offset: {auctionOffset}, count: {auctionTake}");
                var auctionProducts = await _auctionProductService.GetFilteredAuctionProductsAsync(auctionOffset, auctionTake, null, null, null, search, sellerId);
                result.AddRange(auctionProducts.Cast<Product>());
                
                remainingCount -= auctionProducts.Count();
                currentOffset = Math.Max(0, currentOffset - auctionCount);
                
                Console.WriteLine($"DEBUG: Loaded {auctionProducts.Count()} auction products, remaining: {remainingCount}");
            }
            else
            {
                currentOffset = Math.Max(0, currentOffset - auctionCount);
            }

            // Phase 3: Borrow Products
            if (currentOffset < borrowCount && remainingCount > 0)
            {
                int borrowOffset = currentOffset;
                int borrowTake = Math.Min(remainingCount, borrowCount - currentOffset);
                
                Console.WriteLine($"DEBUG: Loading borrow products - offset: {borrowOffset}, count: {borrowTake}");
                var borrowProducts = _borrowProductsService.GetFilteredProducts(borrowOffset, borrowTake, null, null, null, search, sellerId);
                result.AddRange(borrowProducts.Cast<Product>());
                
            }

            Console.WriteLine($"DEBUG: LoadProductsWithServerSidePagination complete - total loaded: {result.Count}");
            return result;
        }

        /// <summary>
        /// Updates the seller profile.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateSellerProfileAsync()
        {
            if (Seller != null)
            {
                try
                {
                    await _sellerService.UpdateSeller(Seller);
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error updating seller: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Notifies that a property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
