using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using BusinessLogicLayer.ViewModel;
using DataAccessLayer;
using ViewModelLayer.ViewModel;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Services.AuctionProductsService;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.BasketService;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Shared.Services.ProductCategoryService;
using MarketMinds.Shared.Services.ProductConditionService;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.Services.ProductTagService;
using MarketMinds.ViewModels;
using MarketMinds.Shared.Services.DreamTeam.ChatbotService;
using MarketMinds.Shared.Services.ImagineUploadService;
using MarketMinds.Shared.Services.UserService;
using Marketplace_SE.Services.DreamTeam;
using MarketMinds.Shared.Services.ConversationService;
using MarketMinds.Shared.Services.MessageService;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Helper;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Views;
using MarketMinds.ViewModels.Admin;
using static MarketMinds.ViewModels.ContractRenewViewModel;

namespace MarketMinds
{
    public partial class App : Application
    {
        public static IConfiguration Configuration;
        public static DataBaseConnection DatabaseConnection;
        // Repository declarations
        public static IProductCategoryRepository ProductCategoryRepository;
        public static IMessageRepository MessageRepository;
        public static IProductConditionRepository ProductConditionRepository;
        public static IConversationRepository ConversationRepository;
        public static IChatRepository ChatRepository;
        public static IChatbotRepository ChatbotRepository;
        public static BuyerProxyRepository BuyerRepository;
        public static UserProxyRepository UserRepository;
        public static ReviewProxyRepository ReviewRepository;
        public static ProductTagProxyRepository ProductTagRepository;
        public static AuctionProductsProxyRepository AuctionProductsRepository;
        public static BorrowProductsProxyRepository BorrowProductsRepository;
        public static BasketProxyRepository BasketRepository;
        public static BuyProductsProxyRepository BuyProductsRepository;

        // Service declarations
        public static IBuyerService BuyerService;
        public static AdminService AdminService;
        public static AnalyticsService AnalyticsService;
        public static BuyProductsService BuyProductsService;
        public static BorrowProductsService BorrowProductsService;
        public static AuctionProductsService AuctionProductsService;
        public static ProductCategoryService CategoryService;
        public static ProductTagService TagService;
        public static ProductConditionService ConditionService;
        public static ReviewsService ReviewsService;
        public static BasketService BasketService;
        public static MarketMinds.Shared.Services.DreamTeam.ChatbotService.ChatbotService ChatBotService;
        public static MarketMinds.Shared.Services.DreamTeam.ChatService.ChatService ChatService;
        public static IImageUploadService ImageUploadService;
        public static IUserService UserService;
        public static AccountPageService AccountPageService { get; private set; }
        public static IConversationService ConversationService;
        public static IMessageService MessageService;
        public static MarketMinds.Shared.Services.DreamTeam.ChatbotService.IChatbotService NewChatbotService;
        public static IContractService ContractService;
        public static IPDFService PDFService;
        public static IContractRenewalService ContractRenewalService;
        public static IFileSystem FileSystem;

        // ViewModel declarations
        public static BuyerProfileViewModel BuyerProfileViewModel { get; private set; }
        public static AdminViewModel AdminViewModel { get; private set; }
        public static BuyProductsViewModel BuyProductsViewModel { get; private set; }
        public static BorrowProductsViewModel BorrowProductsViewModel { get; private set; }
        public static AuctionProductsViewModel AuctionProductsViewModel { get; private set; }
        public static ProductCategoryViewModel ProductCategoryViewModel { get; private set; }
        public static ProductConditionViewModel ProductConditionViewModel { get; private set; }
        public static ProductTagViewModel ProductTagViewModel { get; private set; }
        public static SortAndFilterViewModel<AuctionProductsService> AuctionProductSortAndFilterViewModel { get; private set; }
        public static SortAndFilterViewModel<BorrowProductsService> BorrowProductSortAndFilterViewModel { get; private set; }
        public static SortAndFilterViewModel<BuyProductsService> BuyProductSortAndFilterViewModel { get; private set; }
        public static ReviewCreateViewModel ReviewCreateViewModel { get; private set; }
        public static SeeBuyerReviewsViewModel SeeBuyerReviewsViewModel { get; private set; }
        public static SeeSellerReviewsViewModel SeeSellerReviewsViewModel { get; private set; }
        public static BasketViewModel BasketViewModel { get; private set; }
        public static CompareProductsViewModel CompareProductsViewModel { get; private set; }
        public static ChatBotViewModel ChatBotViewModel { get; private set; }
        public static ChatViewModel ChatViewModel { get; private set; }
        public static MainMarketplaceViewModel MainMarketplaceViewModel { get; private set; }
        public static LoginViewModel LoginViewModel { get; private set; }
        public static RegisterViewModel RegisterViewModel { get; private set; }
        public static MarketMinds.Shared.Models.User CurrentUser { get; set; } // this acts like the session user (desktop session)
        public static ContractRenewViewModel ContractRenewViewModel { get; private set; }

