using Microsoft.AspNetCore.Authentication.Cookies;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Shared.Services.ProductCategoryService;
using MarketMinds.Shared.Services.ProductConditionService;
using MarketMinds.Shared.Services.ProductTagService;
using MarketMinds.Shared.Services.ImagineUploadService;
using MarketMinds.Shared.Services.BasketService;
using MarketMinds.Shared.Services.UserService;
using MarketMinds.Shared.Services.ConversationService;
using MarketMinds.Shared.Services.MessageService;
using MarketMinds.Shared.Services.ReviewService;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Repositories;
using MarketMinds.Server.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Register shared service clients
builder.Services.AddHttpClient("ApiClient", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    if (string.IsNullOrEmpty(baseUrl))
    {
        baseUrl = "http://localhost:5001";
    }
    if (!baseUrl.EndsWith("/"))
    {
        baseUrl += "/";
    }
    client.BaseAddress = new Uri(baseUrl + "api/");
});

// Register ImageUploadService with configuration
builder.Services.AddSingleton<IImageUploadService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var clientId = configuration["ImgurSettings:ClientId"];
    if (string.IsNullOrEmpty(clientId))
    {
        throw new InvalidOperationException("Imgur Client ID is not configured in appsettings.json");
    }
    return new ImageUploadService(configuration);
});

// Register repositories
builder.Services.AddSingleton<AuctionProductsProxyRepository>();
builder.Services.AddSingleton<BorrowProductsProxyRepository>();
builder.Services.AddSingleton<BuyProductsProxyRepository>();
builder.Services.AddSingleton<ProductCategoryProxyRepository>();
builder.Services.AddSingleton<ProductConditionProxyRepository>();
builder.Services.AddSingleton<ProductTagProxyRepository>();
builder.Services.AddSingleton<BasketProxyRepository>();
builder.Services.AddSingleton<UserProxyRepository>();
builder.Services.AddSingleton<ChatProxyRepository>();
builder.Services.AddSingleton<MarketMinds.Shared.ProxyRepository.ChatbotProxyRepository>();
builder.Services.AddSingleton<ConversationProxyRepository>();
builder.Services.AddSingleton<MessageProxyRepository>();
builder.Services.AddSingleton<ReviewProxyRepository>();
builder.Services.AddSingleton<BuyerProxyRepository>(sp =>
    new BuyerProxyRepository(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddSingleton<SellerProxyRepository>(sp =>
    new SellerProxyRepository(
        builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/"));

// BuyerLinkage proxy repository
builder.Services.AddSingleton<BuyerLinkageProxyRepository>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ApiClient");
    return new BuyerLinkageProxyRepository(httpClient);
});

// BuyerSellerFollow proxy repository
builder.Services.AddSingleton<BuyerSellerFollowProxyRepository>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ApiClient");
    return new BuyerSellerFollowProxyRepository(httpClient);
});

