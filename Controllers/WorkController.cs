using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using System.Security.Claims;

[Authorize]
public class WorkController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;


    // GET: Works
    public async Task<IActionResult> Index()
    {
        var works = await _context.Works
            .Where(w => w.UserId == CurrentUserId)
            .Include(w => w.Customer)
            .ToListAsync();

        return View(works);
    }


    // GET: Works/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var work = await _context.Works
            .Where(w => w.UserId == CurrentUserId)
            .Include(w => w.Customer)
            .Include(w => w.WorkExpenses)
            .FirstOrDefaultAsync(w => w.WorkId == id);

        if (work == null)
            return NotFound();

        return View(work);
    }


    // GET: Works/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.CustomerId = new SelectList(
            await _context.Customers
                .Where(c => c.UserId == CurrentUserId)
                .ToListAsync(),
            "CustomerId",
            "CustomerName");

        return View();
    }


    // POST: Works/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("WorkId,WorkName,CustomerId,StartDate,EndDate,Status,Description,WorkLocation,EstimatedAmount,ActualAmount")]
        Work work)
    {
        var customerExists = await _context.Customers
            .AnyAsync(c =>
                c.CustomerId == work.CustomerId &&
                c.UserId == CurrentUserId);

        if (!customerExists)
        {
            ModelState.AddModelError(
                "CustomerId",
                "Please select a valid customer.");
        }


        if (ModelState.IsValid)
        {
            work.UserId = CurrentUserId;

            _context.Works.Add(work);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        ViewBag.CustomerId = new SelectList(
            await _context.Customers
                .Where(c => c.UserId == CurrentUserId)
                .ToListAsync(),
            "CustomerId",
            "CustomerName",
            work.CustomerId);

        return View(work);
    }


    // GET: Works/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var work = await _context.Works
            .Include(w => w.Customer)
            .FirstOrDefaultAsync(w =>
                w.WorkId == id &&
                w.UserId == CurrentUserId);

        if (work == null)
            return NotFound();


        ViewData["CustomerId"] = new SelectList(
            await _context.Customers
                .Where(c => c.UserId == CurrentUserId)
                .ToListAsync(),
            "CustomerId",
            "CustomerName",
            work.CustomerId);


        return View(work);
    }


    // POST: Works/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("WorkId,WorkName,CustomerId,StartDate,EndDate,Status,Description,WorkLocation,EstimatedAmount,ActualAmount")]
        Work work)
    {
        if (id != work.WorkId)
            return NotFound();


        var existingWork = await _context.Works
            .FirstOrDefaultAsync(w =>
                w.WorkId == id &&
                w.UserId == CurrentUserId);

        if (existingWork == null)
            return NotFound();


        var customerExists = await _context.Customers
            .AnyAsync(c =>
                c.CustomerId == work.CustomerId &&
                c.UserId == CurrentUserId);

        if (!customerExists)
        {
            ModelState.AddModelError(
                "CustomerId",
                "Please select a valid customer.");
        }


        if (ModelState.IsValid)
        {
            existingWork.WorkName = work.WorkName;
            existingWork.CustomerId = work.CustomerId;
            existingWork.StartDate = work.StartDate;
            existingWork.EndDate = work.EndDate;
            existingWork.Status = work.Status;
            existingWork.Description = work.Description;
            existingWork.WorkLocation = work.WorkLocation;
            existingWork.EstimatedAmount = work.EstimatedAmount;
            existingWork.ActualAmount = work.ActualAmount;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        ViewBag.CustomerId = new SelectList(
            await _context.Customers
                .Where(c => c.UserId == CurrentUserId)
                .ToListAsync(),
            "CustomerId",
            "CustomerName",
            work.CustomerId);

        return View(work);
    }


    // GET: Works/Delete/5
    public async Task<IActionResult> Delete(int? workid)
    {
        if (workid == null)
            return NotFound();


        var work = await _context.Works
            .FirstOrDefaultAsync(w =>
                w.WorkId == workid &&
                w.UserId == CurrentUserId);

        if (work == null)
            return NotFound();


        return View(work);
    }


    // POST: Works/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? workid)
    {
        if (workid == null)
            return NotFound();


        var work = await _context.Works
            .FirstOrDefaultAsync(w =>
                w.WorkId == workid &&
                w.UserId == CurrentUserId);

        if (work == null)
            return NotFound();


        _context.Works.Remove(work);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    private bool WorkExists(int workid)
    {
        return _context.Works.Any(w =>
            w.WorkId == workid &&
            w.UserId == CurrentUserId);
    }
}