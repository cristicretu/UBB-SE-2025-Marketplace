using Server.DataAccessLayer;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Repositories.ProductTagRepository
{
    public class ProductTagRepository : IProductTagRepository
    {
        private readonly ApplicationDbContext databaseContext;

        public ProductTagRepository(ApplicationDbContext context)
        {
            databaseContext = context;
        }

        public List<ProductTag> GetAllProductTags()
        {
            try
            {
                var allTags = databaseContext.ProductTags.ToList();
                return allTags;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in GetAllProductTags using EF: {exception.Message}");
                throw;
            }
        }

        public ProductTag CreateProductTag(string displayTitle)
        {
            try
            {
                var tagToCreate = new ProductTag(0, displayTitle);
                databaseContext.ProductTags.Add(tagToCreate);
                databaseContext.SaveChanges();
                return tagToCreate;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in CreateProductTag using EF: {exception.Message}");
                throw;
            }
        }

        public void DeleteProductTag(string displayTitle)
        {
            try
            {
                var tagToDelete = databaseContext.ProductTags.FirstOrDefault(tag => tag.Title == displayTitle);

                if (tagToDelete == null)
                {
                    throw new KeyNotFoundException($"Product tag with title '{displayTitle}' not found.");
                }

                databaseContext.ProductTags.Remove(tagToDelete);
                databaseContext.SaveChanges();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in DeleteProductTag using EF: {exception.Message}");
                throw;
            }
        }
    }
}