        private const int ADMIN = 1;
        private const int BUYER = 2;
        private const int SELLER = 3;

        private static IConfiguration appConfiguration;
        public static Window LoginWindow = null!;
        public static Window MainWindow = null!;
        public static Window BuyerProfileWindow = null!;
        private static HttpClient httpClient;
        // public static void ShowSellerProfile()
        // {
        //     Debug.WriteLine("ShowSellerProfile called");

        //     try
        //     {
        //         // Create a new window
        //         var sellerProfileWindow = new Window();

        //         // First initialize the SellerService
        //         // Change this line in ShowSellerProfile()
        //         var sellerRepository = new SellerProxyRepository(Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/api/");

        //         var sellerService = new SellerService(sellerRepository);

        //         // Initialize the ViewModel with the service and current user
        //         if (CurrentUser == null)
        //         {
        //             Debug.WriteLine("ERROR: CurrentUser is null in ShowSellerProfile");
        //             throw new InvalidOperationException("Cannot show seller profile: Current user is null");
        //         }

        //         // Create a frame to handle the navigation
        //         var frame = new Microsoft.UI.Xaml.Controls.Frame();
        //         sellerProfileWindow.Content = frame;

        //         // Create and configure the view model
        //         var viewModel = new SellerProfileViewModel(sellerService, CurrentUser);

        //         // Navigate to the page with the ViewModel as parameter
        //         frame.Navigate(typeof(MarketMinds.Views.SellerProfileView), viewModel);

        //         // Now show the window
        //         sellerProfileWindow.Activate();
        //         Debug.WriteLine("Seller profile window activated with ViewModel");
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.WriteLine($"ERROR showing seller profile: {ex.Message}");
        //         Debug.WriteLine(ex.StackTrace);
        //         ShowErrorDialog($"Could not open seller profile: {ex.Message}");
        //     }
        // }

        // public static void ShowBuyerProfile()
        // {
        //     BuyerProfileWindow = new Window();
        //     BuyerProfileViewModel.User = CurrentUser;
        //     _ = BuyerProfileViewModel.LoadBuyerProfile();
        //     var buyerProfileView = new MarketMinds.Views.BuyerProfileView();
        //     buyerProfileView.ViewModel = BuyerProfileViewModel;
        //     BuyerProfileWindow.Content = buyerProfileView;
        //     BuyerProfileWindow.Activate();
        // }

        public static void ShowAdminProfile()
        {
            var adminWindow = new AdminView();
            adminWindow.Activate();
        }

        public static void ShowHomePage()
        {
            var homePage = new HomePageView();
            homePage.Activate();
        }

