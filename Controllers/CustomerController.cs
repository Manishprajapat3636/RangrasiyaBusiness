
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RangrasiyaBusiness.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Customer
        public async Task<IActionResult> Index(string? search)
        {
            var userId = CurrentUserId;

            var customers = _context.Customers
                .Where(c => c.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                customers = customers.Where(c =>
                    c.CustomerName.Contains(search) ||
                    c.City.Contains(search) ||
                    c.Phone.Contains(search));
            }

            ViewBag.Search = search;

            return View(await customers.ToListAsync());
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.UserId = CurrentUserId;

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.CustomerId == id &&
                    c.UserId == CurrentUserId);

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.CustomerId)
                return NotFound();

            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.CustomerId == id &&
                    c.UserId == CurrentUserId);

            if (existingCustomer == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                existingCustomer.CustomerName = customer.CustomerName;
                existingCustomer.ContactPerson = customer.ContactPerson;
                existingCustomer.City = customer.City;
                existingCustomer.Phone = customer.Phone;
                existingCustomer.GSTIN = customer.GSTIN;
                existingCustomer.Address = customer.Address;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.CustomerId == id &&
                    c.UserId == CurrentUserId);

            if (customer == null)
                return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
