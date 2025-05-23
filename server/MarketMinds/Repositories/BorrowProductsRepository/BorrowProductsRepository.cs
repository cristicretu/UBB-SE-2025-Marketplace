using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Server.MarketMinds.Repositories.BorrowProductsRepository
{
    public class BorrowProductsRepository : IBorrowProductsRepository
    {
        private readonly ApplicationDbContext context;

        public BorrowProductsRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public List<BorrowProduct> GetProducts()
        {
            var products = context.BorrowProducts
                .Include(product => product.Condition)
                .Include(product => product.Category)
                .ToList();

            foreach (var product in products)
            {
                LoadProductRelationships(product);
            }

            return products;
        }

        public void DeleteProduct(BorrowProduct product)
        {
            context.BorrowProducts.Remove(product);
            context.SaveChanges();
        }

        public void AddProduct(BorrowProduct product)
        {
            context.BorrowProducts.Add(product);
            context.SaveChanges();
        }

        public void UpdateProduct(BorrowProduct product)
        {
            var existingProduct = context.BorrowProducts.Find(product.Id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"BorrowProduct with ID {product.Id} not found for update.");
            }

            context.Entry(existingProduct).CurrentValues.SetValues(product);

            if (product.Images != null && product.Images.Any())
            {
                foreach (var image in product.Images)
                {
                    if (image.Id == 0)
                    {
                        image.ProductId = product.Id;
                        context.Set<BorrowProductImage>().Add(image);
                    }
                }
            }

            context.SaveChanges();
        }

        public BorrowProduct GetProductByID(int id)
        {
            var product = context.BorrowProducts
                .Include(product => product.Condition)
                .Include(product => product.Category)
                .FirstOrDefault(product => product.Id == id);

            if (product == null)
            {
                throw new KeyNotFoundException($"BorrowProduct with ID {id} not found.");
            }

            LoadProductRelationships(product);

            return product;
        }

        public void AddImageToProduct(int productId, BorrowProductImage image)
        {
            image.ProductId = productId;
            context.BorrowProductImages.Add(image);
            context.SaveChanges();
        }

        private void LoadProductRelationships(BorrowProduct product)
        {
            context.Entry(product)
                .Collection(product => product.Images)
                .Load();

            context.Entry(product)
                .Collection(product => product.ProductTags)
                .Load();

            if (product.ProductTags != null && product.ProductTags.Any())
            {
                foreach (var productTag in product.ProductTags)
                {
                    context.Entry(productTag)
                        .Reference(pt => pt.Tag)
                        .Load();
                }
            }
        }
    }
}