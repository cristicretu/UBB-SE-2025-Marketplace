//using MarketMinds.Shared.Models;
//using MarketMinds.Repositories.ProductCategoryRepository;
//using System.Collections.Generic;

//namespace MarketMinds.Test.Services.ProductCategoryService
//{
//    internal class ProductCategoryRepositoryMock : IProductCategoryRepository
//    {
//        public List<Category> Categories { get; set; } = new List<Category>();
//        private int currentIndex = 0;

//        public ProductCategoryRepositoryMock()
//        {
//            Categories = new List<Category>();
//        }

//        public List<Category> GetAllProductCategories()
//        {
//            return Categories;
//        }

//        public Category CreateProductCategory(string displayTitle, string description)
//        {
//            var newCategory = new Category(
//                id: ++currentIndex,
//                displayTitle: displayTitle,
//                description: description
//            );

//            Categories.Add(newCategory);
//            return newCategory;
//        }

//        public void DeleteProductCategory(string displayTitle)
//        {
//            Categories.RemoveAll(c => c.DisplayTitle == displayTitle);
//        }
//    }
//}