        // Implementation of IOnLoginSuccessCallback
        private class LoginSuccessHandler : IOnLoginSuccessCallback
        {
            public async Task OnLoginSuccess(User user)
            {
                // Set the current user
                CurrentUser = user;
                // NEED THIS:
                UserSession.CurrentUserId = CurrentUser.Id;

                // Redirect based on user role
                switch (user.UserType)
                {
                    case 2: // Buyer
                    case 3: // Seller
                        ShowHomePage(); // the buyers and sellers will now see this main page
                        break;
                    case 1: // Admin
                        ShowAdminProfile();
                        break;
                    default:
                        Debug.WriteLine("ERROR: Invalid user type in LoginSuccessHandler, this should never happen");
                        break;
                }

                // Optionally close the login window
                if (LoginWindow != null)
                {
                    LoginWindow.Close();
                }
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                Debug.WriteLine("App constructor start");
                this.InitializeComponent();

                // Set up global exception handling
                this.UnhandledException += App_UnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                // Initialize configuration
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                appConfiguration = builder.Build();
                // Initialize API configuration
                InitializeConfiguration();

                // Initialize HttpClient
                httpClient = new HttpClient();
                Debug.WriteLine(AppContext.BaseDirectory);
                string baseAddress = Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/api/";
                httpClient.BaseAddress = new Uri(baseAddress);
                Debug.WriteLine($"Initialized HTTP client with base address: {baseAddress}");

                Debug.WriteLine("App constructor complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CRITICAL ERROR in App constructor: {ex}");
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true; // Mark as handled to prevent app termination

            // Log the exception
            Debug.WriteLine($"UNHANDLED UI EXCEPTION: {e.Message}\n{e.Exception}");

            // No need to break into the debugger here as we're handling it
            ShowErrorDialog($"An error occurred: {e.Message}. See debug output for details.");
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            // Log the exception
            Exception ex = e.ExceptionObject as Exception;
            Debug.WriteLine($"CRITICAL UNHANDLED EXCEPTION: {ex?.Message}\n{ex}");

            // Can't show UI from this thread, just log it
            if (e.IsTerminating)
            {
                Debug.WriteLine("APPLICATION IS TERMINATING DUE TO UNHANDLED EXCEPTION");
            }
        }

        // Helper method to show error dialogs from static methods
        private static void ShowErrorDialog(string message)
        {
            Debug.WriteLine($"Error dialog: {message}");

            // Try to use MainWindow if it exists
            if (MainWindow?.Content?.XamlRoot != null)
            {
                var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Error",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = MainWindow.Content.XamlRoot
                };

                _ = dialog.ShowAsync();
            }
        }

