using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketMinds.Shared.Services.AuctionProductsService
{
    public class AuctionProductsService : IAuctionProductsService, IProductService, IAuctionProductService
    {
        private readonly AuctionProductsProxyRepository auctionProductsRepository;

        private const int NULL_BID_AMOUNT = 0;
        private const int MAX_AUCTION_TIME = 5;
        private const double DEFAULT_MIN_PRICE = 1.0;
        private const int EMPTY_COLLECTION_COUNT = 0;
        private const double MINIMUM_PRICE = 0.0;

        public AuctionProductsService(AuctionProductsProxyRepository auctionProductsRepository)
        {
            this.auctionProductsRepository = auctionProductsRepository;
        }

        #region Synchronous Methods

        public void CreateListing(Product product)
        {
            if (!(product is AuctionProduct auctionProduct))
            {
                throw new ArgumentException("Product must be an AuctionProduct.", nameof(product));
            }

            Console.WriteLine($"TRACE: CreateListing received EndTime: {auctionProduct.EndTime}");

            if (auctionProduct.StartTime == default(DateTime))
            {
                auctionProduct.StartTime = DateTime.Now;
            }

            if (auctionProduct.EndTime == default(DateTime))
            {
                Console.WriteLine($"TRACE: CreateListing setting default EndTime (was default value)");
                auctionProduct.EndTime = DateTime.Now.AddDays(7);
            }
            else
            {
                Console.WriteLine($"TRACE: CreateListing keeping EndTime: {auctionProduct.EndTime}");
            }

            if (auctionProduct.StartPrice <= MINIMUM_PRICE && auctionProduct.CurrentPrice > MINIMUM_PRICE)
            {
                auctionProduct.StartPrice = auctionProduct.CurrentPrice;
            }
            else if (auctionProduct.StartPrice > MINIMUM_PRICE && auctionProduct.CurrentPrice <= MINIMUM_PRICE)
            {
                auctionProduct.CurrentPrice = auctionProduct.StartPrice;
            }

            Console.WriteLine($"TRACE: Before repository call EndTime: {auctionProduct.EndTime}");
            auctionProductsRepository.CreateListing(auctionProduct);
            Console.WriteLine($"TRACE: After repository call EndTime: {auctionProduct.EndTime}");
        }

        public void PlaceBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            ValidateBid(auction, bidder, bidAmount);

            try
            {
                var bid = new Bid(bidder.Id, auction.Id, bidAmount)
                {
                    Product = auction,
                    Bidder = bidder
                };

                double originalBalance = bidder.Balance;
                double originalPrice = auction.CurrentPrice;
                var originalBids = new List<Bid>(auction.Bids);

                bidder.Balance -= bidAmount;
                auction.AddBid(bid);
                auction.CurrentPrice = bidAmount;
                ExtendAuctionTime(auction);

                try
                {
                    auctionProductsRepository.PlaceBid(auction, bidder, bidAmount);
                }
                catch (Exception exception)
                {
                    bidder.Balance = originalBalance;
                    auction.CurrentPrice = originalPrice;
                    auction.Bids = originalBids;

                    throw new Exception($"Server rejected the bid: {exception.Message}", exception);
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Server rejected"))
                {
                    throw;
                }
                throw new Exception($"Failed to place bid: {exception.Message}", exception);
            }
        }

        public void ConcludeAuction(AuctionProduct auction)
        {
            if (auction.Id == 0)
            {
                throw new ArgumentException("Auction Product ID must be set for delete.", nameof(auction.Id));
            }

            auctionProductsRepository.ConcludeAuction(auction);
        }

        public string GetTimeLeft(AuctionProduct auction)
        {
            var timeLeft = auction.EndTime - DateTime.Now;
            string result = timeLeft > TimeSpan.Zero ? timeLeft.ToString(@"dd\:hh\:mm\:ss") : "Auction Ended";
            return result;
        }

        public bool IsAuctionEnded(AuctionProduct auction)
        {
            bool isEnded = DateTime.Now >= auction.EndTime;
            return isEnded;
        }

        public void ValidateBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            if (auction == null)
            {
                throw new ArgumentNullException(nameof(auction), "Auction cannot be null");
            }

            if (bidder == null)
            {
                throw new ArgumentNullException(nameof(bidder), "Bidder cannot be null");
            }

            if (auction.Id <= 0)
            {
                throw new Exception("Cannot bid on an unsaved auction");
            }

            if (bidder.Id <= 0)
            {
                throw new Exception("Cannot bid with an unsaved user profile");
            }

            if (bidder.Id == auction.SellerId)
            {
                throw new Exception("You cannot bid on your own auction");
            }

            if (IsAuctionEnded(auction))
            {
                throw new Exception("Auction already ended");
            }

            if (DateTime.Now < auction.StartTime)
            {
                throw new Exception($"Auction hasn't started yet. Starts at {auction.StartTime}");
            }

            double minimumBid = auction.Bids.Count == NULL_BID_AMOUNT ? auction.StartPrice : auction.CurrentPrice + 1;

            if (bidAmount < minimumBid)
            {
                throw new Exception($"Bid must be at least ${minimumBid}");
            }

            if (bidAmount > bidder.Balance)
            {
                throw new Exception("Insufficient balance");
            }
        }

        public void ValidateBid(AuctionProduct auction, int bidderId, double bidAmount)
        {
            if (auction == null)
            {
                Console.WriteLine("TRACE: ValidateBid - Auction is null");
                throw new ArgumentNullException(nameof(auction), "Auction cannot be null");
            }

            if (auction.Id <= 0)
            {
                Console.WriteLine("TRACE: ValidateBid - Invalid auction ID");
                throw new InvalidOperationException("Cannot bid on an unsaved auction");
            }

            if (bidderId <= 0)
            {
                Console.WriteLine("TRACE: ValidateBid - Invalid bidder ID");
                throw new InvalidOperationException("Cannot bid with an invalid user id");
            }

            if (bidderId == auction.SellerId)
            {
                Console.WriteLine($"TRACE: ValidateBid - Bidder ({bidderId}) is the seller ({auction.SellerId})");
                throw new InvalidOperationException("You cannot bid on your own auction");
            }

            if (IsAuctionEnded(auction))
            {
                Console.WriteLine($"TRACE: ValidateBid - Auction has ended (EndTime={auction.EndTime}, Now={DateTime.Now})");
                throw new InvalidOperationException("Cannot place bid on an ended auction");
            }

            if (DateTime.Now < auction.StartTime)
            {
                Console.WriteLine($"TRACE: ValidateBid - Auction hasn't started yet (StartTime={auction.StartTime}, Now={DateTime.Now})");
                throw new InvalidOperationException($"Auction hasn't started yet. Starts at {auction.StartTime}");
            }

            // Calculate minimum bid
            double minimumBid = auction.CurrentPrice + 1;
            if (auction.Bids == null || auction.Bids.Count == 0)
            {
                minimumBid = auction.StartPrice;
            }

            Console.WriteLine($"TRACE: ValidateBid - Minimum bid is {minimumBid}, attempted bid is {bidAmount}");
            if (bidAmount < minimumBid)
            {
                throw new InvalidOperationException($"Bid must be at least ${minimumBid}");
            }

            // Note: We don't have direct access to the bidder's balance in this method
            // since we only have the bidder ID. The balance check would need to be implemented
            // at the API endpoint that provides the user's balance information.
            Console.WriteLine($"TRACE: ValidateBid - Bid is valid");
        }

        public void ExtendAuctionTime(AuctionProduct auction)
        {
            var timeRemaining = auction.EndTime - DateTime.Now;

            if (timeRemaining.TotalMinutes < MAX_AUCTION_TIME)
            {
                var oldEndTime = auction.EndTime;
                auction.EndTime = DateTime.Now.AddMinutes(MAX_AUCTION_TIME);
            }
        }

        public List<AuctionProduct> GetProducts()
        {
            try
            {
                return auctionProductsRepository.GetProducts();
            }
            catch (Exception exception)
            {
                string innerMessage = exception.InnerException?.Message ?? exception.Message;
                throw new Exception($"Error retrieving products: {innerMessage}", exception);
            }
        }

        public AuctionProduct GetProductById(int id)
        {
            try
            {
                var product = auctionProductsRepository.GetProductById(id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Auction product with ID {id} not found.");
                }
                return product;
            }
            catch (Exception exception)
            {
                throw new KeyNotFoundException($"Auction product with ID {id} not found: {exception.Message}");
            }
        }

        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType? sortCondition, string searchQuery)
        {
            List<Product> products = GetProducts().Cast<Product>().ToList();
            List<Product> productResultSet = new List<Product>();
            foreach (Product product in products)
            {
                bool matchesConditions = selectedConditions == null || selectedConditions.Count == EMPTY_COLLECTION_COUNT || selectedConditions.Any(condition => condition.Id == product.Condition.Id);
                bool matchesCategories = selectedCategories == null || selectedCategories.Count == EMPTY_COLLECTION_COUNT || selectedCategories.Any(category => category.Id == product.Category.Id);
                bool matchesTags = selectedTags == null || selectedTags.Count == EMPTY_COLLECTION_COUNT || selectedTags.Any(tag => product.Tags.Any(productTag => productTag.Id == tag.Id));
                bool matchesSearchQuery = string.IsNullOrEmpty(searchQuery) || product.Title.ToLower().Contains(searchQuery.ToLower());

                if (matchesConditions && matchesCategories && matchesTags && matchesSearchQuery)
                {
                    productResultSet.Add(product);
                }
            }

            if (sortCondition != null)
            {
                if (sortCondition.IsAscending)
                {
                    productResultSet = productResultSet.OrderBy(
                        product =>
                        {
                            var property = product?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return property?.GetValue(product);
                        }).ToList();
                }
                else
                {
                    productResultSet = productResultSet.OrderByDescending(
                        product =>
                        {
                            var property = product?.GetType().GetProperty(sortCondition.InternalAttributeFieldTitle);
                            return property?.GetValue(product);
                        }).ToList();
                }
            }

            return productResultSet;
        }

        #endregion

        #region Async Methods

        public async Task<List<AuctionProduct>> GetAllAuctionProductsAsync()
        {
            try
            {
                return await auctionProductsRepository.GetAllAuctionProductsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all auction products: {ex.Message}");
                return new List<AuctionProduct>();
            }
        }

        public async Task<List<AuctionProduct>> GetAllAuctionProductsAsync(int offset, int count)
        {
            try
            {
                return await auctionProductsRepository.GetAllAuctionProductsAsync(offset, count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting auction products with pagination (offset: {offset}, count: {count}): {ex.Message}");
                return new List<AuctionProduct>();
            }
        }

        public async Task<int> GetAuctionProductCountAsync()
        {
            try
            {
                return await auctionProductsRepository.GetAuctionProductCountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting auction product count: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<AuctionProduct>> GetFilteredAuctionProductsAsync(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                return await auctionProductsRepository.GetFilteredAuctionProductsAsync(offset, count, conditionIds, categoryIds, maxPrice, searchTerm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered auction products: {ex.Message}");
                return new List<AuctionProduct>();
            }
        }

        public async Task<int> GetFilteredAuctionProductCountAsync(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                return await auctionProductsRepository.GetFilteredAuctionProductCountAsync(conditionIds, categoryIds, maxPrice, searchTerm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered auction product count: {ex.Message}");
                return 0;
            }
        }

        public async Task<AuctionProduct> GetAuctionProductByIdAsync(int id)
        {
            try
            {
                return await Task.Run(() => auctionProductsRepository.GetProductById(id));
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to retrieve auction product by ID {id}: {exception.Message}", exception);
            }
        }

        public async Task<bool> CreateAuctionProductAsync(AuctionProduct auctionProduct)
        {
            try
            {
                Console.WriteLine($"TRACE: CreateAuctionProductAsync received EndTime: {auctionProduct.EndTime}");
                SetDefaultAuctionTimes(auctionProduct);
                Console.WriteLine($"TRACE: After SetDefaultAuctionTimes EndTime: {auctionProduct.EndTime}");
                SetDefaultPricing(auctionProduct);
                CreateListing(auctionProduct);
                Console.WriteLine($"TRACE: After CreateListing EndTime: {auctionProduct.EndTime}");
                return true;
            }
            catch (Exception exception)
            {
                throw new Exception($"Error creating auction product: {exception.Message}", exception);
            }
        }

        public async Task<bool> PlaceBidAsync(int auctionId, int bidderId, double bidAmount)
        {
            try
            {
                Console.WriteLine($"TRACE: PlaceBidAsync start - auctionId={auctionId}, bidderId={bidderId}, bidAmount={bidAmount}");
                var auction = GetProductById(auctionId);
                if (auction == null || auction.Id == 0)
                {
                    Console.WriteLine($"TRACE: PlaceBidAsync failed - Auction not found with ID {auctionId}");
                    throw new KeyNotFoundException($"Auction product with ID {auctionId} not found.");
                }

                Console.WriteLine($"TRACE: PlaceBidAsync - Auction found: ID={auction.Id}, CurrentPrice={auction.CurrentPrice}, EndTime={auction.EndTime}");
                Console.WriteLine($"TRACE: PlaceBidAsync - Validating bid");

                try
                {
                    ValidateBid(auction, bidderId, bidAmount);
                }
                catch (Exception validateEx)
                {
                    Console.WriteLine($"TRACE: PlaceBidAsync - Validation failed: {validateEx.GetType().Name}: {validateEx.Message}");
                    throw;
                }

                Console.WriteLine($"TRACE: PlaceBidAsync - Processing refund for previous bidder");
                ProcessRefundForPreviousBidder(auction, bidAmount);

                Console.WriteLine($"TRACE: PlaceBidAsync - Extending auction time if needed");
                ExtendAuctionTimeIfNeeded(auction);

                Console.WriteLine($"TRACE: PlaceBidAsync - Current EndTime={auction.EndTime}, Now={DateTime.Now}");
                if (auction.EndTime > DateTime.Now)
                {
                    var bid = new Bid
                    {
                        BidderId = bidderId,
                        ProductId = auctionId,
                        Price = bidAmount,
                        Timestamp = DateTime.Now
                    };

                    var bidder = new User { Id = bidderId };

                    Console.WriteLine($"TRACE: PlaceBidAsync - Calling repository PlaceBid");
                    try
                    {
                        auctionProductsRepository.PlaceBid(auction, bidder, bidAmount);
                        Console.WriteLine($"TRACE: PlaceBidAsync - Bid placed successfully");
                        return true;
                    }
                    catch (InvalidOperationException repoEx) when (repoEx.Message.Contains("buyer") || repoEx.Message.Contains("permission"))
                    {
                        Console.WriteLine($"TRACE: PlaceBidAsync - User role error: {repoEx.Message}");
                        throw; // Re-throw to propagate to caller
                    }
                    catch (Exception repoEx)
                    {
                        Console.WriteLine($"TRACE: PlaceBidAsync - Repository error: {repoEx.GetType().Name}: {repoEx.Message}");
                        throw;
                    }
                }
                else
                {
                    Console.WriteLine($"TRACE: PlaceBidAsync - Auction has already ended: EndTime={auction.EndTime}, Now={DateTime.Now}");
                    throw new InvalidOperationException("Auction has already ended.");
                }
            }
            catch (InvalidOperationException) when (DateTime.Now >= GetProductById(auctionId).EndTime)
            {
                Console.WriteLine($"TRACE: PlaceBidAsync - Exception caught: Auction has ended");
                return false;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("buyer") || ex.Message.Contains("permission"))
            {
                // Let this specific error propagate up to be handled specially by the controller
                Console.WriteLine($"TRACE: PlaceBidAsync - Buyer role error propagated: {ex.Message}");
                throw;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"TRACE: PlaceBidAsync - Exception caught: {exception.GetType().Name}: {exception.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAuctionProductAsync(AuctionProduct auctionProduct)
        {
            try
            {
                auctionProductsRepository.UpdateAuctionProduct(auctionProduct);
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"TRACE: UpdateAuctionProductAsync failed: {exception.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAuctionProductAsync(int id)
        {
            try
            {
                var product = GetProductById(id);
                ConcludeAuction(product);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        #endregion

        #region Additional Business Logic Methods

        public void ExtendAuctionTimeIfNeeded(AuctionProduct auction)
        {
            var timeRemaining = auction.EndTime - DateTime.Now;

            if (timeRemaining.TotalMinutes < MAX_AUCTION_TIME)
            {
                var oldEndTime = auction.EndTime;
                auction.EndTime = DateTime.Now.AddMinutes(MAX_AUCTION_TIME);
            }
        }

        public void SetDefaultAuctionTimes(AuctionProduct product)
        {
            Console.WriteLine($"TRACE: SetDefaultAuctionTimes received EndTime: {product.EndTime}");
            Console.WriteLine($"TRACE: Conditions - Default: {product.EndTime == default}, Year < 2000: {product.EndTime.Year < 2000}, Earlier than now: {product.EndTime < DateTime.Now}");

            // Only set default start time if it's not set or is an invalid date
            if (product.StartTime == default || product.StartTime.Year < 2000)
            {
                product.StartTime = DateTime.Now;
            }

            // Only set default end time if it's not set, is an invalid date, or is in the past
            // This allows users to set future end dates
            if (product.EndTime == default || product.EndTime.Year < 2000 || product.EndTime < DateTime.Now)
            {
                Console.WriteLine($"TRACE: Setting default EndTime (original: {product.EndTime})");
                product.EndTime = DateTime.Now.AddDays(7);
            }
            else
            {
                Console.WriteLine($"TRACE: Keeping original EndTime: {product.EndTime}");
            }

            Console.WriteLine($"TRACE: SetDefaultAuctionTimes final EndTime: {product.EndTime}");
        }

        public void SetDefaultPricing(AuctionProduct product)
        {
            if (product.StartPrice <= MINIMUM_PRICE)
            {
                if (product.CurrentPrice > MINIMUM_PRICE)
                {
                    product.StartPrice = product.CurrentPrice;
                }
                else
                {
                    product.StartPrice = DEFAULT_MIN_PRICE;
                    product.CurrentPrice = DEFAULT_MIN_PRICE;
                }
            }
            else if (product.CurrentPrice <= MINIMUM_PRICE)
            {
                product.CurrentPrice = product.StartPrice;
            }
        }

        public void ProcessRefundForPreviousBidder(AuctionProduct product, double newBidAmount)
        {
            if (product.Bids != null && product.Bids.Any())
            {
                var highestBid = product.Bids.OrderByDescending(b => b.Price).FirstOrDefault();
                if (highestBid != null)
                {
                    int previousBidderId = highestBid.BidderId;
                    double previousBidAmount = highestBid.Price;
                }
            }
        }
        #endregion

        // merge-nicusor
        Task<Product> IProductService.GetProductByIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        Task<string> IProductService.GetSellerNameAsync(int? sellerId)
        {
            // Not implemented in auction products service
            return Task.FromResult<string>(null);
        }

        /// <summary>
        /// Gets a list of products that can be borrowed.
        /// Implementation for IProductService interface.
        /// </summary>
        /// <returns>A list of borrowable products.</returns>
        public async Task<List<Product>> GetBorrowableProductsAsync()
        {
            // Since this is AuctionProductsService, we don't have borrowable products
            // Return an empty list when this method is called on this service
            return new List<Product>();
        }

        public async Task<double> GetMaxPriceAsync()
        {
            try
            {
                return await auctionProductsRepository.GetMaxPriceAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting max price for auction products: {ex.Message}");
                return 0.0; // Return 0 on error
            }
        }
    }
}
