//using MarketMinds.Shared.Models;
//using MarketMinds.Repositories;
//using System.Collections.Generic;
//using System.Linq;

//namespace MarketMinds.Test.Services.ProductTagService
//{
//    internal class ProductRepositoryMock : IProductsRepository
//    {
//        public List<Product> Products { get; private set; }

//        public ProductRepositoryMock()
//        {
//            Products = new List<Product>();
//        }

//        public List<Product> GetProducts()
//        {
//            return Products;
//        }

//        public Product GetProductByID(int id)
//        {
//            return Products.FirstOrDefault(p => p.Id == id);
//        }

//        public void AddProduct(Product product)
//        {
//            Products.Add(product);
//        }

//        public void UpdateProduct(Product product)
//        {
//            var existingProduct = Products.FirstOrDefault(p => p.Id == product.Id);
//            if (existingProduct != null)
//            {
//                Products.Remove(existingProduct);
//                Products.Add(product);
//            }
//        }

//        public void DeleteProduct(Product product)
//        {
//            var existingProduct = Products.FirstOrDefault(p => p.Id == product.Id);
//            if (existingProduct != null)
//            {
//                Products.Remove(existingProduct);
//            }
//        }
//    }
//}