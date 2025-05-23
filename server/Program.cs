using System.Text.Json.Serialization;
using DataAccessLayer;
using MarketMinds.Repositories.AuctionProductsRepository;
using MarketMinds.Repositories.BuyProductsRepository;
using MarketMinds.Repositories.ReviewRepository;
using MarketMinds.Repositories.ProductCategoryRepository;
using MarketMinds.Repositories.ProductConditionRepository;
using MarketMinds.Repositories.ProductTagRepository;
using MarketMinds.Repositories.ConversationRepository;
using MarketMinds.Repositories.MessageRepository;
using MarketMinds.Repositories.ChatbotRepository;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.ProxyRepository;
using Microsoft.EntityFrameworkCore;
using Server.MarketMinds.Repositories.BorrowProductsRepository;
using Server.MarketMinds.Repositories.UserRepository;
// Additional repository namespaces
using MarketMinds.Repositories;
using Server.Repository;
using MarketMinds.Shared.Repositories;
using MarketMinds.Shared.Services;
using Server.MarketMinds.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Do not use ReferenceHandler.Preserve as requested by the user
    // We'll use JsonIgnore attributes on navigation properties instead
    options.JsonSerializerOptions.ReferenceHandler = null;

    // Enable camel casing to match frontend expectations
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

    // Ignore null values in the output
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// compatibility with old API without EF, need to remove this when EF is fully implemented
builder.Services.AddSingleton<DataBaseConnection>();

// EntityFramework database connection setup
var initialCatalog = builder.Configuration["InitialCatalog"];
var localDataSource = builder.Configuration["LocalDataSource"];
var connectionString = $"Server={localDataSource};Database={initialCatalog};Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<Server.DataAccessLayer.ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register all repositories
builder.Services.AddScoped<IAuctionProductsRepository, AuctionProductsRepository>();
builder.Services.AddScoped<IBuyProductsRepository, BuyProductsRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<MarketMinds.Shared.IRepository.IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<MarketMinds.Shared.IRepository.IProductConditionRepository, ProductConditionRepository>();
builder.Services.AddScoped<IProductTagRepository, ProductTagRepository>();
builder.Services.AddScoped<IBorrowProductsRepository, BorrowProductsRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IChatbotRepository, ChatbotRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, UserProxyRepository>();

// Additional repository registrations
builder.Services.AddScoped<IWaitListRepository, WaitListRepository>();
builder.Services.AddScoped<ITrackedOrderRepository, TrackedOrderRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<ISellerRepository, SellerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPDFRepository, PDFRepository>();
builder.Services.AddScoped<IOrderSummaryRepository, OrderSummaryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IDummyWalletRepository, DummyWalletRepository>();
builder.Services.AddScoped<IDummyCardRepository, DummyCardRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IContractRenewalRepository, ContractRenewalRepository>();
builder.Services.AddScoped<IBuyerRepository, BuyerRepository>();
builder.Services.AddScoped<IBuyerLinkageRepository, BuyerLinkageRepository>();
// If IChatRepository has an implementation, register it
// builder.Services.AddScoped<IChatRepository, ChatRepository>();
// If IBasketRepository has an implementation, register it
// builder.Services.AddScoped<IBasketRepository, BasketRepository>();

// Register services
// Removed IBuyerLinkageService registration since we handle logic in controller

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();