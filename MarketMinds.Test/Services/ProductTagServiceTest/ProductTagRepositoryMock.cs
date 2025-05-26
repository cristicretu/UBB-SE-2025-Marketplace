using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MarketMinds.Test.Services.ProductTagServiceTest
{
    public class ProductTagProxyRepositoryMock : ProductTagProxyRepository
    {
        private List<ProductTag> productTags;

        public ProductTagProxyRepositoryMock() : base(null)
        {
            productTags = new List<ProductTag>();
        }

        public void SetupProductTags(List<ProductTag> tags)
        {
            productTags = tags;
        }

        public string GetAllProductTagsRaw()
        {
            return JsonSerializer.Serialize(productTags);
        }

        public string CreateProductTagRaw(string displayTitle)
        {
            var newTag = new ProductTag
            {
                Id = productTags.Count > 0 ? productTags.Max(t => t.Id) + 1 : 1,
                Title = displayTitle
            };

            productTags.Add(newTag);
            return JsonSerializer.Serialize(newTag);
        }

        public void DeleteProductTag(string displayTitle)
        {
            var tagToDelete = productTags.FirstOrDefault(t => t.Title == displayTitle);
            if (tagToDelete != null)
            {
                productTags.Remove(tagToDelete);
            }
            else
            {
                throw new InvalidOperationException($"Tag with title '{displayTitle}' not found.");
            }
        }

        // Helper method to get the current state of tags (for assertions)
        public List<ProductTag> GetCurrentTags()
        {
            return productTags;
        }
    }
}
