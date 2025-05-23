using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Services.BorrowProductsService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MarketMinds.Web.Controllers
{
    [Authorize]
    public class BorrowProductsController : Controller
    {
        private readonly ILogger<BorrowProductsController> _logger;
        private readonly IBorrowProductsService _borrowProductsService;

        public BorrowProductsController(
            ILogger<BorrowProductsController> logger,
            IBorrowProductsService borrowProductsService)
        {
            _logger = logger;
            _borrowProductsService = borrowProductsService;
        }

        // GET: BorrowProducts
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching all borrow products");
                var borrowProducts = await _borrowProductsService.GetAllBorrowProductsAsync();
                return View(borrowProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching borrow products");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching borrow products");
                return View(new List<BorrowProduct>());
            }
        }

        // GET: BorrowProducts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching borrow product with ID {id}");
                var borrowProduct = await _borrowProductsService.GetBorrowProductByIdAsync(id);
                
                if (borrowProduct == null || borrowProduct.Id == 0)
                {
                    _logger.LogWarning($"Borrow product with ID {id} not found");
                    return NotFound();
                }
                
                return View(borrowProduct);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"Borrow product with ID {id} not found");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching borrow product {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BorrowProducts/CalculatePrice
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CalculatePrice(int productId, DateTime endDate)
        {
            try
            {
                var borrowProduct = await _borrowProductsService.GetBorrowProductByIdAsync(productId);
                
                if (borrowProduct == null || borrowProduct.Id == 0)
                {
                    return Json(new { success = false, error = "Product not found" });
                }
                
                DateTime startDate = borrowProduct.StartDate ?? DateTime.Now;
                
                if (endDate < startDate)
                {
                    return Json(new { success = false, error = "End date cannot be before start date" });
                }
                
                if (endDate > borrowProduct.TimeLimit)
                {
                    return Json(new { success = false, error = "End date cannot exceed the time limit" });
                }
                
                int days = (int)Math.Ceiling((endDate - startDate).TotalDays);
                double totalPrice = days * borrowProduct.DailyRate;
                
                return Json(new { 
                    success = true, 
                    price = totalPrice,
                    formattedPrice = totalPrice.ToString("C"),
                    days = days
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating price for product {productId}");
                return Json(new { success = false, error = "An error occurred while calculating the price" });
            }
        }

        // GET: BorrowProducts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BorrowProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBorrowProductDTO productDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var validationErrors = await _borrowProductsService.ValidateProductDTOAsync(productDTO);
                    if (validationErrors.Count > 0)
                    {
                        foreach (var error in validationErrors)
                        {
                            foreach (var message in error.Value)
                            {
                                ModelState.AddModelError(error.Key, message);
                            }
                        }
                        return View(productDTO);
                    }

                    var product = await _borrowProductsService.CreateProductAsync(productDTO);
                    if (product != null)
                    {
                        return RedirectToAction(nameof(Details), new { id = product.Id });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create the borrow product");
                        return View(productDTO);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating borrow product");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the borrow product");
                }
            }
            return View(productDTO);
        }

        // POST: BorrowProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BorrowProduct borrowProduct)
        {
            if (id != borrowProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _borrowProductsService.UpdateBorrowProductAsync(borrowProduct);
                    if (success)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update the borrow product");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating borrow product {id}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the borrow product");
                }
            }
            return View(borrowProduct);
        }

        // GET: BorrowProducts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var borrowProduct = await _borrowProductsService.GetBorrowProductByIdAsync(id);
                if (borrowProduct == null || borrowProduct.Id == 0)
                {
                    return NotFound();
                }
                return View(borrowProduct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching borrow product {id} for delete");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BorrowProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _borrowProductsService.DeleteBorrowProductAsync(id);
                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to delete the borrow product");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting borrow product {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to calculate time left for borrowing
        [NonAction]
        public string GetTimeRemaining(DateTime? endDate)
        {
            if (!endDate.HasValue)
            {
                return "No end date specified";
            }
            
            var timeLeft = endDate.Value - DateTime.Now;
            
            if (timeLeft <= TimeSpan.Zero)
            {
                return "Borrowing period ended";
            }
            
            return $"{timeLeft.Days}d {timeLeft.Hours}h {timeLeft.Minutes}m";
        }
    }
} 