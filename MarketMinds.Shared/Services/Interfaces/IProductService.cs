using MarketMinds.Shared.Models;

/// <summary>
/// Interface for managing products in the service layer.
/// </summary>
public interface IProductService
{
    List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery);
}

