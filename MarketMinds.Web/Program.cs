using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using MarketMinds.Shared;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.AuctionProductsService;
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
    });

// Register shared service clients
builder.Services.AddHttpClient("ApiClient", client =>
{
    // Get base URL from configuration
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    if (string.IsNullOrEmpty(baseUrl))
    {
        baseUrl = "http://localhost:5001";
    }
    // Make sure URL ends with slash
    if (!baseUrl.EndsWith("/"))
    {
        baseUrl += "/";
    }
    client.BaseAddress = new Uri(baseUrl + "api/");
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

// Register services
builder.Services.AddTransient<IAuctionProductService, MarketMinds.Shared.Services.AuctionProductsService.AuctionProductsService>();
builder.Services.AddTransient<IBorrowProductsService, MarketMinds.Shared.Services.BorrowProductsService.BorrowProductsService>();
builder.Services.AddTransient<IBuyProductsService, MarketMinds.Shared.Services.BuyProductsService.BuyProductsService>();
builder.Services.AddTransient<IProductService, MarketMinds.Shared.Services.BuyProductsService.BuyProductsService>();
builder.Services.AddTransient<IProductCategoryService, ProductCategoryService>();
builder.Services.AddTransient<IProductConditionService, ProductConditionService>();
builder.Services.AddTransient<IProductTagService, ProductTagService>();
builder.Services.AddTransient<IImageUploadService, ImageUploadService>();
builder.Services.AddTransient<IBasketService, BasketService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<MarketMinds.Shared.Services.DreamTeam.ChatService.IChatService, MarketMinds.Shared.Services.DreamTeam.ChatService.ChatService>();
builder.Services.AddTransient<MarketMinds.Shared.Services.DreamTeam.ChatbotService.IChatbotService, MarketMinds.Shared.Services.DreamTeam.ChatbotService.ChatbotService>();
builder.Services.AddTransient<IConversationService, ConversationService>();
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddTransient<IReviewsService, ReviewsService>();

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
