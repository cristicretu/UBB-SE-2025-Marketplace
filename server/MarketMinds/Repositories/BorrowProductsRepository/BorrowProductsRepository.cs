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

        public List<BorrowProduct> GetProducts(int offset, int count)
        {
            var query = context.BorrowProducts
                .Include(product => product.Condition)
                .Include(product => product.Category)
                .OrderBy(p => p.Id); // Ensure consistent ordering for pagination

            List<BorrowProduct> products;

            if (count > 0)
            {
                // Apply pagination
                products = query.Skip(offset).Take(count).ToList();
            }
            else
            {
                // Return all products if count is 0
                products = query.ToList();
            }

            foreach (var product in products)
            {
                LoadProductRelationships(product);
            }

            return products;
        }

        public int GetProductCount()
        {
            return context.BorrowProducts.Count();
        }

        public void DeleteProduct(BorrowProduct product)
        {
            context.BorrowProducts.Remove(product);
            context.SaveChanges();
        }

        public void AddProduct(BorrowProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            // Store tags for processing after product creation
            var tagsToProcess = new List<ProductTag>();
            if (product.Tags != null && product.Tags.Any())
            {
                tagsToProcess = product.Tags.ToList();
            }

            if (product.ProductTags != null && product.ProductTags.Any())
            {
                foreach (var productTag in product.ProductTags)
                {
                    if (productTag.TagId > 0)
                    {
                        var existingTag = context.ProductTags.Find(productTag.TagId);
                        if (existingTag != null)
                        {
                            productTag.Tag = existingTag;
                        }
                    }
                }
            }

            context.BorrowProducts.Add(product);
            context.SaveChanges();

            // Process tags after product is saved and has an ID
            if (tagsToProcess.Any())
            {
                foreach (var tag in tagsToProcess)
                {
                    var borrowProductTag = new BorrowProductProductTag
                    {
                        ProductId = product.Id,
                        TagId = tag.Id
                    };
                    context.BorrowProductProductTags.Add(borrowProductTag);
                }
                context.SaveChanges();

            }
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

            if (product.SellerId != null)
            {
                var seller = context.Users.FirstOrDefault(u => u.Id == product.SellerId);
                if (seller != null)
                {
                    product.Seller = seller;
                }
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

            // Load tags into the Tags property from ProductTags
            var tags = context.BorrowProductProductTags
                .Where(bppt => bppt.ProductId == product.Id)
                .Include(bppt => bppt.Tag)
                .Select(bppt => bppt.Tag)
                .ToList();

            product.Tags = tags;
        }
    }
}