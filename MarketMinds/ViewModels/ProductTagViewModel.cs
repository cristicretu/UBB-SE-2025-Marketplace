﻿using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ProductTagService;

namespace MarketMinds.ViewModels
{
    public class ProductTagViewModel
    {
        private IProductTagService productTagService;

        public ProductTagViewModel(ProductTagService productTagService)
        {
            this.productTagService = productTagService;
        }
        public List<ProductTag> GetAllProductTags()
        {
            return productTagService.GetAllProductTags();
        }

        public ProductTag CreateProductTag(string displayTitle)
        {
            return productTagService.CreateProductTag(displayTitle);
        }

        public void DeleteProductTag(string displayTitle)
        {
            productTagService.DeleteProductTag(displayTitle);
        }
    }
}
