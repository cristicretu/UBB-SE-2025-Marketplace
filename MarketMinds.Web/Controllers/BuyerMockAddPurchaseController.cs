using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Service;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class BuyerMockAddPurchaseController : Controller
    {
        private readonly IBuyerService _buyerService;

        public BuyerMockAddPurchaseController(IBuyerService buyerService)
        {
            _buyerService = buyerService;
        }

        public IActionResult Index()
        {
            var viewModel = new BuyerMockAddPurchaseViewModel
            {
                ProductId = 0, // Default values
                Quantity = 1
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddPurchase(BuyerMockAddPurchaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Logic to add purchase (mocked for now)
            TempData["SuccessMessage"] = "Purchase added successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
