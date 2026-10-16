using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using System.Security.Claims;

[Authorize]
public class CompanyProfileController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly ApplicationDbContext _context;

    public CompanyProfileController(
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: CompanyProfile/Index
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.UserId == CurrentUserId);

        return View(company);
    }

    // GET: CompanyProfile/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var existingCompany = await _context.CompanyProfiles
            .AnyAsync(c => c.UserId == CurrentUserId);

        if (existingCompany)
            return RedirectToAction(nameof(Index));

        return View();
    }

    // POST: CompanyProfile/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyProfile company)
    {
        var existingCompany = await _context.CompanyProfiles
            .AnyAsync(c => c.UserId == CurrentUserId);

        if (existingCompany)
            return RedirectToAction(nameof(Index));

        if (!ModelState.IsValid)
            return View(company);

        company.UserId = CurrentUserId;

        if (company.LogoFile != null)
        {
            company.LogoPath = await SaveLogo(company.LogoFile);
        }

        _context.CompanyProfiles.Add(company);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: CompanyProfile/Edit
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var company = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.UserId == CurrentUserId);

        if (company == null)
            return RedirectToAction(nameof(Create));

        return View(company);
    }

    // POST: CompanyProfile/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CompanyProfile company)
    {
        var existingCompany = await _context.CompanyProfiles
            .FirstOrDefaultAsync(c =>
                c.CompanyProfileId == company.CompanyProfileId &&
                c.UserId == CurrentUserId);

        if (existingCompany == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(company);

        // Company information
        existingCompany.CompanyName = company.CompanyName;
        existingCompany.OwnerName = company.OwnerName;
        existingCompany.Phone = company.Phone;
        existingCompany.Email = company.Email;
        existingCompany.Address = company.Address;
        existingCompany.City = company.City;
        existingCompany.State = company.State;
        existingCompany.Pincode = company.Pincode;
        existingCompany.BusinessType = company.BusinessType;

        // Tax & registration
        existingCompany.GSTIN = company.GSTIN;
        existingCompany.PAN = company.PAN;
        existingCompany.UDYAMNumber = company.UDYAMNumber;

        // Bank details
        existingCompany.BankName = company.BankName;
        existingCompany.AccountNumber = company.AccountNumber;
        existingCompany.IFSCCode = company.IFSCCode;
        existingCompany.Branch = company.Branch;

        // Document settings
        existingCompany.TermsAndConditions = company.TermsAndConditions;

        // Update logo only when a new logo is selected
        if (company.LogoFile != null)
        {
            existingCompany.LogoPath = await SaveLogo(company.LogoFile);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Save uploaded logo
    private async Task<string> SaveLogo(IFormFile logoFile)
    {
        string folder = Path.Combine(
            _environment.WebRootPath,
            "images");

        Directory.CreateDirectory(folder);

        string extension = Path.GetExtension(logoFile.FileName)
            .ToLowerInvariant();

        string[] allowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

        string fileName = $"{Guid.NewGuid()}{extension}";

        string filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(
            filePath,
            FileMode.Create))
        {
            await logoFile.CopyToAsync(stream);
        }

        return "/images/" + fileName;
    }
}
