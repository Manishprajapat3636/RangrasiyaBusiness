using ClosedXML.Excel;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using RangrasiyaBusiness.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RangrasiyaBusiness.Controllers
{
    [Authorize]
    public class QuotationController : Controller
    {
        private readonly QuotationPdfService _pdfService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public QuotationController(
            ApplicationDbContext context,
            QuotationPdfService pdfService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _pdfService = pdfService;
            _environment = environment;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Quotation
        public async Task<IActionResult> Index(string sortBy = "date", string order = "desc")
        {
            var query = _context.Quotations
                .Where(q => q.UserId == CurrentUserId)
                .Include(q => q.Customer)
                .AsQueryable();

            sortBy = (sortBy ?? "date").Trim().ToLowerInvariant();
            order = (order ?? "desc").Trim().ToLowerInvariant() == "asc" ? "asc" : "desc";

            query = sortBy switch
            {
                "number" => order == "asc" ? query.OrderBy(q => q.QuotationNumber) : query.OrderByDescending(q => q.QuotationNumber),
                "amount" => order == "asc" ? query.OrderBy(q => q.GrandTotal) : query.OrderByDescending(q => q.GrandTotal),
                "customer" => order == "asc" ? query.OrderBy(q => q.Customer!.CustomerName).ThenBy(q => q.Customer!.ContactPerson) : query.OrderByDescending(q => q.Customer!.CustomerName).ThenByDescending(q => q.Customer!.ContactPerson),
                "status" => order == "asc" ? query.OrderBy(q => q.Status) : query.OrderByDescending(q => q.Status),
                _ => order == "asc" ? query.OrderBy(q => q.QuotationDate).ThenBy(q => q.QuotationId) : query.OrderByDescending(q => q.QuotationDate).ThenByDescending(q => q.QuotationId)
            };

            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = order;
            return View(await query.ToListAsync());
        }

        // GET: Quotation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var quotation = await _context.Quotations
                .Where(q =>
                    q.QuotationId == id &&
                    q.UserId == CurrentUserId)
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems)
                    .ThenInclude(qi => qi.Item)
                .FirstOrDefaultAsync();

            if (quotation == null)
                return NotFound();

            return View(quotation);
        }

        // GET: Quotation/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var quotation = await _context.Quotations
                .Where(q =>
                    q.QuotationId == id &&
                    q.UserId == CurrentUserId)
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems)
                    .ThenInclude(qi => qi.Item)
                .FirstOrDefaultAsync();

            if (quotation == null)
                return NotFound();

            var pdfBytes = _pdfService.GeneratePdf(quotation);

            return File(
                pdfBytes,
                "application/pdf",
                $"Quotation-{quotation.QuotationNumber}.pdf");
        }

        // GET: Quotation/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            return View();
        }

        // POST: Quotation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quotation quotation)
        {
            var validCustomer = await _context.Customers
                .AnyAsync(c =>
                    c.CustomerId == quotation.CustomerId &&
                    c.UserId == CurrentUserId);

            if (!validCustomer)
            {
                ModelState.AddModelError(
                    "CustomerId",
                    "Please select a valid customer.");
            }

            var itemIds = quotation.QuotationItems
                .Select(x => x.ItemId)
                .Distinct()
                .ToList();

            var validItemCount = await _context.Items
                .CountAsync(i =>
                    itemIds.Contains(i.ItemId) &&
                    i.UserId == CurrentUserId);

            if (validItemCount != itemIds.Count)
            {
                ModelState.AddModelError(
                    "",
                    "One or more selected items are invalid.");
            }

            if (ModelState.IsValid)
            {
                quotation.UserId = CurrentUserId;

                var selectedItems = await _context.Items
                    .Where(i => itemIds.Contains(i.ItemId) && i.UserId == CurrentUserId)
                    .ToDictionaryAsync(i => i.ItemId);

                foreach (var line in quotation.QuotationItems)
                {
                    if (selectedItems.TryGetValue(line.ItemId, out var selectedItem))
                    {
                        line.ItemNameSnapshot = selectedItem.ItemName;
                        // Keep rate/GST from the submitted quotation form, as before.
                    }
                }

                quotation.SubTotal =
                    quotation.QuotationItems.Sum(x => x.Amount);

                quotation.GSTAmount =
                    quotation.QuotationItems.Sum(x => x.GSTAmount);

                quotation.GrandTotal =
                    quotation.QuotationItems.Sum(x => x.TotalAmount);

                _context.Quotations.Add(quotation);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns(quotation.CustomerId);

            return View(quotation);
        }

        // GET: Quotation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var quotation = await _context.Quotations
                .Where(q =>
                    q.QuotationId == id &&
                    q.UserId == CurrentUserId)
                .Include(q => q.QuotationItems)
                .FirstOrDefaultAsync();

            if (quotation == null)
                return NotFound();

            await LoadDropdowns(quotation.CustomerId);

            return View(quotation);
        }

        // POST: Quotation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Quotation quotation)
        {
            if (id != quotation.QuotationId)
                return NotFound();

            var existingQuotation = await _context.Quotations
                .Where(q =>
                    q.QuotationId == id &&
                    q.UserId == CurrentUserId)
                .Include(q => q.QuotationItems)
                .FirstOrDefaultAsync();

            if (existingQuotation == null)
                return NotFound();

            var validCustomer = await _context.Customers
                .AnyAsync(c =>
                    c.CustomerId == quotation.CustomerId &&
                    c.UserId == CurrentUserId);

            if (!validCustomer)
            {
                ModelState.AddModelError(
                    "CustomerId",
                    "Please select a valid customer.");
            }

            var itemIds = quotation.QuotationItems
                .Select(x => x.ItemId)
                .Distinct()
                .ToList();

            var validItemCount = await _context.Items
                .CountAsync(i =>
                    itemIds.Contains(i.ItemId) &&
                    i.UserId == CurrentUserId);

            if (validItemCount != itemIds.Count)
            {
                ModelState.AddModelError(
                    "",
                    "One or more selected items are invalid.");
            }

            if (ModelState.IsValid)
            {
                existingQuotation.QuotationNumber =
                    quotation.QuotationNumber;

                existingQuotation.QuotationDate =
                    quotation.QuotationDate;

                existingQuotation.CustomerId =
                    quotation.CustomerId;

                existingQuotation.Status =
                    quotation.Status;

                existingQuotation.WorkOrderNo =
                    quotation.WorkOrderNo;

                existingQuotation.PlaceOfWork =
                    quotation.PlaceOfWork;

                existingQuotation.QuotationValidity =
                    quotation.QuotationValidity;

                // Keep the saved rate/GST/unit for items already on this quotation.
                // Newly added items use the current rate from the Items master.
                var existingRates = existingQuotation.QuotationItems
                    .GroupBy(x => x.ItemId)
                    .ToDictionary(
                        g => g.Key,
                        g => new Queue<QuotationItem>(
                            g.Select(x => new QuotationItem
                            {
                                ItemId = x.ItemId,
                                ItemNameSnapshot = x.ItemNameSnapshot,
                                Rate = x.Rate,
                                GSTPercentage = x.GSTPercentage,
                                Unit = x.Unit
                            })));

                _context.QuotationItems.RemoveRange(
                    existingQuotation.QuotationItems);

                existingQuotation.QuotationItems.Clear();
                existingQuotation.SubTotal = 0;
                existingQuotation.GSTAmount = 0;
                existingQuotation.GrandTotal = 0;

                var currentItems = await _context.Items
                    .Where(i =>
                        itemIds.Contains(i.ItemId) &&
                        i.UserId == CurrentUserId)
                    .ToDictionaryAsync(i => i.ItemId);

                foreach (var submittedItem in quotation.QuotationItems)
                {
                    if (!currentItems.TryGetValue(
                        submittedItem.ItemId, out var currentItem))
                    {
                        continue;
                    }

                    decimal rate = currentItem.Rate;
                    decimal gstPercentage = currentItem.GSTPercentage;
                    string unit = submittedItem.Unit;
                    string itemNameSnapshot = currentItem.ItemName;

                    if (existingRates.TryGetValue(
                        submittedItem.ItemId, out var savedRates)
                        && savedRates.Count > 0)
                    {
                        var savedItem = savedRates.Dequeue();
                        rate = savedItem.Rate;
                        gstPercentage = savedItem.GSTPercentage;
                        unit = savedItem.Unit;
                        itemNameSnapshot = savedItem.ItemNameSnapshot ?? currentItem.ItemName;
                    }

                    decimal amount = submittedItem.Quantity * rate;
                    decimal gstAmount = amount * gstPercentage / 100;
                    decimal totalAmount = amount + gstAmount;

                    existingQuotation.QuotationItems.Add(new QuotationItem
                    {
                        ItemId = currentItem.ItemId,
                        ItemNameSnapshot = itemNameSnapshot,
                        Quantity = submittedItem.Quantity,
                        Unit = unit,
                        Rate = rate,
                        GSTPercentage = gstPercentage,
                        Amount = amount,
                        GSTAmount = gstAmount,
                        TotalAmount = totalAmount
                    });

                    existingQuotation.SubTotal += amount;
                    existingQuotation.GSTAmount += gstAmount;
                    existingQuotation.GrandTotal += totalAmount;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns(quotation.CustomerId);

            return View(quotation);
        }
        // GET: Quotation/Conditions/5
        [HttpGet]
        public async Task<IActionResult> Conditions(int? id)
        {
            if (id == null)
                return NotFound();

            var quotation = await _context.Quotations
                .FirstOrDefaultAsync(q =>
                    q.QuotationId == id &&
                    q.UserId == CurrentUserId);

            if (quotation == null)
                return NotFound();

            return RedirectToAction(
                "Index",
                "QuotationCondition",
                new { quotationId = id });
        }

        // GET: Quotation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var quotation = await _context.Quotations
                .Where(q =>
                    q.QuotationId == id &&
                    q.UserId == CurrentUserId)
                .Include(q => q.Customer)
                .FirstOrDefaultAsync();

            if (quotation == null)
                return NotFound();

            return View(quotation);
        }

        // POST: Quotation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quotation = await _context.Quotations
                .Where(q =>
                    q.QuotationId == id &&
                    q.UserId == CurrentUserId)
                .Include(q => q.QuotationItems)
                .FirstOrDefaultAsync();

            if (quotation == null)
                return NotFound();

            _context.QuotationItems.RemoveRange(
                quotation.QuotationItems);

            _context.Quotations.Remove(quotation);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Load only current user's customers and items
        private async Task LoadDropdowns(int? selectedCustomerId = null)
        {
            var customers = await _context.Customers
                .Where(c => c.UserId == CurrentUserId)
                .OrderBy(c => c.CustomerName)
                .ThenBy(c => c.ContactPerson)
                .ToListAsync();

            ViewBag.Customers = customers.Select(c => new SelectListItem
            {
                Value = c.CustomerId.ToString(),
                Text = $"{c.CustomerName} — {c.ContactPerson}{(string.IsNullOrWhiteSpace(c.Phone) ? string.Empty : " | " + c.Phone)}",
                Selected = selectedCustomerId == c.CustomerId
            }).ToList();

            ViewBag.Items = await _context.Items
                .Where(i => i.UserId == CurrentUserId)
                .ToListAsync();
        }



        [HttpGet]
        public async Task<IActionResult> ExportExcel(int id)
        {
            var quotation = await _context.Quotations.AsNoTracking()
                .Where(q => q.QuotationId == id && q.UserId == CurrentUserId)
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems).ThenInclude(qi => qi.Item)
                .FirstOrDefaultAsync();
            if (quotation == null) return NotFound();
            var company = await _context.CompanyProfiles.AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == CurrentUserId);

            using var workbook=new XLWorkbook();
            var sheet=workbook.Worksheets.Add("Quotation");
            sheet.Style.Font.FontName="Arial"; sheet.Style.Font.FontSize=10;
            sheet.Column(1).Width=8; sheet.Column(2).Width=34; sheet.Column(3).Width=12;
            sheet.Column(4).Width=12; sheet.Column(5).Width=15; sheet.Column(6).Width=17; sheet.Column(7).Width=16;
            sheet.Range("C1:G1").Merge(); sheet.Cell("C1").Value=(company?.CompanyName ?? "RANGRASIYA").ToUpperInvariant();
            sheet.Range("A1:G1").Style.Font.Bold=true; sheet.Range("A1:G1").Style.Font.FontSize=17;
            sheet.Range("C1:G1").Style.Font.FontColor=XLColor.FromHtml("#1F4E78");
            sheet.Range("C1:G1").Style.Alignment.Horizontal=XLAlignmentHorizontalValues.Center;
            sheet.Range("C2:G2").Merge(); sheet.Cell("C2").Value=$"Registered Office: {company?.Address}";
            sheet.Range("C2:G2").Style.Alignment.Horizontal=XLAlignmentHorizontalValues.Center;
            sheet.Range("C3:G3").Merge(); sheet.Cell("C3").Value=$"Contact No.: {company?.Phone} | E-mail: {company?.Email}";
            sheet.Range("C3:G3").Style.Alignment.Horizontal=XLAlignmentHorizontalValues.Center;
            sheet.Range("C4:G4").Merge(); sheet.Cell("C4").Value=$"GSTIN: {company?.GSTIN} | PAN No.: {company?.PAN} | UDYAM No.: {company?.UDYAMNumber}";
            sheet.Range("C4:G4").Style.Alignment.Horizontal=XLAlignmentHorizontalValues.Center;
            var logo=string.IsNullOrWhiteSpace(company?.LogoPath)?null:Path.Combine(_environment.WebRootPath,company.LogoPath.TrimStart('/').Replace('/',Path.DirectorySeparatorChar));
            TryAddCompanyLogo(sheet,logo,"A1",75,50);
            sheet.Range("A6:D7").Merge(); sheet.Range("A6:D7").Value="QUOTATION";
            sheet.Range("A6:D7").Style.Font.Bold=true; sheet.Range("A6:D7").Style.Font.FontSize=18;
            sheet.Range("A6:D7").Style.Alignment.Horizontal=XLAlignmentHorizontalValues.Center;
            sheet.Range("A6:D7").Style.Fill.BackgroundColor=XLColor.FromHtml("#D9EAF7");
            sheet.Range("E6:G7").Merge(); sheet.Range("E6:G7").Value=$"Quotation No.: {quotation.QuotationNumber}\nDate: {quotation.QuotationDate:dd-MM-yyyy}";
            sheet.Range("E6:G7").Style.Fill.BackgroundColor=XLColor.FromHtml("#1F4E78");
            sheet.Range("E6:G7").Style.Font.FontColor=XLColor.White; sheet.Range("E6:G7").Style.Alignment.WrapText=true;
            StyleSection(sheet,9,1,3,"QUOTATION DETAILS"); StyleSection(sheet,9,4,7,"CUSTOMER DETAILS");
            string[,] left={{"Date",quotation.QuotationDate.ToString("dd-MM-yyyy")},{"Work Order No.",quotation.WorkOrderNo ?? ""},{"Validity",quotation.QuotationValidity ?? ""},{"Place of Work",quotation.PlaceOfWork ?? ""}};
            string[,] right={{"Name",quotation.Customer?.CustomerName ?? ""},{"Contact Person",quotation.Customer?.ContactPerson ?? ""},{"Phone",quotation.Customer?.Phone ?? ""},{"Address",quotation.Customer?.Address ?? ""},{"GSTIN",quotation.Customer?.GSTIN ?? ""}};
            for(int i=0;i<left.GetLength(0);i++){sheet.Cell(10+i,1).Value=left[i,0];sheet.Cell(10+i,2).Value=":";sheet.Range(10+i,3,10+i,3).Value=left[i,1];}
            for(int i=0;i<right.GetLength(0);i++){sheet.Cell(10+i,4).Value=right[i,0];sheet.Cell(10+i,5).Value=":";sheet.Range(10+i,6,10+i,7).Merge();sheet.Cell(10+i,6).Value=right[i,1];}
            sheet.Range("A10:G14").Style.Border.OutsideBorder=XLBorderStyleValues.Thin; sheet.Range("A10:G14").Style.Border.InsideBorder=XLBorderStyleValues.Hair;
            sheet.Range("A10:A13").Style.Font.Bold=true; sheet.Range("D10:D14").Style.Font.Bold=true;
            int hr=16; string[] heads={"Sr.","Description of Work / Items","Qty","Unit","Rate (₹)","GST","Amount (₹)"};
            for(int c=0;c<heads.Length;c++)sheet.Cell(hr,c+1).Value=heads[c]; StyleTableHeader(sheet,hr,7);
            for(int i=0;i<quotation.QuotationItems.Count;i++){var item=quotation.QuotationItems[i];int r=hr+1+i;sheet.Cell(r,1).Value=i+1;sheet.Cell(r,2).Value=item.ItemNameSnapshot ?? item.Item?.ItemName ?? $"Item {item.ItemId}";sheet.Cell(r,3).Value=item.Quantity;sheet.Cell(r,4).Value=item.Unit ?? "Nos";sheet.Cell(r,5).Value=item.Rate;sheet.Cell(r,6).Value=$"{item.GSTPercentage:0.##}%";sheet.Cell(r,7).Value=item.TotalAmount;}
            int last=hr+quotation.QuotationItems.Count; StyleTableBody(sheet,hr+1,last,7);
            int tr=last+2; sheet.Range(tr,1,tr+3,3).Merge(); sheet.Cell(tr,1).Value="AMOUNT IN WORDS"; sheet.Cell(tr,1).Style.Font.Bold=true;
            sheet.Cell(tr+1,1).Value=$"Grand total: ₹{quotation.GrandTotal:N2}";
            sheet.Cell(tr,5).Value="Subtotal";sheet.Cell(tr,7).Value=quotation.SubTotal;
            sheet.Cell(tr+1,5).Value="GST Amount";sheet.Cell(tr+1,7).Value=quotation.GSTAmount;
            sheet.Cell(tr+2,5).Value="GRAND TOTAL";sheet.Cell(tr+2,7).Value=quotation.GrandTotal;
            sheet.Range(tr,5,tr+2,7).Style.Border.OutsideBorder=XLBorderStyleValues.Thin;sheet.Range(tr,tr,tr+2,7).Style.Border.InsideBorder=XLBorderStyleValues.Hair;
            sheet.Range(tr+2,5,tr+2,7).Style.Fill.BackgroundColor=XLColor.FromHtml("#D9EAF7");sheet.Range(tr+2,5,tr+2,7).Style.Font.Bold=true;
            int sign=tr+5; sheet.Range(sign,1,sign,3).Merge();sheet.Cell(sign,1).Value="Prepared By";
            sheet.Range(sign,5,sign,7).Merge();sheet.Cell(sign,5).Value=$"For {company?.CompanyName}";
            sheet.Range(sign+2,1,sign+2,3).Merge();sheet.Cell(sign+2,1).Value="________________________";
            sheet.Range(sign+2,5,sign+2,7).Merge();sheet.Cell(sign+2,5).Value="________________________";
            sheet.Range(sign+3,1,sign+3,3).Merge();sheet.Cell(sign+3,1).Value="Name / Signature";
            sheet.Range(sign+3,5,sign+3,7).Merge();sheet.Cell(sign+3,5).Value="Authorized Signatory";
            sheet.Range(sign,1,sign+3,7).Style.Font.Bold=true;
            int terms=sign+5; StyleSection(sheet,terms,1,7,"TERMS & CONDITIONS");
            sheet.Range(terms+1,1,terms+4,7).Merge();sheet.Cell(terms+1,1).Value=quotation.TermsAndConditions ?? "";
            sheet.Range(terms+1,1,terms+4,7).Style.Alignment.WrapText=true;sheet.Range(terms+1,1,terms+4,7).Style.Border.OutsideBorder=XLBorderStyleValues.Thin;
            sheet.Column(2).Style.Alignment.WrapText=true;sheet.Columns(5,7).Style.NumberFormat.Format="₹#,##0.00";
            sheet.Rows().AdjustToContents(); SetPrintLayout(sheet,terms+4,7);
            using var stream=new MemoryStream();workbook.SaveAs(stream);
            var safe=string.Join("_",(quotation.QuotationNumber ?? $"Quotation-{id}").Split(Path.GetInvalidFileNameChars()));
            return File(stream.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"{safe}.xlsx");
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
