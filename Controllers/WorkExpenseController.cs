
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using System.Security.Claims;

[Authorize]
public class WorkExpenseController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkExpenseController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: WorkExpense
    public async Task<IActionResult> Index(
        string searchString,
        string expenseType,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var expenses = _context.WorkExpenses
            .Where(e => e.UserId == CurrentUserId)
            .Include(e => e.Work)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            expenses = expenses.Where(e =>
                e.Description.Contains(searchString) ||
                e.Notes.Contains(searchString));
        }

        if (!string.IsNullOrWhiteSpace(expenseType))
        {
            expenses = expenses.Where(e =>
                e.ExpenseType == expenseType);
        }

        if (fromDate.HasValue)
        {
            expenses = expenses.Where(e =>
                e.ExpenseDate.Date >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            expenses = expenses.Where(e =>
                e.ExpenseDate.Date <= toDate.Value.Date);
        }

        ViewBag.SearchString = searchString;
        ViewBag.ExpenseType = expenseType;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        LoadExpenseTypes(expenseType);

        return View(await expenses.ToListAsync());
    }

    // GET: WorkExpense/Create
    public async Task<IActionResult> Create(int? workId)
    {
        await LoadWorks(workId);
        LoadExpenseTypes();

        var expense = new WorkExpense();

        if (workId.HasValue)
        {
            var validWork = await _context.Works.AnyAsync(w =>
                w.WorkId == workId.Value &&
                w.UserId == CurrentUserId);

            if (!validWork)
                return NotFound();

            expense.WorkId = workId.Value;
        }

        return View(expense);
    }

    // POST: WorkExpense/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkExpense expense)
    {
        var validWork = await _context.Works.AnyAsync(w =>
            w.WorkId == expense.WorkId &&
            w.UserId == CurrentUserId);

        if (!validWork)
        {
            ModelState.AddModelError(
                "WorkId",
                "Invalid work selected.");
        }

        if (ModelState.IsValid)
        {
            expense.UserId = CurrentUserId;

            _context.WorkExpenses.Add(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await LoadWorks(expense.WorkId);
        LoadExpenseTypes(expense.ExpenseType);

        return View(expense);
    }

    // GET: WorkExpense/Edit
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var expense = await _context.WorkExpenses
            .Where(e =>
                e.WorkExpenseId == id &&
                e.UserId == CurrentUserId)
            .FirstOrDefaultAsync();

        if (expense == null)
            return NotFound();

        await LoadWorks(expense.WorkId);
        LoadExpenseTypes(expense.ExpenseType);

        return View(expense);
    }

    // POST: WorkExpense/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        WorkExpense expense)
    {
        if (id != expense.WorkExpenseId)
            return NotFound();

        var existingExpense = await _context.WorkExpenses
            .Where(e =>
                e.WorkExpenseId == id &&
                e.UserId == CurrentUserId)
            .FirstOrDefaultAsync();

        if (existingExpense == null)
            return NotFound();

        var validWork = await _context.Works.AnyAsync(w =>
            w.WorkId == expense.WorkId &&
            w.UserId == CurrentUserId);

        if (!validWork)
        {
            ModelState.AddModelError(
                "WorkId",
                "Invalid work selected.");
        }

        if (ModelState.IsValid)
        {
            existingExpense.WorkId = expense.WorkId;
            existingExpense.ExpenseDate = expense.ExpenseDate;
            existingExpense.ExpenseType = expense.ExpenseType;
            existingExpense.Description = expense.Description;
            existingExpense.Amount = expense.Amount;
            existingExpense.Notes = expense.Notes;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await LoadWorks(expense.WorkId);
        LoadExpenseTypes(expense.ExpenseType);

        return View(expense);
    }

    // GET: WorkExpense/Delete
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var expense = await _context.WorkExpenses
            .Where(e =>
                e.WorkExpenseId == id &&
                e.UserId == CurrentUserId)
            .Include(e => e.Work)
            .FirstOrDefaultAsync();

        if (expense == null)
            return NotFound();

        return View(expense);
    }

    // POST: WorkExpense/Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var expense = await _context.WorkExpenses
            .Where(e =>
                e.WorkExpenseId == id &&
                e.UserId == CurrentUserId)
            .FirstOrDefaultAsync();

        if (expense == null)
            return NotFound();

        _context.WorkExpenses.Remove(expense);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Load Works Dropdown
    private async Task LoadWorks(int? selectedWorkId = null)
    {
        ViewBag.WorkId = new SelectList(
            await _context.Works
                .Where(w => w.UserId == CurrentUserId)
                .OrderBy(w => w.WorkName)
                .ToListAsync(),
            "WorkId",
            "WorkName",
            selectedWorkId);
    }

    // Load Expense Types Dropdown
    private void LoadExpenseTypes(string? selectedType = null)
    {
        var expenseTypes = new[]
        {
            "Material",
            "Labour",
            "Transport",
            "Paint",
            "Equipment",
            "Other"
        };

        ViewBag.ExpenseTypes = new SelectList(
            expenseTypes,
            selectedType);
    }
}
