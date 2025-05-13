using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.ProxyRepository;

namespace MarketMinds.Shared.Services.ProductCategoryService
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoryRepository productCategoryRepository;

        public ProductCategoryService(IProductCategoryRepository repository)
        {
            productCategoryRepository = repository as ProductCategoryProxyRepository
                ?? throw new ArgumentException("Repository must be of type ProductCategoryProxyRepository");
        }

        public List<Category> GetAllProductCategories()
        {
            try
            {
                var responseJson = productCategoryRepository.GetAllProductCategoriesRaw();
                var categories = new List<Category>();
                var responseJsonArray = JsonNode.Parse(responseJson)?.AsArray();

                if (responseJsonArray != null)
                {
                    foreach (var responseJsonItem in responseJsonArray)
                    {
                        var id = responseJsonItem["id"]?.GetValue<int>() ?? 0;
                        var name = responseJsonItem["name"]?.GetValue<string>() ?? string.Empty;
                        var description = responseJsonItem["description"]?.GetValue<string>() ?? string.Empty;
                        categories.Add(new Category(id, name, description));
                    }
                }

                if (categories == null)
                {
                    throw new InvalidOperationException("Failed to retrieve product categories.");
                }
                return categories;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting product categories: {ex.Message}", ex);
            }
        }

        public Category CreateProductCategory(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
            }

            if (name.Length > 100)
            {
                throw new ArgumentException("Category name cannot exceed 100 characters.", nameof(name));
            }

            if (description != null && description.Length > 500)
            {
                throw new ArgumentException("Category description cannot exceed 500 characters.", nameof(description));
            }

            try
            {
                var json = productCategoryRepository.CreateProductCategoryRaw(name, description);
                var jsonObject = JsonNode.Parse(json);

                if (jsonObject == null)
                {
                    throw new InvalidOperationException("Failed to create product category.");
                }

                var id = jsonObject["id"]?.GetValue<int>() ?? 0;
                var categoryName = jsonObject["name"]?.GetValue<string>() ?? string.Empty;
                var categoryDescription = jsonObject["description"]?.GetValue<string>() ?? string.Empty;
                var category = new Category(id, categoryName, categoryDescription);

                if (category == null)
                {
                    throw new InvalidOperationException("Failed to create product category.");
                }

                return category;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating product category: {ex.Message}", ex);
            }
        }

        public void DeleteProductCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
            }

            try
            {
                productCategoryRepository.DeleteProductCategoryRaw(name);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting product category: {ex.Message}", ex);
            }
        }
    }

    public class ProductCategoryRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string? Description { get; set; }
    }
}