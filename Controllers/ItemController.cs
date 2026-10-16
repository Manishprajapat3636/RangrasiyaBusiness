
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RangrasiyaBusiness.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Item
        public async Task<IActionResult> Index()
        {
            var items = await _context.Items
                .Where(i => i.UserId == CurrentUserId)
                .ToListAsync();

            return View(items);
        }

        // GET: Item/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Item/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item item)
        {
            if (ModelState.IsValid)
            {
                item.UserId = CurrentUserId;

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        // GET: Item/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var item = await _context.Items
                .FirstOrDefaultAsync(i =>
                    i.ItemId == id &&
                    i.UserId == CurrentUserId);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // POST: Item/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item)
        {
            if (id != item.ItemId)
                return NotFound();

            var existingItem = await _context.Items
                .FirstOrDefaultAsync(i =>
                    i.ItemId == id &&
                    i.UserId == CurrentUserId);

            if (existingItem == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                existingItem.ItemName = item.ItemName;
                existingItem.Category = item.Category;
                existingItem.Unit = item.Unit;
                existingItem.Rate = item.Rate;
                existingItem.GSTPercentage = item.GSTPercentage;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        // GET: Item/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var item = await _context.Items
                .FirstOrDefaultAsync(i =>
                    i.ItemId == id &&
                    i.UserId == CurrentUserId);

            if (item == null)
                return NotFound();

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
