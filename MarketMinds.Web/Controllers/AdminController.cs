using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketMinds.Shared.Services.ProductCategoryService;
using MarketMinds.Shared.Services.ProductConditionService;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IProductCategoryService _categoryService;
        private readonly IProductConditionService _conditionService;

        public AdminController(IProductCategoryService categoryService, IProductConditionService conditionService)
        {
            _categoryService = categoryService;
            _conditionService = conditionService;
        }

        private IActionResult HandleAddOperation(string name, string description, string emptyNameError, string successMessage, System.Action action)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = emptyNameError;
                return RedirectToAction("Index");
            }

            try
            {
                action();
                TempData["Success"] = successMessage;
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var categories = _categoryService.GetAllProductCategories();
            var conditions = _conditionService.GetAllProductConditions();
            
            ViewBag.Categories = categories;
            ViewBag.Conditions = conditions;
            
            return View();
        }

        [HttpPost]
        public IActionResult AddCategory(string name, string description)
        {
            return HandleAddOperation(
                name,
                description,
                "Category name cannot be empty",
                "Category added successfully",
                () => _categoryService.CreateProductCategory(name, description)
            );
        }

        [HttpPost]
        public IActionResult AddCondition(string name, string description)
        {
            return HandleAddOperation(
                name,
                description,
                "Condition name cannot be empty",
                "Condition added successfully",
                () => _conditionService.CreateProductCondition(name, description)
            );
        }

        [HttpPost]
        public IActionResult DeleteCategory(string name)
        {
            try
            {
                _categoryService.DeleteProductCategory(name);
                TempData["Success"] = "Category deleted successfully";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error deleting category: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteCondition(string name)
        {
            try
            {
                _conditionService.DeleteProductCondition(name);
                TempData["Success"] = "Condition deleted successfully";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error deleting condition: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
} 