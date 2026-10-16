using ClosedXML.Excel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using RangrasiyaBusiness.Services;
using RangrasiyaBusiness.ViewModels;
using System.Security.Claims;

namespace RangrasiyaBusiness.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly InvoicePdfService _pdfService;
        private readonly IWebHostEnvironment _environment;

        public InvoiceController(
            ApplicationDbContext context,
            InvoicePdfService pdfService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _pdfService = pdfService;
            _environment = environment;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private async Task LoadDropdowns()
        {
            ViewBag.Customers = await _context.Customers
                .Where(c => c.UserId == CurrentUserId)
                .ToListAsync();

            ViewBag.Items = await _context.Items
                .Where(i => i.UserId == CurrentUserId)
                .ToListAsync();
        }

        private async Task<bool> ValidCustomer(int customerId)
        {
            return await _context.Customers.AnyAsync(c =>
                c.CustomerId == customerId &&
                c.UserId == CurrentUserId);
        }

        private async Task<bool> ValidItems(
            List<InvoiceItemViewModel> items)
        {
            var itemIds = items.Select(i => i.ItemId).Distinct().ToList();

            var validCount = await _context.Items
                .CountAsync(i =>
                    itemIds.Contains(i.ItemId) &&
                    i.UserId == CurrentUserId);

            return validCount == itemIds.Count;
        }

        // GET: Invoice/Index
        public async Task<IActionResult> Index(string sortBy = "date", string order = "desc")
        {
            // Keep the query restricted to the currently signed-in user's invoices.
            var query = _context.Invoices
                .Where(i => i.UserId == CurrentUserId)
                .Include(i => i.Customer)
                .AsQueryable();

            sortBy = (sortBy ?? "date").Trim().ToLowerInvariant();
            order = (order ?? "desc").Trim().ToLowerInvariant() == "asc" ? "asc" : "desc";

            query = sortBy switch
            {
                "number" => order == "asc"
                    ? query.OrderBy(i => i.InvoiceNumber)
                    : query.OrderByDescending(i => i.InvoiceNumber),
                "amount" => order == "asc"
                    ? query.OrderBy(i => i.GrandTotal)
                    : query.OrderByDescending(i => i.GrandTotal),
                "customer" => order == "asc"
                    ? query.OrderBy(i => i.Customer!.CustomerName)
                    : query.OrderByDescending(i => i.Customer!.CustomerName),
                "status" => order == "asc"
                    ? query.OrderBy(i => i.PaymentStatus)
                    : query.OrderByDescending(i => i.PaymentStatus),
                _ => order == "asc"
                    ? query.OrderBy(i => i.InvoiceDate).ThenBy(i => i.InvoiceId)
                    : query.OrderByDescending(i => i.InvoiceDate).ThenByDescending(i => i.InvoiceId)
            };

            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = order;

            var invoices = await query.ToListAsync();
            return View(invoices);
        }

        // GET: Invoice/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            var model = new InvoiceCreateViewModel
            {
                InvoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}",
                InvoiceDate = DateTime.Now,
                PaymentStatus = "Pending"
            };

            return View(model);
        }

        // POST: Invoice/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            InvoiceCreateViewModel model)
        {
            if (!await ValidCustomer(model.CustomerId) ||
                !await ValidItems(model.Items))
            {
                ModelState.AddModelError(
                    "",
                    "Invalid customer or item selected.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            var invoice = new Invoice
            {
                InvoiceNumber = model.InvoiceNumber,
                InvoiceDate = model.InvoiceDate,
                CustomerId = model.CustomerId,
                PaymentStatus = model.PaymentStatus,
                WorkOrderNo = model.WorkOrderNo,
                PlaceOfWork = model.PlaceOfWork,
                SACHSNCode = model.SACHSNCode,
                UserId = CurrentUserId
            };

            var itemIds = model.Items
                .Select(i => i.ItemId)
                .Distinct()
                .ToList();

            var items = await _context.Items
                .Where(i =>
                    itemIds.Contains(i.ItemId) &&
                    i.UserId == CurrentUserId)
                .ToDictionaryAsync(i => i.ItemId);

            foreach (var itemModel in model.Items)
            {
                if (!items.TryGetValue(itemModel.ItemId, out var item))
                    continue;

                decimal amount =
                    itemModel.Quantity * item.Rate;

                decimal gstAmount =
                    amount * item.GSTPercentage / 100;

                decimal totalAmount =
                    amount + gstAmount;

                invoice.InvoiceItems.Add(new InvoiceItem
                {
                    ItemId = item.ItemId,
                    ItemNameSnapshot = item.ItemName,
                    Quantity = itemModel.Quantity,
                    Rate = item.Rate,
                    GSTPercentage = item.GSTPercentage,
                    Amount = amount,
                    GSTAmount = gstAmount,
                    TotalAmount = totalAmount
                });

                invoice.SubTotal += amount;
                invoice.GSTAmount += gstAmount;
                invoice.GrandTotal += totalAmount;
            }

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Invoice/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var invoice = await _context.Invoices
                .Where(i =>
                    i.InvoiceId == id &&
                    i.UserId == CurrentUserId)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync();

            if (invoice == null)
                return NotFound();

            await LoadDropdowns();

            var model = new InvoiceCreateViewModel
            {
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                CustomerId = invoice.CustomerId,
                PaymentStatus = invoice.PaymentStatus,
                WorkOrderNo = invoice.WorkOrderNo,
                PlaceOfWork = invoice.PlaceOfWork,
                SACHSNCode = invoice.SACHSNCode,

                Items = invoice.InvoiceItems.Select(item =>
                    new InvoiceItemViewModel
                    {
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                        Rate = item.Rate,
                        GSTPercentage = item.GSTPercentage
                    }).ToList()
            };

            ViewData["InvoiceId"] = invoice.InvoiceId;

            return View(model);
        }

        // POST: Invoice/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            InvoiceCreateViewModel model)
        {
            var invoice = await _context.Invoices
                .Where(i =>
                    i.InvoiceId == id &&
                    i.UserId == CurrentUserId)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync();

            if (invoice == null)
                return NotFound();

            if (!await ValidCustomer(model.CustomerId) ||
                !await ValidItems(model.Items))
            {
                ModelState.AddModelError(
                    "",
                    "Invalid customer or item selected.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                ViewData["InvoiceId"] = id;
                return View(model);
            }

            invoice.InvoiceNumber = model.InvoiceNumber;
            invoice.InvoiceDate = model.InvoiceDate;
            invoice.CustomerId = model.CustomerId;
            invoice.PaymentStatus = model.PaymentStatus;
            invoice.WorkOrderNo = model.WorkOrderNo;
            invoice.PlaceOfWork = model.PlaceOfWork;
            invoice.SACHSNCode = model.SACHSNCode;

            // Save the rates currently stored on this invoice before rebuilding its items.
            // Existing items keep their saved rate/GST; newly added items use current master rates.
            var existingRates = invoice.InvoiceItems
                .GroupBy(x => x.ItemId)
                .ToDictionary(
                    g => g.Key,
                    g => new Queue<InvoiceItem>(
                        g.Select(x => new InvoiceItem
                        {
                            ItemId = x.ItemId,
                            ItemNameSnapshot = x.ItemNameSnapshot,
                            Rate = x.Rate,
                            GSTPercentage = x.GSTPercentage
                        })));

            _context.InvoiceItems.RemoveRange(invoice.InvoiceItems);
            invoice.InvoiceItems.Clear();

            invoice.SubTotal = 0;
            invoice.GSTAmount = 0;
            invoice.GrandTotal = 0;

            var itemIds = model.Items
                .Select(i => i.ItemId)
                .Distinct()
                .ToList();

            var items = await _context.Items
                .Where(i =>
                    itemIds.Contains(i.ItemId) &&
                    i.UserId == CurrentUserId)
                .ToDictionaryAsync(i => i.ItemId);

            foreach (var itemModel in model.Items)
            {
                if (!items.TryGetValue(itemModel.ItemId, out var item))
                    continue;

                decimal rate = item.Rate;
                decimal gstPercentage = item.GSTPercentage;
                string itemNameSnapshot = item.ItemName;

                if (existingRates.TryGetValue(itemModel.ItemId, out var savedRates)
                    && savedRates.Count > 0)
                {
                    var savedItem = savedRates.Dequeue();
                    rate = savedItem.Rate;
                    gstPercentage = savedItem.GSTPercentage;
                    itemNameSnapshot = savedItem.ItemNameSnapshot ?? item.ItemName;
                }

                decimal amount = itemModel.Quantity * rate;
                decimal gstAmount = amount * gstPercentage / 100;
                decimal totalAmount = amount + gstAmount;

                invoice.InvoiceItems.Add(new InvoiceItem
                {
                    ItemId = item.ItemId,
                    ItemNameSnapshot = itemNameSnapshot,
                    Quantity = itemModel.Quantity,
                    Rate = rate,
                    GSTPercentage = gstPercentage,
                    Amount = amount,
                    GSTAmount = gstAmount,
                    TotalAmount = totalAmount
                });

                invoice.SubTotal += amount;
                invoice.GSTAmount += gstAmount;
                invoice.GrandTotal += totalAmount;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Invoice/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Where(i =>
                    i.InvoiceId == id &&
                    i.UserId == CurrentUserId)
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Item)
                .FirstOrDefaultAsync();

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        // GET: Invoice/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Invoices
                .Where(i =>
                    i.InvoiceId == id &&
                    i.UserId == CurrentUserId)
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync();

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        // POST: Invoice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Invoices
                .Where(i =>
                    i.InvoiceId == id &&
                    i.UserId == CurrentUserId)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync();

            if (invoice == null)
                return NotFound();

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Invoice/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var invoice = await _context.Invoices
                .Where(i =>
                    i.InvoiceId == id &&
                    i.UserId == CurrentUserId)
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.Item)
                .FirstOrDefaultAsync();

            if (invoice == null)
                return NotFound();

            var pdfBytes = _pdfService.GeneratePdf(invoice);

            return File(
                pdfBytes,
                "application/pdf",
                $"Invoice-{invoice.InvoiceNumber}.pdf");
        }


        [HttpGet]
        public async Task<IActionResult> ExportExcel(int id)
        {
            var invoice = await _context.Invoices.AsNoTracking()
                .Where(i => i.InvoiceId == id && i.UserId == CurrentUserId)
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems).ThenInclude(ii => ii.Item)
                .FirstOrDefaultAsync();
            if (invoice == null) return NotFound();

            var company = await _context.CompanyProfiles.AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == CurrentUserId);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Tax Invoice");
            sheet.Style.Font.FontName = "Arial";
            sheet.Style.Font.FontSize = 10;
            sheet.Column(1).Width = 8; sheet.Column(2).Width = 34;
            sheet.Column(3).Width = 13; sheet.Column(4).Width = 12;
            sheet.Column(5).Width = 15; sheet.Column(6).Width = 17;

            sheet.Range("C1:F1").Merge();
            sheet.Range("C1:F1").Value = (company?.CompanyName ?? "RANGRASIYA").ToUpperInvariant();
            sheet.Range("C1:F1").Style.Font.Bold = true;
            sheet.Range("C1:F1").Style.Font.FontSize = 18;
            sheet.Range("C1:F1").Style.Font.FontColor = XLColor.FromHtml("#1F4E78");
            sheet.Range("C1:F1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range("C2:F2").Merge();
            sheet.Cell("C2").Value = company?.Address ?? "";
            sheet.Range("C2:F2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range("C3:F3").Merge();
            sheet.Cell("C3").Value = $"Contact No.: {company?.Phone} | E-mail: {company?.Email}";
            sheet.Range("C3:F3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range("C4:F4").Merge();
            sheet.Cell("C4").Value = $"GSTIN: {company?.GSTIN} | PAN: {company?.PAN} | UDYAM: {company?.UDYAMNumber}";
            sheet.Range("C4:F4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range("A1:F4").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            sheet.Range("A1:F4").Style.Border.BottomBorderColor = XLColor.FromHtml("#1F4E78");

            var logoPath = string.IsNullOrWhiteSpace(company?.LogoPath) ? null :
                Path.Combine(_environment.WebRootPath, company.LogoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            TryAddCompanyLogo(sheet, logoPath, "A1", 80, 55);

            sheet.Range("A6:C7").Merge();
            sheet.Range("A6:C7").Value = "TAX INVOICE";
            sheet.Range("A6:C7").Style.Font.Bold = true;
            sheet.Range("A6:C7").Style.Font.FontSize = 18;
            sheet.Range("A6:C7").Style.Font.FontColor = XLColor.FromHtml("#1F3557");
            sheet.Range("A6:C7").Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");
            sheet.Range("A6:C7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range("D6:F7").Merge();
            sheet.Range("D6:F7").Value = $"Invoice No.: {invoice.InvoiceNumber}\nDate of Issue: {invoice.InvoiceDate:dd-MM-yyyy}\nPlace of Supply: {company?.State ?? ""}";
            sheet.Range("D6:F7").Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E78");
            sheet.Range("D6:F7").Style.Font.FontColor = XLColor.White;
            sheet.Range("D6:F7").Style.Alignment.WrapText = true;
            sheet.Range("D6:F7").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            StyleSection(sheet, 9, 1, 3, "BILL TO (CUSTOMER DETAILS)");
            StyleSection(sheet, 9, 4, 6, "WORK DETAILS");
            string customerPhone = invoice.Customer?.Phone ?? "";
            string customerEmail = "";
            int sep = customerPhone.IndexOf('/');
            if (sep >= 0) { customerEmail = customerPhone[(sep + 1)..].Trim(); customerPhone = customerPhone[..sep].Trim(); }
            string[,] left = {
                {"Name", invoice.Customer?.CustomerName ?? ""},
                {"Contact Person", invoice.Customer?.ContactPerson ?? ""},
                {"Address", invoice.Customer?.Address ?? ""},
                {"State", invoice.Customer?.City ?? ""},
                {"Phone", customerPhone},
                {"Email", string.IsNullOrWhiteSpace(customerEmail) ? "-" : customerEmail},
                {"GSTIN", invoice.Customer?.GSTIN ?? ""}
            };
            string[,] right = {
                {"Work Order/Date No.", invoice.WorkOrderNo ?? ""},
                {"SAC Code", invoice.SACHSNCode ?? ""},
                {"HSN Code", invoice.SACHSNCode ?? ""},
                {"Place of Work", invoice.PlaceOfWork ?? ""}
            };
            for(int i=0;i<left.GetLength(0);i++){ sheet.Cell(10+i,1).Value=left[i,0]; sheet.Cell(10+i,2).Value=":"; sheet.Range(10+i,3,10+i,3).Value=left[i,1]; }
            for(int i=0;i<right.GetLength(0);i++){ sheet.Cell(10+i,4).Value=right[i,0]; sheet.Cell(10+i,5).Value=":"; sheet.Cell(10+i,6).Value=right[i,1]; }
            sheet.Range(10,1,16,3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Range(10,1,16,3).Style.Border.InsideBorder = XLBorderStyleValues.Hair;
            sheet.Range(10,4,13,6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            sheet.Range(10,4,13,6).Style.Border.InsideBorder = XLBorderStyleValues.Hair;
            sheet.Range("A10:A16").Style.Font.Bold = true; sheet.Range("D10:D13").Style.Font.Bold = true;

            int headerRow=18;
            string[] headers={"Sr. No.","Description of Work / Items","Unit","Quantity","Rate (₹)","Amount (₹)"};
            for(int c=0;c<headers.Length;c++) sheet.Cell(headerRow,c+1).Value=headers[c];
            StyleTableHeader(sheet,headerRow,6);
            for(int i=0;i<invoice.InvoiceItems.Count;i++){
                var item=invoice.InvoiceItems[i]; int row=headerRow+1+i;
                sheet.Cell(row,1).Value=i+1; sheet.Cell(row,2).Value=item.ItemNameSnapshot ?? item.Item?.ItemName ?? $"Item {item.ItemId}";
                sheet.Cell(row,3).Value=item.Item?.Unit ?? ""; sheet.Cell(row,4).Value=item.Quantity;
                sheet.Cell(row,5).Value=item.Rate; sheet.Cell(row,6).Value=item.TotalAmount;
            }
            int lastItemRow=headerRow+invoice.InvoiceItems.Count;
            StyleTableBody(sheet,headerRow+1,lastItemRow,6);
            int totalsRow=lastItemRow+2;
            sheet.Range(totalsRow,1,totalsRow+4,4).Merge();
            sheet.Cell(totalsRow,1).Value="Amount in Words";
            sheet.Cell(totalsRow,1).Style.Font.Bold=true;
            sheet.Cell(totalsRow+1,1).Value="(Amount in words as shown on the PDF)";
            string[] labels={"Amount Before Tax","Add : CGST @ 9%","Add : SGST @ 9%","Total Tax Amount","TOTAL AMOUNT AFTER TAX"};
            decimal[] amounts={invoice.SubTotal,invoice.GSTAmount/2m,invoice.GSTAmount/2m,invoice.GSTAmount,invoice.GrandTotal};
            for(int i=0;i<labels.Length;i++){sheet.Range(totalsRow+i,5,totalsRow+i,5).Merge();sheet.Cell(totalsRow+i,5).Value=labels[i];sheet.Cell(totalsRow+i,6).Value=amounts[i];}
            sheet.Range(totalsRow,5,totalsRow+4,6).Style.Border.OutsideBorder=XLBorderStyleValues.Thin;
            sheet.Range(totalsRow,5,totalsRow+4,6).Style.Border.InsideBorder=XLBorderStyleValues.Hair;
            sheet.Range(totalsRow+4,5,totalsRow+4,6).Style.Fill.BackgroundColor=XLColor.FromHtml("#D9EAF7");
            sheet.Range(totalsRow+4,5,totalsRow+4,6).Style.Font.Bold=true;
            int bottom=totalsRow+6;
            StyleSection(sheet,bottom,1,3,"BANK DETAILS");
            StyleSection(sheet,bottom,4,6,"TERMS & CONDITIONS");
            sheet.Range(bottom+1,1,bottom+4,3).Merge();
            sheet.Cell(bottom+1,1).Value=$"Bank Name: {company?.BankName}\nAccount No.: {company?.AccountNumber}\nIFSC Code: {company?.IFSCCode}\nBranch: {company?.Branch}";
            sheet.Range(bottom+1,1,bottom+4,3).Style.Alignment.WrapText=true;
            sheet.Range(bottom+1,4,bottom+4,6).Merge();
            sheet.Cell(bottom+1,4).Value=company?.TermsAndConditions ?? "This is a computer-generated invoice.";
            sheet.Range(bottom+1,4,bottom+4,6).Style.Alignment.WrapText=true;
            sheet.Range(bottom+1,1,bottom+4,6).Style.Border.OutsideBorder=XLBorderStyleValues.Thin;
            sheet.Range(bottom+6,4,bottom+6,6).Merge();
            sheet.Cell(bottom+6,4).Value=$"For {company?.CompanyName}";
            sheet.Cell(bottom+7,4).Value="________________________";
            sheet.Cell(bottom+8,4).Value="Authorized Signatory";
            sheet.Range(bottom+6,4,bottom+8,6).Style.Alignment.Horizontal=XLAlignmentHorizontalValues.Right;
            sheet.Column(2).Style.Alignment.WrapText=true;
            sheet.Column(6).Style.NumberFormat.Format="₹#,##0.00";
            sheet.Column(5).Style.NumberFormat.Format="₹#,##0.00";
            sheet.Range(headerRow+1,4,lastItemRow,6).Style.NumberFormat.Format="#,##0.00";
            sheet.Rows().AdjustToContents();
            sheet.Row(6).Height=25; sheet.Row(7).Height=25;
            SetPrintLayout(sheet,bottom+9,6);
            using var stream=new MemoryStream(); workbook.SaveAs(stream);
            var safe=string.Join("_",(invoice.InvoiceNumber ?? $"Invoice-{id}").Split(Path.GetInvalidFileNameChars()));
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
