using System.Text.Json.Nodes;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;

namespace MarketMinds.Shared.Services.ProductConditionService
{
    public class ProductConditionService : IProductConditionService
    {
        private readonly IProductConditionRepository productConditionRepository;

        public ProductConditionService(IProductConditionRepository repository)
        {
            productConditionRepository = repository as ProductConditionProxyRepository
                ?? throw new ArgumentException("Repository must be of type ProductConditionProxyRepository");
        }

        public List<Condition> GetAllProductConditions()
        {
            try
            {
                var responseJson = productConditionRepository.GetAllProductConditionsRaw();
                var clientConditions = new List<Condition>();
                var responseJsonArray = JsonNode.Parse(responseJson)?.AsArray();

                if (responseJsonArray != null)
                {
                    foreach (var responseJsonItem in responseJsonArray)
                    {
                        var id = responseJsonItem["id"]?.GetValue<int>() ?? 0;
                        var name = responseJsonItem["name"]?.GetValue<string>() ?? string.Empty;
                        var description = responseJsonItem["description"]?.GetValue<string>() ?? string.Empty;
                        clientConditions.Add(new Condition(id, name, description));
                    }
                }
                return clientConditions;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving product conditions: {ex.Message}", ex);
            }
        }

        public Condition CreateProductCondition(string displayTitle, string description)
        {
            // Business logic validation moved from controller
            if (string.IsNullOrWhiteSpace(displayTitle))
            {
                throw new ArgumentException("Condition title cannot be null or empty.", nameof(displayTitle));
            }

            if (displayTitle.Length > 100)
            {
                throw new ArgumentException("Condition title cannot exceed 100 characters.", nameof(displayTitle));
            }

            if (description != null && description.Length > 500)
            {
                throw new ArgumentException("Condition description cannot exceed 500 characters.", nameof(description));
            }

            try
            {
                var json = productConditionRepository.CreateProductConditionRaw(displayTitle, description);
                var jsonObject = JsonNode.Parse(json);

                if (jsonObject == null)
                {
                    throw new InvalidOperationException("Failed to parse the server response.");
                }

                var id = jsonObject["id"]?.GetValue<int>() ?? 0;
                var name = jsonObject["name"]?.GetValue<string>() ?? string.Empty;
                var conditionDescription = jsonObject["description"]?.GetValue<string>() ?? string.Empty;
                return new Condition(id, name, conditionDescription);
            }
            catch (ArgumentException)
            {
                // Rethrow argument exceptions directly
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating product condition: {ex.Message}", ex);
            }
        }

        public void DeleteProductCondition(string displayTitle)
        {
            if (string.IsNullOrWhiteSpace(displayTitle))
            {
                throw new ArgumentException("Condition title cannot be null or empty.", nameof(displayTitle));
            }

            try
            {
                productConditionRepository.DeleteProductConditionRaw(displayTitle);
            }
            catch (KeyNotFoundException)
            {
                // We'll treat this as a success for idempotent deletes
                return;
            }
            catch (Exception ex)
            {
                // Check for foreign key constraint errors
                if (ex.ToString().Contains("REFERENCE constraint") ||
                    ex.ToString().Contains("FK_BorrowProducts_ProductConditions"))
                {
                    throw new InvalidOperationException($"Cannot delete condition '{displayTitle}' because it is being used by one or more products.", ex);
                }

                throw new InvalidOperationException($"Error deleting product condition: {ex.Message}", ex);
            }
        }
    }

    public class ProductConditionRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}