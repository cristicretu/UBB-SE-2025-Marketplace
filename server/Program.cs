using System.Text.Json.Serialization;
using DataAccessLayer;
using MarketMinds.Repositories.AuctionProductsRepository;
using MarketMinds.Repositories.BuyProductsRepository;
using MarketMinds.Repositories.BasketRepository;
using MarketMinds.Repositories.ReviewRepository;
using MarketMinds.Repositories.ProductCategoryRepository;
using MarketMinds.Repositories.ProductConditionRepository;
using MarketMinds.Repositories.ProductTagRepository;
using MarketMinds.Repositories.ConversationRepository;
using MarketMinds.Repositories.MessageRepository;
using MarketMinds.Repositories.ChatbotRepository;
using MarketMinds.Shared.IRepository;
using Microsoft.EntityFrameworkCore;
using Server.MarketMinds.Repositories.BorrowProductsRepository;
using Server.MarketMinds.Repositories.AccountRepository;
using Server.MarketMinds.Repositories.UserRepository;

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
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<MarketMinds.Shared.IRepository.IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<MarketMinds.Shared.IRepository.IProductConditionRepository, ProductConditionRepository>();
builder.Services.AddScoped<IProductTagRepository, ProductTagRepository>();
builder.Services.AddScoped<IBorrowProductsRepository, BorrowProductsRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IChatbotRepository, ChatbotRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

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