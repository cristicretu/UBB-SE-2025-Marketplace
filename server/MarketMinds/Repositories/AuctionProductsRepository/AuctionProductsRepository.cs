using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;


namespace MarketMinds.Repositories.AuctionProductsRepository
{
    public class AuctionProductsRepository : IAuctionProductsRepository
    {
        private readonly ApplicationDbContext context;

        public AuctionProductsRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public List<AuctionProduct> GetProducts()
        {
            try
            {
                var products = context.AuctionProducts
                    .Include(product => product.Condition)
                    .Include(product => product.Category)
                    .Include(product => product.Bids)
                    .Include(product => product.Images)
                    .ToList();

                return products;
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to retrieve auction products: {exception.Message}", exception);
            }
        }

        public void DeleteProduct(AuctionProduct product)
        {
            try
            {
                context.AuctionProducts.Remove(product);
                context.SaveChanges();
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to delete auction product: {exception.Message}", exception);
            }
        }

        public void AddProduct(AuctionProduct product)
        {
            try
            {
                context.AuctionProducts.Add(product);
                context.SaveChanges();
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to add auction product: {exception.Message}", exception);
            }
        }

        public void UpdateProduct(AuctionProduct product)
        {
            try
            {
                var existingProduct = context.AuctionProducts.Find(product.Id);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"AuctionProduct with ID {product.Id} not found for update.");
                }

                context.Entry(existingProduct).CurrentValues.SetValues(product);

                if (product.Images != null && product.Images.Any())
                {

                    foreach (var image in product.Images)
                    {
                        if (image.Id == 0)
                        {
                            image.ProductId = product.Id;
                            context.Set<ProductImage>().Add(image);
                        }
                    }
                }

                context.SaveChanges();
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to update auction product: {exception.Message}", exception);
            }
        }

        public AuctionProduct GetProductByID(int id)
        {
            try
            {
                var product = context.AuctionProducts
                    .Include(product => product.Condition)
                    .Include(product => product.Category)
                    .Include(product => product.Bids)
                    .Include(product => product.Images)
                    .FirstOrDefault(product => product.Id == id);

                if (product == null)
                {
                    throw new KeyNotFoundException($"AuctionProduct with ID {id} not found.");
                }

                return product;
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                throw new KeyNotFoundException($"AuctionProduct with ID {id} not found.", keyNotFoundException);
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to retrieve auction product by ID: {exception.Message}", exception);
            }
        }
    }
}