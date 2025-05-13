//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MarketMinds.Shared.Models;
//using MarketMinds.Repositories.BorrowProductsRepository;
//using MarketMinds.Repositories;

//namespace MarketMinds.Test.Services.BorrowProductsServiceTest
//{
//    public class BorrowProductsRepositoryMock : IBorrowProductsRepository
//    {
//        private List<Product> products;
//        private int createListingCount;
//        private int deleteListingCount;
//        private int getProductsCount;
//        private int getProductByIdCount;
//        private int updateProductCount;

//        public BorrowProductsRepositoryMock()
//        {
//            products = new List<Product>();
//            createListingCount = 0;
//            deleteListingCount = 0;
//            getProductsCount = 0;
//            getProductByIdCount = 0;
//            updateProductCount = 0;
//        }

//        public void AddProduct(Product product)
//        {
//            if (!(product is BorrowProduct))
//            {
//                throw new InvalidCastException("Product must be of type BorrowProduct");
//            }

//            products.Add(product);
//            createListingCount++;
//        }

//        public void DeleteProduct(Product product)
//        {
//            products.RemoveAll(p => p.Id == product.Id);
//            deleteListingCount++;
//        }

//        public List<Product> GetProducts()
//        {
//            getProductsCount++;
//            return products;
//        }

//        public Product GetProductByID(int productId)
//        {
//            getProductByIdCount++;
//            return products.FirstOrDefault(p => p.Id == productId);
//        }

//        public void UpdateProduct(Product product)
//        {
//            if (!(product is BorrowProduct))
//            {
//                throw new InvalidCastException("Product must be of type BorrowProduct");
//            }

//            var existingProduct = products.FirstOrDefault(p => p.Id == product.Id);
//            if (existingProduct != null)
//            {
//                products.Remove(existingProduct);
//                products.Add(product);
//            }
//            updateProductCount++;
//        }

//        public int GetCreateListingCount()
//        {
//            return createListingCount;
//        }

//        public int GetDeleteListingCount()
//        {
//            return deleteListingCount;
//        }

//        public int GetGetProductsCount()
//        {
//            return getProductsCount;
//        }

//        public int GetGetProductByIdCount()
//        {
//            return getProductByIdCount;
//        }

//        public int GetUpdateProductCount()
//        {
//            return updateProductCount;
//        }
//    }
//}
