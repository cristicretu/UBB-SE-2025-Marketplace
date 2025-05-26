using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using System.Threading.Tasks;

namespace MarketMinds.Tests.Services.ChatBotServiceTest
{
    public class ChatBotRepositoryMock : IChatbotRepository
    {
        private Node _rootNode;
        private int _loadCount;

//        public ChatBotRepositoryMock()
//        {
//            _loadCount = 0;
//            InitializeTestNodes();
//        }

//        public Node LoadChatTree()
//        {
//            _loadCount++;
//            return _rootNode;
//        }

        public int GetLoadCount() => _loadCount;

        private void InitializeTestNodes()
        {
            _rootNode = new Node
            {
                Id = 1,
                ButtonLabel = "Start",
                LabelText = "Welcome",
                Response = "Welcome to the chat service",
                Children = new List<Node>
                {
                    new Node { Id = 2, ButtonLabel = "Option 1", LabelText = "Help", Response = "How can I help you?", Children = new List<Node>() },
                    new Node { Id = 3, ButtonLabel = "Option 2", LabelText = "Info", Response = "Here's some information", Children = new List<Node>() },
                    new Node { Id = -1, ButtonLabel = "Error", LabelText = "Error", Response = "Error occurred", Children = new List<Node>() }
                }
            };
        }

        public Task<string> GetBotResponseAsync(string userMessage, int? userId = null) => Task.FromResult("Mock response");
        public Task<string> GetUserContextAsync(int userId) => Task.FromResult("Mock user context");
        public Task<User> GetUserAsync(int userId) => Task.FromResult(new User { Id = userId, Username = "MockUser", Email = "mock@user.com" });
        public Task<Basket> GetUserBasketAsync(int userId) => Task.FromResult(new Basket { Id = 1 });
        public Task<List<BasketItem>> GetBasketItemsAsync(int basketId) => Task.FromResult(new List<BasketItem>());
        public Task<List<Product>> GetShoppingCartItemsAsync(int userId) => Task.FromResult(new List<Product>());
        public Task<BuyProduct> GetBuyProductAsync(int productId) => Task.FromResult(new BuyProduct());
        public Task<List<Review>> GetReviewsGivenByUserAsync(int userId) => Task.FromResult(new List<Review>());
        public Task<List<Review>> GetReviewsReceivedByUserAsync(int userId) => Task.FromResult(new List<Review>());
        public Task<User> GetUserByIdAsync(int userId) => Task.FromResult(new User { Id = userId, Username = $"User{userId}" });
        public Task<List<Order>> GetBuyerOrdersAsync(int userId) => Task.FromResult(new List<Order>());
        public Task<List<Order>> GetSellerOrdersAsync(int userId) => Task.FromResult(new List<Order>());
        public Task<List<TrackedOrder>> GetTrackedOrdersAsync(int userId) => Task.FromResult(new List<TrackedOrder>());
        public Task<List<UserWaitList>> GetUserWaitlistsAsync(int userId) => Task.FromResult(new List<UserWaitList>());
        public Task<List<AuctionProduct>> GetUserAuctionProductsAsync(int userId) => Task.FromResult(new List<AuctionProduct>());
        public Task<List<Bid>> GetUserBidsAsync(int userId) => Task.FromResult(new List<Bid>());
    }
}
