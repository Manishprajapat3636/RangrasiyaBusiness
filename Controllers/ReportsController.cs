
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using System.Security.Claims;

namespace RangrasiyaBusiness.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index(
            DateTime? fromDate,
            DateTime? toDate)
        {
            string userId = CurrentUserId;

            DateTime from = fromDate ??
                new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    1);

            DateTime to = (toDate ?? DateTime.Now.Date).Date;

            DateTime endDate = to.AddDays(1);

            // Only logged-in user's invoices
            var invoices = await _context.Invoices
                .Where(i =>
                    i.UserId == userId &&
                    i.InvoiceDate >= from &&
                    i.InvoiceDate < endDate)
                .Include(i => i.Customer)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            // Only logged-in user's expenses
            var expenses = await _context.WorkExpenses
                .Where(e =>
                    e.UserId == userId &&
                    e.ExpenseDate >= from &&
                    e.ExpenseDate < endDate)
                .Include(e => e.Work)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();

            decimal totalSales =
                invoices.Sum(i => i.GrandTotal);

            decimal totalExpenses =
                expenses.Sum(e => e.Amount);

            // Only logged-in user's works
            int totalWorks = await _context.Works
                .CountAsync(w => w.UserId == userId);

            var model = new ReportsViewModel
            {
                FromDate = from,
                ToDate = to,

                TotalInvoices = invoices.Count,
                TotalSales = totalSales,
                TotalExpenses = totalExpenses,
                NetProfit = totalSales - totalExpenses,

                TotalWorks = totalWorks,

                Invoices = invoices,
                Expenses = expenses
            };

            return View(model);
        }
    }
}