// Contract-related proxy repositories
builder.Services.AddSingleton<ContractProxyRepository>(sp =>
    new ContractProxyRepository(
        builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/"));
builder.Services.AddSingleton<PDFProxyRepository>(sp =>
    new PDFProxyRepository(
        builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/"));
builder.Services.AddSingleton<ContractRenewalProxyRepository>(sp =>
    new ContractRenewalProxyRepository(
        builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/"));
builder.Services.AddSingleton<NotificationProxyRepository>(sp =>
    new NotificationProxyRepository(
        builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/"));

// Merged: Register all unique proxy repositories from both branches
builder.Services.AddSingleton<OrderProxyRepository>(sp =>
    new OrderProxyRepository(
        builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/"));
builder.Services.AddSingleton<OrderSummaryProxyRepository>(sp =>
    new OrderSummaryProxyRepository(
        builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/"));
builder.Services.AddSingleton<TrackedOrderProxyRepository>(sp =>
    new TrackedOrderProxyRepository(
        builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/"));
builder.Services.AddSingleton<ShoppingCartProxyRepository>(sp =>
    new ShoppingCartProxyRepository(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddSingleton<DummyWalletProxyRepository>(sp =>
    new DummyWalletProxyRepository(
        sp.GetRequiredService<IConfiguration>()["ApiSettings:BaseUrl"] ?? "http://localhost:5001"));

// Register services
builder.Services.AddTransient<IAuctionProductService, MarketMinds.Shared.Services.AuctionProductsService.AuctionProductsService>();
builder.Services.AddTransient<IBorrowProductsService, MarketMinds.Shared.Services.BorrowProductsService.BorrowProductsService>();

// Use the same instance for both interfaces
builder.Services.AddTransient<MarketMinds.Shared.Services.BuyProductsService.BuyProductsService>();
builder.Services.AddTransient<IBuyProductsService>(sp => sp.GetRequiredService<MarketMinds.Shared.Services.BuyProductsService.BuyProductsService>());
builder.Services.AddTransient<IProductService>(sp => sp.GetRequiredService<MarketMinds.Shared.Services.BuyProductsService.BuyProductsService>());

builder.Services.AddTransient<IProductCategoryService, ProductCategoryService>();
builder.Services.AddTransient<IProductConditionService, ProductConditionService>();
builder.Services.AddTransient<IProductTagService, ProductTagService>();
builder.Services.AddTransient<IImageUploadService, ImageUploadService>();
builder.Services.AddTransient<IBasketService, BasketService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IChatService, ChatService>();
builder.Services.AddTransient<IChatbotService, ChatbotService>();
builder.Services.AddTransient<IConversationService, ConversationService>();
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddTransient<IReviewsService, ReviewsService>();
builder.Services.AddTransient<IBuyerService, BuyerService>();
builder.Services.AddTransient<ISellerService, SellerService>();

// Contract-related services
builder.Services.AddTransient<IContractService, ContractService>();
builder.Services.AddTransient<IPDFService, PDFService>();
builder.Services.AddTransient<IContractRenewalService, ContractRenewalService>();
builder.Services.AddTransient<INotificationContentService, NotificationContentService>();

// Merged: Register all unique services from both branches
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<ITrackedOrderService, TrackedOrderService>();
builder.Services.AddTransient<IOrderHistoryService, OrderHistoryService>();
builder.Services.AddTransient<IOrderSummaryService, OrderSummaryService>();
builder.Services.AddTransient<IShoppingCartService, ShoppingCartService>();
builder.Services.AddTransient<IDummyWalletService, DummyWalletService>();

// BuyerLinkage service
builder.Services.AddTransient<IBuyerLinkageService>(sp =>
{
    var repository = sp.GetRequiredService<BuyerLinkageProxyRepository>();
    var buyerService = sp.GetRequiredService<IBuyerService>();
    var logger = sp.GetRequiredService<ILogger<BuyerLinkageService>>();
    
    // Create the actual service implementation
    return new BuyerLinkageService(repository, buyerService, logger);
});

// BuyerSellerFollow service
builder.Services.AddTransient<IBuyerSellerFollowService>(sp =>
{
    var repository = sp.GetRequiredService<BuyerSellerFollowProxyRepository>();
    var buyerService = sp.GetRequiredService<IBuyerService>();
    var sellerService = sp.GetRequiredService<ISellerService>();
    var logger = sp.GetRequiredService<ILogger<BuyerSellerFollowService>>();
    
    // Create the actual service implementation
    return new BuyerSellerFollowService(repository, buyerService, sellerService, logger);
});

// Register IBuyerRepository and IBuyerService
builder.Services.AddTransient<IBuyerRepository, BuyerProxyRepository>();
builder.Services.AddTransient<IBuyerService, BuyerService>();

// Register repository interfaces
builder.Services.AddTransient<IProductCategoryRepository>(sp => sp.GetRequiredService<ProductCategoryProxyRepository>());
builder.Services.AddTransient<IProductConditionRepository>(sp => sp.GetRequiredService<ProductConditionProxyRepository>());
builder.Services.AddTransient<IProductTagRepository>(sp => sp.GetRequiredService<ProductTagProxyRepository>());
builder.Services.AddTransient<IBasketRepository>(sp => sp.GetRequiredService<BasketProxyRepository>());
builder.Services.AddTransient<IAccountRepository>(sp => sp.GetRequiredService<UserProxyRepository>());
builder.Services.AddTransient<IChatRepository>(sp => sp.GetRequiredService<ChatProxyRepository>());
builder.Services.AddTransient<MarketMinds.Shared.IRepository.IChatbotRepository>(sp => sp.GetRequiredService<MarketMinds.Shared.ProxyRepository.ChatbotProxyRepository>());
builder.Services.AddTransient<IConversationRepository>(sp => sp.GetRequiredService<ConversationProxyRepository>());
builder.Services.AddTransient<IMessageRepository>(sp => sp.GetRequiredService<MessageProxyRepository>());
builder.Services.AddTransient<IReviewRepository>(sp => sp.GetRequiredService<ReviewProxyRepository>());
builder.Services.AddTransient<IBuyerRepository>(sp => sp.GetRequiredService<BuyerProxyRepository>());
builder.Services.AddTransient<ISellerRepository>(sp => sp.GetRequiredService<SellerProxyRepository>());

// Contract-related repository interfaces
builder.Services.AddTransient<IContractRepository>(sp => sp.GetRequiredService<ContractProxyRepository>());
builder.Services.AddTransient<IPDFRepository>(sp => sp.GetRequiredService<PDFProxyRepository>());
builder.Services.AddTransient<IContractRenewalRepository>(sp => sp.GetRequiredService<ContractRenewalProxyRepository>());
builder.Services.AddTransient<INotificationRepository>(sp => sp.GetRequiredService<NotificationProxyRepository>());

// Merged: Register all unique repository interfaces from both branches
builder.Services.AddTransient<IOrderHistoryRepository>(sp => sp.GetRequiredService<OrderHistoryProxyRepository>());
builder.Services.AddTransient<IOrderRepository>(sp => sp.GetRequiredService<OrderProxyRepository>());
builder.Services.AddTransient<IOrderSummaryRepository>(sp => sp.GetRequiredService<OrderSummaryProxyRepository>());
builder.Services.AddTransient<ITrackedOrderRepository>(sp => sp.GetRequiredService<TrackedOrderProxyRepository>());
builder.Services.AddTransient<IShoppingCartRepository>(sp => sp.GetRequiredService<ShoppingCartProxyRepository>());
builder.Services.AddTransient<IDummyWalletRepository>(sp => sp.GetRequiredService<DummyWalletProxyRepository>());

// BuyerLinkage repository
builder.Services.AddTransient<IBuyerLinkageRepository>(sp => sp.GetRequiredService<BuyerLinkageProxyRepository>());

// BuyerSellerFollow repository
builder.Services.AddTransient<IBuyerSellerFollowRepository>(sp => sp.GetRequiredService<BuyerSellerFollowProxyRepository>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();