using Microsoft.AspNetCore.Mvc;
using WebMarketplace.Models;


namespace WebMarketplace.Controllers
{
    public class BuyerFamilySyncItemController : Controller
    {
        // GET: BuyerFamilySyncItem
        public IActionResult Index()
        {
            return View(); // No mock data
        }

        // GET: BuyerFamilySyncItem/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(); // No mock data
        }

        // GET: BuyerFamilySyncItem/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BuyerFamilySyncItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,DisplayName,Status")] BuyerFamilySyncItemViewModel buyerFamilySyncItemViewModel)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(buyerFamilySyncItemViewModel);
        }

        // GET: BuyerFamilySyncItem/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(); // No mock data
        }

        // POST: BuyerFamilySyncItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,DisplayName,Status")] BuyerFamilySyncItemViewModel buyerFamilySyncItemViewModel)
        {
            if (id != buyerFamilySyncItemViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(buyerFamilySyncItemViewModel);
        }

        // GET: BuyerFamilySyncItem/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(); // No mock data
        }

        // POST: BuyerFamilySyncItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
