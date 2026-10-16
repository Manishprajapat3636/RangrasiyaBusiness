using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using System.Security.Claims;

namespace RangrasiyaBusiness.Controllers
{
    [Authorize]
    public class QuotationConditionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuotationConditionController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;


        // =========================================================
        // INDEX - SHOW ALL CONDITIONS
        // =========================================================

        public async Task<IActionResult> Index(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Customer)
                .FirstOrDefaultAsync(q =>
                    q.QuotationId == quotationId &&
                    q.UserId == CurrentUserId);

            if (quotation == null)
                return NotFound();

            var conditions = await _context.QuotationConditions
                .Where(c =>
                    c.QuotationId == quotationId &&
                    c.Quotation!.UserId == CurrentUserId)
                .OrderBy(c => c.QuotationConditionId)
                .ToListAsync();

            ViewBag.Quotation = quotation;

            return View(conditions);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Customer)
                .FirstOrDefaultAsync(q =>
                    q.QuotationId == quotationId &&
                    q.UserId == CurrentUserId);

            if (quotation == null)
                return NotFound();

            var condition = new QuotationCondition
            {
                QuotationId = quotationId
            };

            ViewBag.Quotation = quotation;

            return View(condition);
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            QuotationCondition condition)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Customer)
                .FirstOrDefaultAsync(q =>
                    q.QuotationId == condition.QuotationId &&
                    q.UserId == CurrentUserId);

            if (quotation == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(condition.ConditionText))
            {
                ModelState.AddModelError(
                    "ConditionText",
                    "Please enter a condition.");
            }

            if (ModelState.IsValid)
            {
                condition.ConditionText =
                    condition.ConditionText.Trim();

                _context.QuotationConditions.Add(condition);

                await _context.SaveChangesAsync();

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        quotationId = condition.QuotationId
                    });
            }

            ViewBag.Quotation = quotation;

            return View(condition);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var condition = await _context.QuotationConditions
                .Include(c => c.Quotation)
                    .ThenInclude(q => q!.Customer)
                .FirstOrDefaultAsync(c =>
                    c.QuotationConditionId == id &&
                    c.Quotation!.UserId == CurrentUserId);

            if (condition == null)
                return NotFound();

            return View(condition);
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var condition = await _context.QuotationConditions
                .Include(c => c.Quotation)
                    .ThenInclude(q => q!.Customer)
                .FirstOrDefaultAsync(c =>
                    c.QuotationConditionId == id &&
                    c.Quotation!.UserId == CurrentUserId);

            if (condition == null)
                return NotFound();

            ViewBag.Quotation = condition.Quotation;

            return View(condition);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            QuotationCondition condition)
        {
            if (id != condition.QuotationConditionId)
                return NotFound();

            var existingCondition =
                await _context.QuotationConditions
                    .Include(c => c.Quotation)
                        .ThenInclude(q => q!.Customer)
                    .FirstOrDefaultAsync(c =>
                        c.QuotationConditionId == id &&
                        c.Quotation!.UserId == CurrentUserId);

            if (existingCondition == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(condition.ConditionText))
            {
                ModelState.AddModelError(
                    "ConditionText",
                    "Please enter a condition.");
            }

            if (ModelState.IsValid)
            {
                existingCondition.ConditionText =
                    condition.ConditionText.Trim();

                await _context.SaveChangesAsync();

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        quotationId =
                            existingCondition.QuotationId
                    });
            }

            ViewBag.Quotation = existingCondition.Quotation;

            return View(condition);
        }


        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var condition = await _context.QuotationConditions
                .Include(c => c.Quotation)
                    .ThenInclude(q => q!.Customer)
                .FirstOrDefaultAsync(c =>
                    c.QuotationConditionId == id &&
                    c.Quotation!.UserId == CurrentUserId);

            if (condition == null)
                return NotFound();

            return View(condition);
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var condition =
                await _context.QuotationConditions
                    .Include(c => c.Quotation)
                    .FirstOrDefaultAsync(c =>
                        c.QuotationConditionId == id &&
                        c.Quotation!.UserId == CurrentUserId);

            if (condition == null)
                return NotFound();

            int quotationId = condition.QuotationId;

            _context.QuotationConditions.Remove(condition);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index),
                new { quotationId });
        }
    }
}