        private IConfiguration InitializeConfiguration()
        {
            Configuration = appConfiguration;
            return Configuration;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Create but don't show the main window yet
            MainWindow = new MarketMinds.MainWindow();
            BuyerRepository = new BuyerProxyRepository(Configuration);
            ProductCategoryRepository = new ProductCategoryProxyRepository(Configuration);
            MessageRepository = new MessageProxyRepository(Configuration);
            ProductConditionRepository = new ProductConditionProxyRepository(Configuration);
            ConversationRepository = new ConversationProxyRepository(Configuration);
            ChatbotRepository = new ChatbotProxyRepository(Configuration);
            ChatRepository = new ChatProxyRepository(Configuration);
            UserRepository = new UserProxyRepository(Configuration);
            BuyerRepository = new BuyerProxyRepository(Configuration);
            ReviewRepository = new ReviewProxyRepository(Configuration);
            ProductTagRepository = new ProductTagProxyRepository(Configuration);
            AuctionProductsRepository = new AuctionProductsProxyRepository(Configuration);
            BorrowProductsRepository = new BorrowProductsProxyRepository(Configuration);
            BasketRepository = new BasketProxyRepository(Configuration);
            BuyProductsRepository = new BuyProductsProxyRepository(Configuration);

            // Initialize services
            AdminService = new AdminService(UserRepository);
            AnalyticsService = new AnalyticsService(UserRepository, BuyerRepository);

            UserService = new UserService(Configuration);
            BuyerService = new BuyerService(BuyerRepository, UserRepository);
            ReviewsService = new ReviewsService(Configuration, UserService, CurrentUser);
            BuyProductsService = new BuyProductsService(BuyProductsRepository);
            BorrowProductsService = new BorrowProductsService();
            AuctionProductsService = new AuctionProductsService(AuctionProductsRepository);
            CategoryService = new ProductCategoryService(ProductCategoryRepository);
            TagService = new ProductTagService(Configuration);
            ConditionService = new ProductConditionService(ProductConditionRepository);
            BasketService = new BasketService(BasketRepository);
            AccountPageService = new AccountPageService(Configuration);
            ConversationService = new ConversationService(ConversationRepository);
            MessageService = new MessageService(MessageRepository);
            ChatBotService = new ChatbotService(ChatbotRepository);
            ChatService = new MarketMinds.Shared.Services.DreamTeam.ChatService.ChatService(ChatRepository);
            NewChatbotService = new MarketMinds.Shared.Services.DreamTeam.ChatbotService.ChatbotService(ChatbotRepository);
            ContractService = new ContractService();
            PDFService = new PDFService();
            ContractRenewalService = new ContractRenewalService();
            FileSystem = new FileSystemWrapper();

            // Initialize non-user dependent view models
            BuyProductsViewModel = new BuyProductsViewModel(BuyProductsService);
            AuctionProductsViewModel = new AuctionProductsViewModel(AuctionProductsService);
            ProductCategoryViewModel = new ProductCategoryViewModel(CategoryService);
            ProductTagViewModel = new ProductTagViewModel(TagService);
            ProductConditionViewModel = new ProductConditionViewModel(ConditionService);
            BorrowProductsViewModel = new BorrowProductsViewModel(BorrowProductsService);
            AuctionProductSortAndFilterViewModel = new SortAndFilterViewModel<AuctionProductsService>(AuctionProductsService);
            BorrowProductSortAndFilterViewModel = new SortAndFilterViewModel<BorrowProductsService>(BorrowProductsService);
            BuyProductSortAndFilterViewModel = new SortAndFilterViewModel<BuyProductsService>(BuyProductsService);
            CompareProductsViewModel = new CompareProductsViewModel();
            ChatBotViewModel = new ChatBotViewModel(ChatBotService);
            ChatViewModel = new ChatViewModel(ChatService);
            MainMarketplaceViewModel = new MainMarketplaceViewModel();
            ContractRenewViewModel = new ContractRenewViewModel(ContractService, PDFService, ContractRenewalService, UserService, FileSystem);
            BuyerProfileViewModel = new BuyerProfileViewModel()
            {
                BuyerService = BuyerService,
                User = CurrentUser,
            };
            // Initialize login and register view models with proper callbacks
            AdminViewModel = new AdminViewModel(AdminService, AnalyticsService, UserService);
            LoginViewModel = new LoginViewModel(UserService, new LoginSuccessHandler(), new CaptchaService());
            RegisterViewModel = new RegisterViewModel(UserService);
            ReviewCreateViewModel = null;
            SeeSellerReviewsViewModel = null;
            SeeBuyerReviewsViewModel = null;
            BasketViewModel = null;
            // Show login window first instead of main window
            LoginWindow = new LoginWindow();
            LoginWindow.Activate();
        }

        // public static void ShowMainWindow()
        // {
        //     if (CurrentUser != null)
        //     {
        //         BasketViewModel = new BasketViewModel(CurrentUser, BasketService);
        //         ReviewCreateViewModel = new ReviewCreateViewModel(ReviewsService, CurrentUser, CurrentUser);
        //         SeeBuyerReviewsViewModel = new SeeBuyerReviewsViewModel(ReviewsService, CurrentUser);
        //         SeeSellerReviewsViewModel = new SeeSellerReviewsViewModel(ReviewsService, CurrentUser, CurrentUser);
        //         NewChatbotService.SetCurrentUser(CurrentUser);
        //         ChatBotService.SetCurrentUser(CurrentUser);
        //         ChatBotViewModel.SetCurrentUser(CurrentUser);
        //         if (LoginWindow != null)
        //         {
        //             LoginWindow.Close();
        //         }
        //         MainWindow.Activate();
        //     }
        //     else
        //     {
        //         Debug.WriteLine("DEBUG: ERROR - Attempted to show main window with NULL CurrentUser");
        //     }
        // }

        public static void ResetLoginState()
        {
            LoginViewModel = new LoginViewModel(UserService, new LoginSuccessHandler(), new CaptchaService());
            CurrentUser = null;
        }
    }
}
