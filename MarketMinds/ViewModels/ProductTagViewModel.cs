using System.Collections.Generic;
using System.Collections.ObjectModel;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ProductTagService;

namespace MarketMinds.ViewModels
{
    public class ProductTagViewModel
    {
        private IProductTagService productTagService;

        public ObservableCollection<ProductTag> Tags { get; set; } = new ObservableCollection<ProductTag>();

        public ProductTagViewModel(IProductTagService productTagService)
        {
            this.productTagService = productTagService;
        }

        public List<ProductTag> GetAllProductTags()
        {
            return productTagService.GetAllProductTags();
        }

        public ProductTag CreateProductTag(string displayTitle)
        {
            var tag = productTagService.CreateProductTag(displayTitle);
            if (tag != null)
            {
                Tags.Add(tag);
            }
            return tag;
        }

        public void DeleteProductTag(string displayTitle)
        {
            productTagService.DeleteProductTag(displayTitle);
            // Optionally remove from Tags collection if needed
        }
    }
}
