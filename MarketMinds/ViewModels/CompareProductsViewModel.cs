using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ProductComparisonService;

namespace ViewModelLayer.ViewModel
{
    public class CompareProductsViewModel
    {
        public Product LeftProduct;
        public Product RightProduct;
        private readonly IProductComparisonService comparisonService;

        public CompareProductsViewModel()
        {
            comparisonService = new ProductComparisonService();
        }

        public bool AddProductForCompare(Product product)
        {
            var result = comparisonService.AddProduct(LeftProduct, RightProduct, product);
            LeftProduct = result.LeftProduct;
            RightProduct = result.RightProduct;
            return result.IsComplete;
        }
    }
}