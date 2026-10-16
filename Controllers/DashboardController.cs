
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using System.Security.Claims;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var userId = CurrentUserId;

        var dashboard = new DashboardViewModel
        {
            // Only logged-in user's customers
            TotalCustomers = await _context.Customers
                .CountAsync(c => c.UserId == userId),

            // Only logged-in user's items
            TotalItems = await _context.Items
                .CountAsync(i => i.UserId == userId),

            // Only logged-in user's quotations
            TotalQuotations = await _context.Quotations
                .CountAsync(q => q.UserId == userId),

            // Only logged-in user's invoices
            TotalInvoices = await _context.Invoices
                .CountAsync(i => i.UserId == userId),

            // Only logged-in user's works
            TotalWorks = await _context.Works
                .CountAsync(w => w.UserId == userId),

            // Only logged-in user's invoice amount
            TotalInvoiceAmount = await _context.Invoices
                .Where(i => i.UserId == userId)
                .SumAsync(i => (decimal?)i.GrandTotal) ?? 0,

            // Recent invoices of logged-in user only
            RecentInvoices = await _context.Invoices
                .Where(i => i.UserId == userId)
                .Include(i => i.Customer)
                .OrderByDescending(i => i.InvoiceDate)
                .Take(5)
                .ToListAsync(),

            // Recent quotations of logged-in user only
            RecentQuotations = await _context.Quotations
                .Where(q => q.UserId == userId)
                .Include(q => q.Customer)
                .OrderByDescending(q => q.QuotationDate)
                .Take(5)
                .ToListAsync()
        };

        return View(dashboard);
    }
}
