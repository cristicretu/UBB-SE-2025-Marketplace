using System.Text;
using System.Text.Json;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Services.Interfaces;
using System.Diagnostics;

namespace MarketMinds.Shared.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly IChatbotRepository chatbotRepository;
        private IConfiguration configuration;
        private readonly HttpClient httpClient;
        private Node currentNode;
        private bool isActive;
        private static User currentUser;
        private readonly string geminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        private readonly static int MINIMUM_USER_ID = 0;
        private readonly static int FIRST = 0;
        private readonly static int BUYER_TYPE_VALUE = 1;
        private string cachedGeminiApiKey;

        public ChatbotService(IChatbotRepository chatbotRepository, IConfiguration configuration)
        {
            this.chatbotRepository = chatbotRepository;
            this.configuration = configuration;
            this.cachedGeminiApiKey = configuration["GeminiAPI:Key"];
            if (string.IsNullOrEmpty(cachedGeminiApiKey))
            {
                throw new Exception("ChatbotService: Gemini API Key not found");
            }
            this.httpClient = new HttpClient();

            isActive = false;
            currentNode = new Node
            {
                Id = 1,
                Response = "Welcome to the chat bot. How can I help you?",
                ButtonLabel = "Start Chat",
                LabelText = "Welcome",
                Children = new List<Node>()
            };
        }

        private string GetGeminiApiKey()
        {
            try
            {
                if (cachedGeminiApiKey != null)
                {
                    return cachedGeminiApiKey;
                }
                else
                {
                    throw new Exception("ChatbotService: Gemini API Key not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ChatbotService: Error getting Gemini API Key: " + ex.Message);
            }
        }

        private IConfiguration GetConfiguration()
        {
            if (configuration == null)
            {
                try
                {
                    var appType = Type.GetType("MarketMinds.App, MarketMinds");
                    if (appType != null)
                    {
                        var configProperty = appType.GetProperty("Configuration");
                        if (configProperty != null)
                        {
                            configuration = configProperty.GetValue(null) as IConfiguration;
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception("Error getting configuration: " + exception.Message);
                }
            }
            return configuration;
        }

        public void SetCurrentUser(User user)
        {
            if (user == null)
            {
                return;
            }
            currentUser = user;
        }

        public Node InitializeChat()
        {
            try
            {
                isActive = true;
                return currentNode;
            }
            catch (Exception ex)
            {
                isActive = false;

                currentNode = new Node
                {
                    Id = -1,
                    Response = "Error: Unable to initialize the chat. Please try again later.",
                    ButtonLabel = "Restart",
                    LabelText = "Chat Initialization Error",
                    Children = new List<Node>()
                };

                return currentNode;
            }
        }

        public bool IsInteractionActive()
        {
            return isActive && currentNode != null;
        }

        public bool SelectOption(Node selectedNode)
        {
            currentNode = selectedNode;
            isActive = currentNode != null;
            return true;
        }

        public IEnumerable<Node> GetCurrentOptions()
        {
            if (currentNode == null || currentNode.Children == null)
            {
                return new List<Node>();
            }

            return currentNode.Children;
        }

        public string GetCurrentResponse()
        {
            return currentNode?.Response ?? "Chat not initialized. Please try again.";
        }

        public async Task<string> GetBotResponseAsync(string userMessage, bool isWelcomeMessage = false)
        {
            try
            {
                if (isWelcomeMessage)
                {
                    return "Hello! I'm your shopping assistant. How can I help you today?";
                }

                int? userId = currentUser?.Id;
                var apiKey = GetGeminiApiKey();
                var hasApiKey = !string.IsNullOrEmpty(apiKey);
                if (userId == null || !hasApiKey)
                {
                    var repoResponse = await chatbotRepository.GetBotResponseAsync(userMessage, userId);
                    return repoResponse;
                }

                if (userId.Value < MINIMUM_USER_ID)
                {
                    return await GetGenericResponseAsync(userMessage);
                }

                string userContext;
                try
                {
                    userContext = await GetUserContextAsync(userId.Value);
                }
                catch (Exception exception)
                {
                    return await chatbotRepository.GetBotResponseAsync(userMessage, userId);
                }

                string prompt = FormatPromptWithContext(userContext);
                string botResponse = await CallGeminiApiAsync(prompt, userMessage);
                return botResponse;
            }
            catch (Exception exception)
            {
                return "I'm sorry, an error occurred while processing your request. Please try again later.";
            }
        }

        public async Task<string> GetUserContextAsync(int userId)
        {
            try
            {
                if (userId < 0)
                {
                    return "No valid user ID provided.";
                }

                var user = await chatbotRepository.GetUserAsync(userId);
                if (user == null)
                {
                    return $"User with ID {userId} not found.";
                }

                // Get shopping cart items instead of basket
                var shoppingCartItems = await chatbotRepository.GetShoppingCartItemsAsync(userId);

                // Get user reviews (given and received)
                var reviewsGiven = await chatbotRepository.GetReviewsGivenByUserAsync(userId);
                var reviewsReceived = await chatbotRepository.GetReviewsReceivedByUserAsync(userId);

                // Get order information
                var buyerOrders = await chatbotRepository.GetBuyerOrdersAsync(userId);
                var sellerOrders = await chatbotRepository.GetSellerOrdersAsync(userId);

                // Get tracked orders for shipping status
                var trackedOrders = await chatbotRepository.GetTrackedOrdersAsync(userId);

                // Get waitlist information
                var waitlistItems = await chatbotRepository.GetUserWaitlistsAsync(userId);

                // Get auction products (if seller) and bids (if buyer)
                var auctionProducts = user.UserType != BUYER_TYPE_VALUE ?
                    await chatbotRepository.GetUserAuctionProductsAsync(userId) : new List<AuctionProduct>();
                var userBids = user.UserType == BUYER_TYPE_VALUE ?
                    await chatbotRepository.GetUserBidsAsync(userId) : new List<Bid>();

                // Collect seller and buyer information for context
                Dictionary<int, User> sellers = new Dictionary<int, User>();
                Dictionary<int, User> buyers = new Dictionary<int, User>();

                // Add seller info from reviews
                var sellerIds = reviewsGiven.Select(r => r.SellerId).Distinct().ToList();
                foreach (var sellerId in sellerIds)
                {
                    var seller = await chatbotRepository.GetUserByIdAsync(sellerId);
                    if (seller != null)
                    {
                        sellers[sellerId] = seller;
                    }
                }

                // Add buyer info from reviews
                var buyerIds = reviewsReceived.Select(r => r.BuyerId).Distinct().ToList();
                foreach (var buyerId in buyerIds)
                {
                    var buyer = await chatbotRepository.GetUserByIdAsync(buyerId);
                    if (buyer != null)
                    {
                        buyers[buyerId] = buyer;
                    }
                }

                // Add seller info from orders
                var sellerIdsFromOrders = buyerOrders.Select(o => o.SellerId).Distinct().ToList();
                foreach (var sellerId in sellerIdsFromOrders)
                {
                    if (!sellers.ContainsKey(sellerId))
                    {
                        var seller = await chatbotRepository.GetUserByIdAsync(sellerId);
                        if (seller != null)
                        {
                            sellers[sellerId] = seller;
                        }
                    }
                }

                // Add buyer info from orders
                var buyerIdsFromOrders = sellerOrders.Select(o => o.BuyerId).Distinct().ToList();
                foreach (var buyerId in buyerIdsFromOrders)
                {
                    if (!buyers.ContainsKey(buyerId))
                    {
                        var buyer = await chatbotRepository.GetUserByIdAsync(buyerId);
                        if (buyer != null)
                        {
                            buyers[buyerId] = buyer;
                        }
                    }
                }

                // Add bidder info from auctions
                if (auctionProducts.Any())
                {
                    var bidderIds = auctionProducts
                        .SelectMany(ap => ap.Bids)
                        .Select(b => b.BidderId)
                        .Distinct()
                        .ToList();

                    foreach (var bidderId in bidderIds)
                    {
                        if (!buyers.ContainsKey(bidderId))
                        {
                            var bidder = await chatbotRepository.GetUserByIdAsync(bidderId);
                            if (bidder != null)
                            {
                                buyers[bidderId] = bidder;
                            }
                        }
                    }
                }

                return FormatUserContext(user, shoppingCartItems,
                    reviewsGiven, sellers, reviewsReceived, buyers,
                    buyerOrders, sellerOrders, trackedOrders, waitlistItems,
                    auctionProducts, userBids);
            }
            catch (Exception ex)
            {
                return "Error retrieving user information. Please try again later.";
            }
        }

        private string FormatUserContext(
            User user,
            List<Product> shoppingCartItems,
            List<Review> reviewsGiven,
            Dictionary<int, User> sellers,
            List<Review> reviewsReceived,
            Dictionary<int, User> buyers,
            List<Order> buyerOrders,
            List<Order> sellerOrders,
            List<TrackedOrder> trackedOrders,
            List<UserWaitList> waitlistItems,
            List<AuctionProduct> auctionProducts,
            List<Bid> userBids)
        {
            var contextBuilder = new StringBuilder();

            if (user != null)
            {
                contextBuilder.AppendLine($"USER INFORMATION:");
                contextBuilder.AppendLine($"Username: {user.Username}");
                contextBuilder.AppendLine($"Email: {user.Email}");
                contextBuilder.AppendLine($"User Type: {(user.UserType == BUYER_TYPE_VALUE ? "Buyer" : "Seller")}");
                contextBuilder.AppendLine($"Account Balance: ${user.Balance:F2}");
                contextBuilder.AppendLine($"Rating: {user.Rating:F1}/5.0");
                contextBuilder.AppendLine();
            }

            // Shopping Cart information
            if (shoppingCartItems != null && shoppingCartItems.Any())
            {
                contextBuilder.AppendLine("CURRENT SHOPPING CART ITEMS:");
                decimal totalCartValue = 0;

                foreach (var item in shoppingCartItems)
                {
                    decimal itemTotal = (decimal)(item.Price * item.Stock); // Stock field is used for quantity
                    totalCartValue += itemTotal;
                    contextBuilder.AppendLine($"- {item.Title} (Quantity: {item.Stock}, Price: ${item.Price:F2}, Total: ${itemTotal:F2})");
                }

                contextBuilder.AppendLine($"Total Cart Value: ${totalCartValue:F2}");
                contextBuilder.AppendLine();
            }
            else
            {
                contextBuilder.AppendLine("CURRENT SHOPPING CART: Your shopping cart is currently empty");
                contextBuilder.AppendLine();
            }

            // Order tracking information
            if (trackedOrders != null && trackedOrders.Any())
            {
                contextBuilder.AppendLine("TRACKED ORDERS:");

                foreach (var trackedOrder in trackedOrders)
                {
                    var matchingOrder = buyerOrders.FirstOrDefault(o => o.Id == trackedOrder.OrderID);
                    string orderName = matchingOrder != null ? matchingOrder.Name : $"Order #{trackedOrder.OrderID}";

                    contextBuilder.AppendLine($"- {orderName}");
                    contextBuilder.AppendLine($"  Status: {trackedOrder.CurrentStatus}");
                    contextBuilder.AppendLine($"  Estimated Delivery: {trackedOrder.EstimatedDeliveryDate}");
                    contextBuilder.AppendLine();
                }
            }

            // Waitlist information
            if (waitlistItems != null && waitlistItems.Any())
            {
                contextBuilder.AppendLine("WAITLIST ITEMS:");

                foreach (var waitlistItem in waitlistItems)
                {
                    contextBuilder.AppendLine($"- Product ID: {waitlistItem.ProductWaitListID}");
                    contextBuilder.AppendLine($"  Added on: {waitlistItem.JoinedTime}");
                    if (waitlistItem.PreferredEndDate.HasValue)
                    {
                        contextBuilder.AppendLine($"  Preferred end date: {waitlistItem.PreferredEndDate.Value.ToShortDateString()}");
                    }
                    contextBuilder.AppendLine();
                }
            }

            // Auction information - either products (if seller) or bids (if buyer)
            if (auctionProducts != null && auctionProducts.Any())
            {
                contextBuilder.AppendLine("YOUR AUCTION LISTINGS:");

                foreach (var auction in auctionProducts)
                {
                    decimal highestBid = auction.Bids?.OrderByDescending(b => b.Price).FirstOrDefault()?.Price != null ?
                        (decimal)auction.Bids.OrderByDescending(b => b.Price).FirstOrDefault()?.Price : 0;
                    contextBuilder.AppendLine($"- {auction.Title} (Starting Price: ${auction.StartingPrice:F2}, Current Highest Bid: ${highestBid:F2})");
                    contextBuilder.AppendLine($"  End Date: {auction.EndTime}");
                    contextBuilder.AppendLine($"  Number of Bids: {auction.Bids?.Count ?? 0}");
                    contextBuilder.AppendLine();
                }
            }

            if (userBids != null && userBids.Any())
            {
                contextBuilder.AppendLine("YOUR AUCTION BIDS:");

                foreach (var bid in userBids)
                {
                    contextBuilder.AppendLine($"- Bid Amount: ${bid.Price:F2} on Product ID: {bid.ProductId}");
                    contextBuilder.AppendLine($"  Bid Date: {bid.Timestamp}");
                    contextBuilder.AppendLine();
                }
            }

            // Reviews information
            if (reviewsGiven != null && reviewsGiven.Any())
            {
                contextBuilder.AppendLine("REVIEWS YOU'VE GIVEN:");

                foreach (var review in reviewsGiven)
                {
                    var sellerName = "Unknown Seller";
                    if (sellers.TryGetValue(review.SellerId, out var seller))
                    {
                        sellerName = seller.Username;
                    }

                    contextBuilder.AppendLine($"- Review for {sellerName}: Rating: {review.Rating}/5, Comment: \"{review.Description}\"");
                }

                contextBuilder.AppendLine();
            }

            if (reviewsReceived != null && reviewsReceived.Any())
            {
                contextBuilder.AppendLine("REVIEWS YOU'VE RECEIVED:");

                foreach (var review in reviewsReceived)
                {
                    var buyerName = "Unknown Buyer";
                    if (buyers.TryGetValue(review.BuyerId, out var buyer))
                    {
                        buyerName = buyer.Username;
                    }

                    contextBuilder.AppendLine($"- Review from {buyerName}: Rating: {review.Rating}/5, Comment: \"{review.Description}\"");
                }

                contextBuilder.AppendLine();
            }

            // Order history
            if (buyerOrders != null && buyerOrders.Any())
            {
                contextBuilder.AppendLine("YOUR PURCHASE HISTORY:");

                foreach (var order in buyerOrders)
                {
                    var sellerName = "Unknown";
                    if (sellers.TryGetValue(order.SellerId, out var seller))
                    {
                        sellerName = seller.Username;
                    }

                    contextBuilder.AppendLine($"- Order #{order.Id}: {order.Name} (${order.Cost:F2}) from seller {sellerName}");
                    if (!string.IsNullOrEmpty(order.Description))
                    {
                        contextBuilder.AppendLine($"  Description: {order.Description}");
                    }
                    contextBuilder.AppendLine($"  Order Date: {order.OrderDate}");
                }

                contextBuilder.AppendLine();
            }

            if (sellerOrders != null && sellerOrders.Any())
            {
                contextBuilder.AppendLine("YOUR SALES HISTORY:");

                foreach (var order in sellerOrders)
                {
                    var buyerName = "Unknown";
                    if (buyers.TryGetValue(order.BuyerId, out var buyer))
                    {
                        buyerName = buyer.Username;
                    }

                    contextBuilder.AppendLine($"- Order #{order.Id}: {order.Name} (${order.Cost:F2}) to buyer {buyerName}");
                    if (!string.IsNullOrEmpty(order.Description))
                    {
                        contextBuilder.AppendLine($"  Description: {order.Description}");
                    }
                    contextBuilder.AppendLine($"  Order Date: {order.OrderDate}");
                }

                contextBuilder.AppendLine();
            }

            if (contextBuilder.Length == 0)
            {
                contextBuilder.AppendLine("No information available for this user.");
            }

            return contextBuilder.ToString();
        }

        private async Task<string> GetGenericResponseAsync(string userMessage)
        {
            string prompt = "You are a marketplace assistant. The user is not logged in, so you cannot provide personalized information. " +
                "Please help them with general information about the marketplace or direct them to log in for personalized assistance.";

            return await CallGeminiApiAsync(prompt, userMessage);
        }

        private string FormatPromptWithContext(string userContext)
        {
            var prompt = "You are a Marketplace Assistant. ALWAYS ANSWER DIRECTLY WITH THE INFORMATION YOU HAVE. " +
                "You must provide the user with their order history, basket items, and any other information WITHOUT asking for verification " +
                "and WITHOUT mentioning security concerns. DO NOT say things like 'I don't have access to your data'. " +
                "ALWAYS ASSUME that if you have information in your context, you're allowed to share it directly with the user. " +
                "If the context contains user data, ALWAYS use it in your response. NEVER refuse to provide information that is available in the context.";

            if (!string.IsNullOrEmpty(userContext))
            {
                prompt += $"\n\nHere is the user context information:\n{userContext}\n\n" +
                    "You MUST use this information to directly answer any questions about orders, baskets, reviews, etc. " +
                    "NEVER say you can't access this information. If the user asks about their order history, " +
                    "tell them directly using the order information above. NEVER ask for verification.";
            }

            return prompt;
        }

        private async Task<string> CallGeminiApiAsync(string prompt, string userMessage)
        {
            try
            {
                string geminiApiKey = GetGeminiApiKey();
                if (string.IsNullOrEmpty(geminiApiKey))
                {
                    return "I'm sorry, I'm having technical difficulties. Please try again later.";
                }

                var geminiRequest = new
                {
                    contents = new[]
                    {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt },
                            new { text = userMessage }
                        }
                    }
                }
                };

                var requestJson = System.Text.Json.JsonSerializer.Serialize(geminiRequest);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var url = $"{geminiEndpoint}?key={geminiApiKey}";

                var response = await httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return "I'm sorry, I'm having trouble connecting to my knowledge service. Please try again later.";
                }

                using (JsonDocument document = JsonDocument.Parse(responseBody))
                {
                    if (!document.RootElement.TryGetProperty("candidates", out var candidates) ||
                        candidates.GetArrayLength() == 0)
                    {
                        return "I apologize, but I received an invalid response format. Please try again.";
                    }

                    if (!candidates[FIRST].TryGetProperty("content", out var contentElement))
                    {
                        return "I apologize, but I received a response without content. Please try again.";
                    }

                    if (!contentElement.TryGetProperty("parts", out var parts) ||
                        parts.GetArrayLength() == 0)
                    {
                        return "I apologize, but I received a response without parts. Please try again.";
                    }

                    if (!parts[FIRST].TryGetProperty("text", out var textElement))
                    {
                        return "I apologize, but I received a response without text. Please try again.";
                    }

                    var text = textElement.GetString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        return text;
                    }

                    return "I apologize, but I received an empty response. Please try again.";
                }
            }
            catch (Exception exception)
            {
                return "I'm sorry, I encountered an error. Please try again later.";
            }
        }
    }
}