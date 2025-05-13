using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Repositories.ProductCategoryRepository
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly ApplicationDbContext databaseContext;

        public ProductCategoryRepository(ApplicationDbContext context)
        {
            databaseContext = context;
        }

        public List<Category> GetAllProductCategories()
        {
            return databaseContext.ProductCategories.ToList();
        }

        public Category CreateProductCategory(string displayTitle, string description)
        {
            var categoryToCreate = new Category(displayTitle, description);

            databaseContext.ProductCategories.Add(categoryToCreate);
            databaseContext.SaveChanges();

            return categoryToCreate;
        }

        public void DeleteProductCategory(string displayTitle)
        {
            var categoryToDelete = databaseContext.ProductCategories.FirstOrDefault(category => category.Name == displayTitle);

            if (categoryToDelete == null)
            {
                throw new KeyNotFoundException($"Category with title '{displayTitle}' not found.");
            }

            databaseContext.ProductCategories.Remove(categoryToDelete);
            databaseContext.SaveChanges();
        }

        // Stub implementations for Raw methods (these won't be called server-side)
        public string GetAllProductCategoriesRaw()
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public string CreateProductCategoryRaw(string displayTitle, string description)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public void DeleteProductCategoryRaw(string displayTitle)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }
    }
}