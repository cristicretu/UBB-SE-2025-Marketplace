namespace MarketMinds.Shared.Services.ProductPaginationService
{
    public class ProductPaginationService : IProductPaginationService
    {
        private readonly int itemsPerPage;

        private const int BASE_PAGE = 1;

        public ProductPaginationService(int itemsPerPage = 20)
        {
            this.itemsPerPage = itemsPerPage;
        }

        public (List<T> CurrentPage, int TotalPages) GetPaginatedProducts<T>(
            List<T> allProducts,
            int currentPage)
        {
            if (allProducts == null)
            {
                throw new ArgumentNullException(nameof(allProducts));
            }

            if (currentPage < BASE_PAGE)
            {
                throw new ArgumentException("Current page must be greater than 0", nameof(currentPage));
            }

            int totalPages = (int)Math.Ceiling(allProducts.Count / (double)itemsPerPage);
            currentPage = Math.Min(currentPage, totalPages);

            int startIndex = (currentPage - 1) * itemsPerPage;
            int count = Math.Min(itemsPerPage, allProducts.Count - startIndex);

            var currentPageProducts = allProducts.Skip(startIndex).Take(count).ToList();

            return (currentPageProducts, totalPages);
        }

        public List<T> ApplyFilters<T>(
            List<T> products,
            Func<T, bool> filterPredicate)
        {
            if (products == null)
            {
                throw new ArgumentNullException(nameof(products));
            }

            if (filterPredicate == null)
            {
                throw new ArgumentNullException(nameof(filterPredicate));
            }

            return products.Where(filterPredicate).ToList();
        }
    }
}