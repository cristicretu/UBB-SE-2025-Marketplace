using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;

namespace WebMarketplace.Controllers
{
    public class BuyerAddressesController : Controller
    {
        private readonly IBuyerAddressService _buyerAddressService;

        public BuyerAddressesController(IBuyerAddressService buyerAddressService)
        {
            _buyerAddressService = buyerAddressService;
        }

        // GET: BuyerAddresses
        public async Task<IActionResult> Index()
        {
            var addresses = await _buyerAddressService.GetAllAddressesAsync();
            return View(addresses);
        }

        // GET: BuyerAddresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _buyerAddressService.GetAddressByIdAsync(id.Value);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // GET: BuyerAddresses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BuyerAddresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StreetLine,City,Country,PostalCode")] Address address)
        {
            if (ModelState.IsValid)
            {
                await _buyerAddressService.AddAddressAsync(address);
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }

        // GET: BuyerAddresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _buyerAddressService.GetAddressByIdAsync(id.Value);
            if (address == null)
            {
                return NotFound();
            }
            return View(address);
        }

        // POST: BuyerAddresses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StreetLine,City,Country,PostalCode")] Address address)
        {
            if (id != address.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _buyerAddressService.UpdateAddressAsync(address);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }

        // GET: BuyerAddresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _buyerAddressService.GetAddressByIdAsync(id.Value);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // POST: BuyerAddresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var addressExists = await _buyerAddressService.AddressExistsAsync(id);
            if (!addressExists)
            {
                return NotFound();
            }

            await _buyerAddressService.DeleteAddressAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
