using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Repositories.ProductConditionRepository
{
    public class ProductConditionRepository : IProductConditionRepository
    {
        private readonly ApplicationDbContext databaseContext;

        public ProductConditionRepository(ApplicationDbContext context)
        {
            databaseContext = context;
        }

        public List<Condition> GetAllProductConditions()
        {
            return databaseContext.ProductConditions.ToList();
        }

        public Condition CreateProductCondition(string displayTitle, string description)
        {
            var conditionToCreate = new Condition(displayTitle, description);
            databaseContext.ProductConditions.Add(conditionToCreate);
            databaseContext.SaveChanges();
            return conditionToCreate;
        }

        public void DeleteProductCondition(string displayTitle)
        {
            var conditionToDelete = databaseContext.ProductConditions.FirstOrDefault(condition => condition.Name == displayTitle);

            if (conditionToDelete == null)
            {
                throw new KeyNotFoundException($"Product condition with title '{displayTitle}' not found.");
            }

            databaseContext.ProductConditions.Remove(conditionToDelete);
            databaseContext.SaveChanges();
        }

        // Stub implementations for Raw methods (these won't be called server-side)
        public string GetAllProductConditionsRaw()
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public string CreateProductConditionRaw(string displayTitle, string description)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public void DeleteProductConditionRaw(string displayTitle)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }
    }
}
