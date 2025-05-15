using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IOrderHistoryService _orderHistoryService;
        private readonly IOrderSummaryService _orderSummaryService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IDummyWalletService _dummyWalletService;

        public CheckoutController(
            IOrderHistoryService orderHistoryService,
            IOrderSummaryService orderSummaryService,
            IOrderService orderService,
            IProductService productService,
            IDummyWalletService dummyWalletService)
        {
            _orderHistoryService = orderHistoryService;
            _orderSummaryService = orderSummaryService;
            _orderService = orderService;
            _productService = productService;
            _dummyWalletService = dummyWalletService;
        }

        public IActionResult BillingInfo(int orderHistoryId)
        {
            var model = new BillingInfoViewModel(orderHistoryId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BillingInfo(BillingInfoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.SelectedPaymentMethod == "card")
            {
                return RedirectToAction("CardInfo", new { orderHistoryId = model.OrderHistoryID });
            }
            else
            {
                if (model.SelectedPaymentMethod == "wallet")
                {
                    await ProcessWalletRefill(model);
                }
                return RedirectToAction("FinalizePurchase", new { orderHistoryId = model.OrderHistoryID });
            }
        }

        public IActionResult CardInfo(int orderHistoryId)
        {
            var model = new CardInfoViewModel(orderHistoryId);
            return View(model);
        }

        [HttpPost]
        public IActionResult CardInfo(CardInfoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // Process card payment
            return RedirectToAction("FinalizePurchase", new { orderHistoryId = model.OrderHistoryID });
        }

        public IActionResult FinalizePurchase(int orderHistoryId)
        {
            var model = new FinalizePurchaseViewModel(orderHistoryId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateBorrowedTax(int productId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // Instead of fetching the product by ID, we'll create a dummy product for now
                var product = new Product
                {
                    ProductId = productId,
                    ProductType = "borrowed", // Assuming this is a borrowed product
                    Price = 100 // Default price that will be modified by the borrowed tax calculation
                };
                
                var model = new BillingInfoViewModel();
                model.StartDate = startDate;
                model.EndDate = endDate;
                await model.ApplyBorrowedTax(product);
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task ProcessWalletRefill(BillingInfoViewModel model)
        {
            double walletBalance = await _dummyWalletService.GetWalletBalanceAsync(1);
            double newBalance = walletBalance - model.Total;
            await _dummyWalletService.UpdateWalletBalance(1, newBalance);
        }
    }
} 