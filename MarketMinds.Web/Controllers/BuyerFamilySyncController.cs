using Microsoft.AspNetCore.Mvc;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class BuyerFamilySyncController : Controller
    {
        // GET: BuyerFamilySync
        public IActionResult Index()
        {
            return View(); // No mock data
        }

        // GET: BuyerFamilySync/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(); // No mock data
        }

        // GET: BuyerFamilySync/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BuyerFamilySync/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,LinkedBuyerName,Status")] BuyerFamilySyncViewModel buyerFamilySyncViewModel)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(buyerFamilySyncViewModel);
        }

        // GET: BuyerFamilySync/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(); // No mock data
        }

        // POST: BuyerFamilySync/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,LinkedBuyerName,Status")] BuyerFamilySyncViewModel buyerFamilySyncViewModel)
        {
            if (id != buyerFamilySyncViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(buyerFamilySyncViewModel);
        }

        // GET: BuyerFamilySync/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(); // No mock data
        }

        // POST: BuyerFamilySync/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
