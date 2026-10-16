using ClosedXML.Excel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using System.Security.Claims;

namespace RangrasiyaBusiness.Controllers
{
    [Authorize]
    public class WorkListController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public WorkListController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: WorkList
        public async Task<IActionResult> Index()
        {
            var workLists = await _context.WorkLists
                .Where(w => w.UserId == CurrentUserId)
                .Include(w => w.Customer)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(workLists);
        }

        // GET: WorkList/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var workList = await _context.WorkLists
                .Where(w => w.UserId == CurrentUserId)
                .Include(w => w.Customer)
                .Include(w => w.WorkListMeasurements)
                .FirstOrDefaultAsync(w => w.WorkListId == id);

            if (workList == null)
                return NotFound();

            return View(workList);
        }

        // GET: WorkList/Create
        public async Task<IActionResult> Create()
        {
            await LoadCustomersAsync();
            return View();
        }

        // POST: WorkList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            WorkList workList,
            List<WorkListMeasurement> measurements)
        {
            RemoveMeasurementValidation();

            await ValidateCustomerAsync(workList);

            var duplicate = await _context.WorkLists.AnyAsync(w =>
                w.UserId == CurrentUserId &&
                w.WorkOrderNo == workList.WorkOrderNo);

            if (duplicate)
            {
                ModelState.AddModelError(
                    nameof(workList.WorkOrderNo),
                    "This Work Order Number already exists.");
            }

            ValidateMeasurements(measurements);

            if (!ModelState.IsValid)
            {
                await LoadCustomersAsync(workList.CustomerId);
                return View(workList);
            }

            workList.UserId = CurrentUserId;
            workList.CreatedAt = DateTime.Now;

            _context.WorkLists.Add(workList);

            // Save the parent first to generate WorkListId.
            await _context.SaveChangesAsync();

            foreach (var measurement in ValidMeasurements(measurements))
            {
                measurement.WorkListId = workList.WorkListId;

                // SQL Server requires Location to have a value.
                measurement.Location ??= string.Empty;

                measurement.Total = CalculateTotal(measurement);

                _context.WorkListMeasurements.Add(measurement);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: WorkList/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var workList = await _context.WorkLists
                .Where(w => w.UserId == CurrentUserId)
                .Include(w => w.WorkListMeasurements)
                .FirstOrDefaultAsync(w => w.WorkListId == id);

            if (workList == null)
                return NotFound();

            await LoadCustomersAsync(workList.CustomerId);
            return View(workList);
        }

        // POST: WorkList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            WorkList workList,
            List<WorkListMeasurement> measurements)
        {
            if (id != workList.WorkListId)
                return NotFound();

            var existing = await _context.WorkLists
                .Include(w => w.WorkListMeasurements)
                .FirstOrDefaultAsync(w =>
                    w.WorkListId == id &&
                    w.UserId == CurrentUserId);

            if (existing == null)
                return NotFound();

            RemoveMeasurementValidation();

            await ValidateCustomerAsync(workList);

            var duplicate = await _context.WorkLists.AnyAsync(w =>
                w.UserId == CurrentUserId &&
                w.WorkListId != id &&
                w.WorkOrderNo == workList.WorkOrderNo);

            if (duplicate)
            {
                ModelState.AddModelError(
                    nameof(workList.WorkOrderNo),
                    "This Work Order Number already exists.");
            }

            ValidateMeasurements(measurements);

            if (!ModelState.IsValid)
            {
                await LoadCustomersAsync(workList.CustomerId);
                return View(workList);
            }

            existing.WorkOrderNo = workList.WorkOrderNo;
            existing.QuotationNo = workList.QuotationNo;
            existing.BillNo = workList.BillNo;
            existing.CustomerId = workList.CustomerId;
            existing.OfficerName = workList.OfficerName;
            existing.OfficerContact = workList.OfficerContact;
            existing.WorkType = workList.WorkType;
            existing.FromDate = workList.FromDate;
            existing.ToDate = workList.ToDate;
            existing.Department = workList.Department;
            existing.WorkSiteAddress = workList.WorkSiteAddress;

            // Replace existing measurement rows with submitted rows.
            _context.WorkListMeasurements.RemoveRange(
                existing.WorkListMeasurements);

            foreach (var measurement in ValidMeasurements(measurements))
            {
                measurement.WorkListMeasurementId = 0;
                measurement.WorkListId = existing.WorkListId;
                measurement.WorkList = null;
                measurement.Total = CalculateTotal(measurement);

                _context.WorkListMeasurements.Add(measurement);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: WorkList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var workList = await _context.WorkLists
                .Include(w => w.Customer)
                .FirstOrDefaultAsync(w =>
                    w.WorkListId == id &&
                    w.UserId == CurrentUserId);

            if (workList == null)
                return NotFound();

            return View(workList);
        }

        // POST: WorkList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workList = await _context.WorkLists
                .Include(w => w.WorkListMeasurements)
                .FirstOrDefaultAsync(w =>
                    w.WorkListId == id &&
                    w.UserId == CurrentUserId);

            if (workList == null)
                return NotFound();

            _context.WorkListMeasurements.RemoveRange(
                workList.WorkListMeasurements);

            _context.WorkLists.Remove(workList);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCustomersAsync(
            int? selectedCustomerId = null)
        {
            var customers = await _context.Customers
                .Where(c => c.UserId == CurrentUserId)
                .OrderBy(c => c.CustomerName)
                .ToListAsync();

            ViewBag.CustomerId = new SelectList(
                customers,
                "CustomerId",
                "CustomerName",
                selectedCustomerId);
        }

        private async Task ValidateCustomerAsync(WorkList workList)
        {
            var customerExists = await _context.Customers.AnyAsync(c =>
                c.CustomerId == workList.CustomerId &&
                c.UserId == CurrentUserId);

            if (!customerExists)
            {
                ModelState.AddModelError(
                    nameof(workList.CustomerId),
                    "Please select a valid customer.");
            }
        }

        private void RemoveMeasurementValidation()
        {
            var keys = ModelState.Keys
                .Where(k => k.StartsWith(
                    "measurements[",
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keys)
            {
                ModelState.Remove(key);
            }
        }

        private void ValidateMeasurements(
            List<WorkListMeasurement>? measurements)
        {
            if (measurements == null)
                return;

            foreach (var measurement in ValidMeasurements(measurements))
            {
                if (string.IsNullOrWhiteSpace(measurement.Description))
                {
                    ModelState.AddModelError(
                        nameof(measurement.Description),
                        "Enter a description for every measurement row.");
                    break;
                }

                if (measurement.Length < 0 || measurement.Width < 0)
                {
                    ModelState.AddModelError(
                        nameof(measurement.Length),
                        "Length and width cannot be negative.");
                    break;
                }
            }
        }

        private static IEnumerable<WorkListMeasurement> ValidMeasurements(
            IEnumerable<WorkListMeasurement>? measurements)
        {
            return (measurements ?? Enumerable.Empty<WorkListMeasurement>())
                .Where(m =>
                    !string.IsNullOrWhiteSpace(m.Description) ||
                    !string.IsNullOrWhiteSpace(m.Location) ||
                    m.Length != 0 ||
                    m.Width != 0);
        }

        private static decimal CalculateTotal(
            WorkListMeasurement measurement)
        {
            // Treat an omitted/invalid quantity as 1.
            decimal quantity = measurement.Quantity > 0
                ? measurement.Quantity
                : 1m;

            // Square units use Length × Width × Quantity.
            if (measurement.Unit == "Sq.Ft" ||
                measurement.Unit == "Sq.M")
            {
                return measurement.Length * measurement.Width * quantity;
            }

            // Linear/count/weight/volume units use Length × Quantity.
            return measurement.Length * quantity;
        }


        [HttpGet]
        public async Task<IActionResult> ExportExcel(int id)
        {
            var workList = await _context.WorkLists.AsNoTracking()
                .Where(w => w.WorkListId == id && w.UserId == CurrentUserId)
                .Include(w => w.Customer).Include(w => w.WorkListMeasurements)
                .FirstOrDefaultAsync();
            if (workList == null) return NotFound();
            var company = await _context.CompanyProfiles.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == CurrentUserId);
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Work List");
            sheet.Style.Font.FontName = "Arial"; sheet.Style.Font.FontSize = 10;
            sheet.Column(1).Width = 8; sheet.Column(2).Width = 30; sheet.Column(3).Width = 24;
            sheet.Column(4).Width = 13; sheet.Column(5).Width = 13; sheet.Column(6).Width = 14;
            sheet.Range("C1:H1").Merge(); sheet.Cell("C1").Value = (company?.CompanyName ?? "RANGRASIYA").ToUpperInvariant();
            sheet.Range("C1:H1").Style.Font.Bold = true; sheet.Range("C1:H1").Style.Font.FontSize = 19;
            sheet.Range("C1:H1").Style.Font.FontColor = XLColor.FromHtml("#1F4E78"); sheet.Range("C1:H1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range("C2:H2").Merge(); sheet.Cell("C2").Value = company?.BusinessType ?? "MARKETING, ADVERTISING AND PAINTING WORKS";
            sheet.Range("C2:H2").Style.Font.Bold = true; sheet.Range("C2:H2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range("C3:H3").Merge(); sheet.Cell("C3").Value = "Work Order & Measurement Sheet"; sheet.Range("C3:H3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            var logo = string.IsNullOrWhiteSpace(company?.LogoPath) ? null : Path.Combine(_environment.WebRootPath, company.LogoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            TryAddCompanyLogo(sheet, logo, "A1", 60, 48);
            sheet.Range("A5:D5").Merge(); sheet.Cell("A5").Value = "WORK LIST"; sheet.Cell("A5").Style.Font.Bold = true; sheet.Cell("A5").Style.Font.FontSize = 18; sheet.Cell("A5").Style.Font.FontColor = XLColor.FromHtml("#1F3557");
            sheet.Range("G5:H5").Merge(); sheet.Cell("E5").Value = $"WORK ORDER NO.\n{workList.WorkOrderNo}"; sheet.Range("G5:H5").Style.Font.Bold = true; sheet.Range("G5:H5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right; sheet.Range("G5:H5").Style.Alignment.WrapText = true;
            sheet.Cell("A7").Value = "QUOTATION NO."; sheet.Cell("B7").Value = workList.QuotationNo ?? ""; sheet.Cell("C7").Value = "BILL NO."; sheet.Cell("D7").Value = workList.BillNo ?? ""; sheet.Cell("E7").Value = "CREATED ON"; sheet.Cell("F7").Value = workList.CreatedAt;
            sheet.Range("A7:H7").Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9"); sheet.Range("A7:H7").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            StyleSection(sheet, 9, 1, 6, "CUSTOMER & WORK INFORMATION");
            string[,] fields = { { "CUSTOMER NAME", workList.Customer?.CustomerName ?? "" }, { "OFFICER NAME", workList.OfficerName ?? "" }, { "OFFICER CONTACT", workList.OfficerContact ?? "" }, { "WORK TYPE", workList.WorkType ?? "" }, { "DEPARTMENT", workList.Department ?? "" }, { "FROM DATE", workList.FromDate?.ToString("dd-MM-yyyy") ?? "" }, { "TO DATE", workList.ToDate?.ToString("dd-MM-yyyy") ?? "" }, { "WORK SITE / ADDRESS", workList.WorkSiteAddress ?? "" } };
            for (int i = 0; i < fields.GetLength(0); i++) { int row = 10 + i; int col = (i % 2 == 0) ? 1 : 4; int valCol = (i % 2 == 0) ? 2 : 5; sheet.Cell(row, col).Value = fields[i, 0]; sheet.Cell(row, col).Style.Font.Bold = true; sheet.Range(row, valCol, row, valCol + 1).Merge(); sheet.Cell(row, valCol).Value = fields[i, 1]; }
            sheet.Range("A10:H17").Style.Border.OutsideBorder = XLBorderStyleValues.Thin; sheet.Range("A10:H17").Style.Border.InsideBorder = XLBorderStyleValues.Hair;
            StyleSection(sheet, 19, 1, 8, "WORK MEASUREMENTS");
            string[] heads = { "S.No.", "Work Description", "Work Location", "Unit", "Length", "Width", "Qty" };
            // Work measurements use seven columns, so expand to G.
            sheet.Column(7).Width = 12; sheet.Column(8).Width = 14;
            for (int c = 0; c < heads.Length; c++) sheet.Cell(20, c + 1).Value = heads[c];
            sheet.Cell(20, 8).Value = "Total";
            StyleTableHeader(sheet, 20, 8);
            for (int i = 0; i < workList.WorkListMeasurements.Count; i++) { var m = workList.WorkListMeasurements.ElementAt(i); int r = 21 + i; sheet.Cell(r, 1).Value = i + 1; sheet.Cell(r, 2).Value = m.Description ?? ""; sheet.Cell(r, 3).Value = m.Location ?? ""; sheet.Cell(r, 4).Value = m.Unit ?? ""; sheet.Cell(r, 5).Value = m.Length; sheet.Cell(r, 6).Value = m.Width; sheet.Cell(r, 7).Value = m.Quantity; sheet.Cell(r, 8).Value = m.Total; }
            int last = 20 + workList.WorkListMeasurements.Count; StyleTableBody(sheet, 21, last, 8);
            int totalRow = last + 1; sheet.Range(totalRow, 1, totalRow, 7).Merge(); sheet.Cell(totalRow, 1).Value = "GRAND TOTAL"; sheet.Cell(totalRow, 1).Style.Font.Bold = true; sheet.Cell(totalRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right; sheet.Cell(totalRow, 8).FormulaA1 = $"SUM(H21:H{Math.Max(21, last)})"; sheet.Cell(totalRow, 8).Style.Font.Bold = true; sheet.Range(totalRow, 1, totalRow, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7"); sheet.Range(totalRow, 1, totalRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            int note = totalRow + 2; sheet.Range(note, 1, note, 7).Merge(); sheet.Cell(note, 1).Value = "Note: Measurements and totals are shown in the units specified for each row.";
            sheet.Range(note + 2, 1, note + 2, 7).Merge(); sheet.Cell(note + 2, 1).Value = "Remarks / Instructions:";
            sheet.Range(note + 3, 1, note + 5, 7).Merge(); sheet.Range(note + 3, 1, note + 5, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            int sign = note + 7; sheet.Range(sign, 1, sign, 2).Merge(); sheet.Cell(sign, 1).Value = "Prepared By";
            sheet.Range(sign, 3, sign, 5).Merge(); sheet.Cell(sign, 3).Value = "Customer / Officer";
            sheet.Range(sign, 6, sign, 7).Merge(); sheet.Cell(sign, 6).Value = "Authorised Signatory";
            sheet.Range(sign + 3, 1, sign + 3, 2).Merge(); sheet.Cell(sign + 3, 1).Value = "Name & Signature";
            sheet.Range(sign + 3, 3, sign + 3, 5).Merge(); sheet.Cell(sign + 3, 3).Value = "Name, Signature & Date";
            sheet.Range(sign + 3, 6, sign + 3, 7).Merge(); sheet.Cell(sign + 3, 6).Value = $"For {company?.CompanyName ?? "Rangrasiya"}";
            sheet.Range(sign, 1, sign + 3, 7).Style.Font.Bold = true; sheet.Range(sign, 1, sign + 3, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range("A1:H" + (sign + 3)).Style.Border.BottomBorderColor = XLColor.FromHtml("#AAB7C4");
            sheet.Column(5).Style.NumberFormat.Format = "0.00"; sheet.Column(6).Style.NumberFormat.Format = "0.00"; sheet.Column(7).Style.NumberFormat.Format = "0.00"; sheet.Column(8).Style.NumberFormat.Format = "0.00";
            sheet.Column(2).Style.Alignment.WrapText = true; sheet.Column(3).Style.Alignment.WrapText = true;
            sheet.Rows().AdjustToContents(); SetPrintLayout(sheet, sign + 3, 8);
            using var stream = new MemoryStream(); workbook.SaveAs(stream);
            var safe = string.Join("_", (workList.WorkOrderNo ?? $"WorkList-{id}").Split(Path.GetInvalidFileNameChars()));
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{safe}.xlsx");
        }


        private static void StyleDocumentTitle(IXLWorksheet sheet, string title, int lastColumn)
        {
            sheet.Range(1, 1, 2, lastColumn).Merge();
            sheet.Cell(1, 1).Value = title;
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 20;
            sheet.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#1F3557");
            sheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            sheet.Range(1, 1, 2, lastColumn).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");
            sheet.Row(1).Height = 25;
            sheet.Row(2).Height = 18;
        }

        private static void StyleSection(IXLWorksheet sheet, int row, int firstColumn, int lastColumn, string title)
        {
            sheet.Range(row, firstColumn, row, lastColumn).Merge();
            var cell = sheet.Cell(row, firstColumn);
            cell.Value = title;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E78");
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            sheet.Row(row).Height = 22;
        }

        private static void StyleTableHeader(IXLWorksheet sheet, int row, int columns)
        {
            var range = sheet.Range(row, 1, row, columns);
            range.Style.Font.Bold = true;
            range.Style.Font.FontColor = XLColor.FromHtml("#1F3557");
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Style.Alignment.WrapText = true;
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            sheet.Row(row).Height = 32;
        }

        private static void StyleTableBody(IXLWorksheet sheet, int firstRow, int lastRow, int columns)
        {
            if (lastRow < firstRow) return;
            var range = sheet.Range(firstRow, 1, lastRow, columns);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = XLColor.FromHtml("#AAB7C4");
            range.Style.Border.InsideBorderColor = XLColor.FromHtml("#D0D7DE");
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        private static void SetPrintLayout(IXLWorksheet sheet, int lastRow, int lastColumn)
        {
            sheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
            sheet.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            sheet.PageSetup.FitToPages(1, 1);
            sheet.PageSetup.PagesWide = 1;
            sheet.PageSetup.PagesTall = 1;
            sheet.PageSetup.Margins.Top = 0.35;
            sheet.PageSetup.Margins.Bottom = 0.35;
            sheet.PageSetup.Margins.Left = 0.25;
            sheet.PageSetup.Margins.Right = 0.25;
            sheet.PageSetup.CenterHorizontally = true;
            sheet.PageSetup.PrintAreas.Add(1, 1, lastRow, lastColumn);

        }

        private static void TryAddCompanyLogo(IXLWorksheet sheet, string? logoPath, string cellAddress, int width, int height)
        {
            if (!string.IsNullOrWhiteSpace(logoPath) && System.IO.File.Exists(logoPath))
            {
                try
                {
                    sheet.AddPicture(logoPath)
                        .MoveTo(sheet.Cell(cellAddress))
                        .WithSize(width, height);
                }
                catch
                {
                    // Keep the workbook usable if the configured logo format is unsupported.
                }
            }
        }

    }
}
