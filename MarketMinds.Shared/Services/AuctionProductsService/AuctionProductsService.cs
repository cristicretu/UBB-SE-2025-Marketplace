using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.ProductTagService;
using MarketMinds.Shared.Services.Interfaces;

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
            
            if (auctionProduct.StartTime == default(DateTime))
            {
                auctionProduct.StartTime = DateTime.Now;
            }
            
            if (auctionProduct.EndTime == default(DateTime))
            {
                auctionProduct.EndTime = DateTime.Now.AddDays(7);
            }
            
            if (auctionProduct.StartPrice <= MINIMUM_PRICE && auctionProduct.CurrentPrice > MINIMUM_PRICE)
            {
                auctionProduct.StartPrice = auctionProduct.CurrentPrice;
            }
            else if (auctionProduct.StartPrice > MINIMUM_PRICE && auctionProduct.CurrentPrice <= MINIMUM_PRICE)
            {
                auctionProduct.CurrentPrice = auctionProduct.StartPrice;
            }
            
            auctionProductsRepository.CreateListing(auctionProduct);
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
                throw new ArgumentNullException(nameof(auction), "Auction cannot be null");
            }
            
            if (auction.Id <= 0)
            {
                throw new InvalidOperationException("Cannot bid on an unsaved auction");
            }
            
            if (bidderId <= 0)
            {
                throw new InvalidOperationException("Cannot bid with an invalid user id");
            }
            
            if (bidderId == auction.SellerId)
            {
                throw new InvalidOperationException("You cannot bid on your own auction");
            }
            
            if (IsAuctionEnded(auction))
            {
                throw new InvalidOperationException("Cannot place bid on an ended auction");
            }
            
            if (DateTime.Now < auction.StartTime)
            {
                throw new InvalidOperationException($"Auction hasn't started yet. Starts at {auction.StartTime}");
            }
            
            double minimumBid = auction.Bids?.Count == NULL_BID_AMOUNT ? auction.StartPrice : auction.CurrentPrice + 1;

            if (bidAmount < minimumBid)
            {
                throw new InvalidOperationException($"Bid must be at least ${minimumBid}");
            }
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

        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
        {
            List<AuctionProduct> products = GetProducts();
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
                return GetProducts();
            }
            catch (Exception exception)
            {
                return new List<AuctionProduct>();
            }
        }

        public async Task<AuctionProduct> GetAuctionProductByIdAsync(int id)
        {
            try
            {
                return GetProductById(id);
            }
            catch (Exception exception)
            {
                return new AuctionProduct();
            }
        }

        public async Task<bool> CreateAuctionProductAsync(AuctionProduct auctionProduct)
        {
            try
            {
                SetDefaultAuctionTimes(auctionProduct);
                SetDefaultPricing(auctionProduct);
                CreateListing(auctionProduct);
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
                var auction = GetProductById(auctionId);
                if (auction == null || auction.Id == 0)
                {
                    throw new KeyNotFoundException($"Auction product with ID {auctionId} not found.");
                }
                
                ValidateBid(auction, bidderId, bidAmount);
                ProcessRefundForPreviousBidder(auction, bidAmount);
                ExtendAuctionTimeIfNeeded(auction);
                
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
                    
                    auctionProductsRepository.PlaceBid(auction, bidder, bidAmount);
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("Auction has already ended.");
                }
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAuctionProductAsync(AuctionProduct auctionProduct)
        {
            try
            {
                auctionProductsRepository.CreateListing(auctionProduct);
                return true;
            }
            catch (Exception exception)
            {
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
            if (product.StartTime == default || product.StartTime.Year < 2000)
            {
                product.StartTime = DateTime.Now;
            }
            
            if (product.EndTime == default || product.EndTime.Year < 2000)
            {
                product.EndTime = DateTime.Now.AddDays(7);
            }
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
    }
}
