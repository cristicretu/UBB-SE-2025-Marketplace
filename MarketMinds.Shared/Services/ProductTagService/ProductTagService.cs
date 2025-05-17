using System.Text.Json;
using MarketMinds.Shared.Models;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.ProxyRepository;

namespace MarketMinds.Shared.Services.ProductTagService
{
    public class ProductTagService : IProductTagService
    {
        private readonly ProductTagProxyRepository repository;
        private readonly JsonSerializerOptions jsonOptions;

        public ProductTagService(IConfiguration configuration)
        {
            repository = new ProductTagProxyRepository(configuration);
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public virtual List<ProductTag> GetAllProductTags()
        {
            try
            {
                var responseJson = repository.GetAllProductTagsRaw();
                var tags = JsonSerializer.Deserialize<List<ProductTag>>(responseJson, jsonOptions) ?? new List<ProductTag>();
                return tags.Select(ConvertToDomainTag).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all product tags: {ex.Message}");
                return new List<ProductTag>();
            }
        }

        public virtual ProductTag CreateProductTag(string displayTitle)
        {
            if (string.IsNullOrWhiteSpace(displayTitle))
            {
                throw new ArgumentException("Product tag display title cannot be null or empty.", nameof(displayTitle));
            }

            if (displayTitle.Length > 100)
            {
                throw new ArgumentException("Product tag display title cannot exceed 100 characters.", nameof(displayTitle));
            }

            try
            {
                var json = repository.CreateProductTagRaw(displayTitle);
                var tag = JsonSerializer.Deserialize<ProductTag>(json, jsonOptions);

                if (tag == null)
                {
                    throw new InvalidOperationException("Failed to create product tag.");
                }

                return ConvertToDomainTag(tag);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product tag: {ex.Message}");
                throw;
            }
        }

        public virtual void DeleteProductTag(string displayTitle)
        {
            if (string.IsNullOrWhiteSpace(displayTitle))
            {
                throw new ArgumentException("Product tag display title cannot be null or empty.", nameof(displayTitle));
            }

            try
            {
                repository.DeleteProductTag(displayTitle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product tag: {ex.Message}");
                throw;
            }
        }

        // Helper methods to convert between domain and shared models
        private ProductTag ConvertToDomainTag(MarketMinds.Shared.Models.ProductTag sharedTag)
        {
            if (sharedTag == null)
            {
                return null;
            }
            return new ProductTag(sharedTag.Id, sharedTag.Title);
        }
    }
}
