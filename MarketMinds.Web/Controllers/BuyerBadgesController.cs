using Microsoft.AspNetCore.Mvc;
using WebMarketplace.Models;

namespace WebMarketplace.Controllers
{
    public class BuyerBadgesController : Controller
    {
        // GET: BuyerBadges
        public IActionResult Index()
        {
            return View(); // No mock data
        }

        // GET: BuyerBadges/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(); // No mock data
        }

        // GET: BuyerBadges/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BuyerBadges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,BadgeName,Discount,Progress,ImageSource")] BuyerBadgeViewModel buyerBadgeViewModel)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(buyerBadgeViewModel);
        }

        // GET: BuyerBadges/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(); // No mock data
        }

        // POST: BuyerBadges/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,BadgeName,Discount,Progress,ImageSource")] BuyerBadgeViewModel buyerBadgeViewModel)
        {
            if (id != buyerBadgeViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(buyerBadgeViewModel);
        }

        // GET: BuyerBadges/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(); // No mock data
        }

        // POST: BuyerBadges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
