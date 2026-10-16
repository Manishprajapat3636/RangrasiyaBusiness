using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RangrasiyaBusiness.Data;
using System.Security.Claims;

namespace RangrasiyaBusiness.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Invoices()
        {
            var rows = await _context.Invoices
                .AsNoTracking()
                .Where(x => x.UserId == CurrentUserId)
                .Include(x => x.Customer)
                .OrderByDescending(x => x.InvoiceDate)
                .Select(x => new
                {
                    x.InvoiceNumber,
                    x.InvoiceDate,
                    Customer = x.Customer == null ? "" : x.Customer.CustomerName,
                    x.WorkOrderNo,
                    x.PlaceOfWork,
                    x.SubTotal,
                    x.GSTAmount,
                    x.GrandTotal,
                    x.PaymentStatus
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Invoices");
            WriteHeader(sheet, "Invoice Number", "Invoice Date", "Customer",
                "Work Order No", "Place of Work", "Subtotal", "GST Amount",
                "Grand Total", "Payment Status");

            for (int i = 0; i < rows.Count; i++)
            {
                var row = i + 2;
                sheet.Cell(row, 1).Value = rows[i].InvoiceNumber ?? "";
                sheet.Cell(row, 2).Value = rows[i].InvoiceDate;
                sheet.Cell(row, 3).Value = rows[i].Customer;
                sheet.Cell(row, 4).Value = rows[i].WorkOrderNo ?? "";
                sheet.Cell(row, 5).Value = rows[i].PlaceOfWork ?? "";
                sheet.Cell(row, 6).Value = rows[i].SubTotal;
                sheet.Cell(row, 7).Value = rows[i].GSTAmount;
                sheet.Cell(row, 8).Value = rows[i].GrandTotal;
                sheet.Cell(row, 9).Value = rows[i].PaymentStatus ?? "";
            }

            FormatSheet(sheet, rows.Count, 9, dateColumns: new[] { 2 }, moneyColumns: new[] { 6, 7, 8 });
            return Download(workbook, "Invoices");
        }

        [HttpGet]
        public async Task<IActionResult> Quotations()
        {
            var rows = await _context.Quotations
                .AsNoTracking()
                .Where(x => x.UserId == CurrentUserId)
                .Include(x => x.Customer)
                .OrderByDescending(x => x.QuotationDate)
                .Select(x => new
                {
                    x.QuotationNumber,
                    x.QuotationDate,
                    Customer = x.Customer == null ? "" : x.Customer.CustomerName,
                    x.WorkOrderNo,
                    x.PlaceOfWork,
                    x.QuotationValidity,
                    x.Status,
                    x.SubTotal,
                    x.GSTAmount,
                    x.GrandTotal
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Quotations");
            WriteHeader(sheet, "Quotation Number", "Quotation Date", "Customer",
                "Work Order No", "Place of Work", "Validity", "Status",
                "Subtotal", "GST Amount", "Grand Total");

            for (int i = 0; i < rows.Count; i++)
            {
                var row = i + 2;
                sheet.Cell(row, 1).Value = rows[i].QuotationNumber ?? "";
                sheet.Cell(row, 2).Value = rows[i].QuotationDate;
                sheet.Cell(row, 3).Value = rows[i].Customer;
                sheet.Cell(row, 4).Value = rows[i].WorkOrderNo ?? "";
                sheet.Cell(row, 5).Value = rows[i].PlaceOfWork ?? "";
                sheet.Cell(row, 6).Value = rows[i].QuotationValidity ?? "";
                sheet.Cell(row, 7).Value = rows[i].Status ?? "";
                sheet.Cell(row, 8).Value = rows[i].SubTotal;
                sheet.Cell(row, 9).Value = rows[i].GSTAmount;
                sheet.Cell(row, 10).Value = rows[i].GrandTotal;
            }

            FormatSheet(sheet, rows.Count, 10, dateColumns: new[] { 2 }, moneyColumns: new[] { 8, 9, 10 });
            return Download(workbook, "Quotations");
        }

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            var rows = await _context.Customers
                .AsNoTracking()
                .Where(x => x.UserId == CurrentUserId)
                .OrderBy(x => x.CustomerName)
                .Select(x => new
                {
                    x.CustomerId,
                    x.CustomerName,
                    x.ContactPerson,
                    x.Phone,
                    x.GSTIN,
                    x.City,
                    x.Address
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Customers");
            WriteHeader(sheet, "Customer ID", "Customer Name", "Contact Person",
                "Phone", "GSTIN", "City", "Address");

            for (int i = 0; i < rows.Count; i++)
            {
                var row = i + 2;
                sheet.Cell(row, 1).Value = rows[i].CustomerId;
                sheet.Cell(row, 2).Value = rows[i].CustomerName ?? "";
                sheet.Cell(row, 3).Value = rows[i].ContactPerson ?? "";
                sheet.Cell(row, 4).Value = rows[i].Phone ?? "";
                sheet.Cell(row, 5).Value = rows[i].GSTIN ?? "";
                sheet.Cell(row, 6).Value = rows[i].City ?? "";
                sheet.Cell(row, 7).Value = rows[i].Address ?? "";
            }

            FormatSheet(sheet, rows.Count, 7);
            return Download(workbook, "Customers");
        }

        [HttpGet]
        public async Task<IActionResult> Items()
        {
            var rows = await _context.Items
                .AsNoTracking()
                .Where(x => x.UserId == CurrentUserId)
                .OrderBy(x => x.ItemName)
                .Select(x => new
                {
                    x.ItemId,
                    x.ItemName,
                    x.Category,
                    x.Unit,
                    x.Rate,
                    x.GSTPercentage
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Items");
            WriteHeader(sheet, "Item ID", "Item Name", "Category", "Unit", "Rate", "GST Percentage");

            for (int i = 0; i < rows.Count; i++)
            {
                var row = i + 2;
                sheet.Cell(row, 1).Value = rows[i].ItemId;
                sheet.Cell(row, 2).Value = rows[i].ItemName ?? "";
                sheet.Cell(row, 3).Value = rows[i].Category ?? "";
                sheet.Cell(row, 4).Value = rows[i].Unit ?? "";
                sheet.Cell(row, 5).Value = rows[i].Rate;
                sheet.Cell(row, 6).Value = rows[i].GSTPercentage;
            }

            FormatSheet(sheet, rows.Count, 6, moneyColumns: new[] { 5 }, decimalColumns: new[] { 6 });
            return Download(workbook, "Items");
        }

        [HttpGet]
        public async Task<IActionResult> Expenses()
        {
            var rows = await _context.WorkExpenses
                .AsNoTracking()
                .Where(x => x.UserId == CurrentUserId)
                .Include(x => x.Work)
                .OrderByDescending(x => x.ExpenseDate)
                .Select(x => new
                {
                    x.ExpenseDate,
                    x.ExpenseType,
                    x.Description,
                    WorkName = x.Work == null ? "" : x.Work.WorkName,
                    x.Amount,
                    x.Notes
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Expenses");
            WriteHeader(sheet, "Expense Date", "Expense Type", "Description",
                "Work / Project", "Amount", "Notes");

            for (int i = 0; i < rows.Count; i++)
            {
                var row = i + 2;
                sheet.Cell(row, 1).Value = rows[i].ExpenseDate;
                sheet.Cell(row, 2).Value = rows[i].ExpenseType ?? "";
                sheet.Cell(row, 3).Value = rows[i].Description ?? "";
                sheet.Cell(row, 4).Value = rows[i].WorkName ?? "";
                sheet.Cell(row, 5).Value = rows[i].Amount;
                sheet.Cell(row, 6).Value = rows[i].Notes ?? "";
            }

            FormatSheet(sheet, rows.Count, 6, dateColumns: new[] { 1 }, moneyColumns: new[] { 5 });
            return Download(workbook, "Expenses");
        }

        [HttpGet]
        public async Task<IActionResult> WorkLists()
        {
            var rows = await _context.WorkLists
                .AsNoTracking()
                .Where(x => x.UserId == CurrentUserId)
                .Include(x => x.Customer)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.WorkListId,
                    x.WorkOrderNo,
                    x.QuotationNo,
                    x.BillNo,
                    Customer = x.Customer == null ? "" : x.Customer.CustomerName,
                    x.OfficerName,
                    x.OfficerContact,
                    x.WorkType,
                    x.FromDate,
                    x.ToDate,
                    x.Department,
                    x.WorkSiteAddress,
                    x.CreatedAt
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Work Lists");
            WriteHeader(sheet, "Work List ID", "Work Order No", "Quotation No", "Bill No",
                "Customer", "Officer Name", "Officer Contact", "Work Type",
                "From Date", "To Date", "Department", "Work Site Address", "Created At");

            for (int i = 0; i < rows.Count; i++)
            {
                var row = i + 2;
                sheet.Cell(row, 1).Value = rows[i].WorkListId;
                sheet.Cell(row, 2).Value = rows[i].WorkOrderNo ?? "";
                sheet.Cell(row, 3).Value = rows[i].QuotationNo ?? "";
                sheet.Cell(row, 4).Value = rows[i].BillNo ?? "";
                sheet.Cell(row, 5).Value = rows[i].Customer;
                sheet.Cell(row, 6).Value = rows[i].OfficerName ?? "";
                sheet.Cell(row, 7).Value = rows[i].OfficerContact ?? "";
                sheet.Cell(row, 8).Value = rows[i].WorkType ?? "";
                if (rows[i].FromDate.HasValue) sheet.Cell(row, 9).Value = rows[i].FromDate.Value;
                if (rows[i].ToDate.HasValue) sheet.Cell(row, 10).Value = rows[i].ToDate.Value;
                sheet.Cell(row, 11).Value = rows[i].Department ?? "";
                sheet.Cell(row, 12).Value = rows[i].WorkSiteAddress ?? "";
                sheet.Cell(row, 13).Value = rows[i].CreatedAt;
            }

            FormatSheet(sheet, rows.Count, 13, dateColumns: new[] { 9, 10, 13 });
            return Download(workbook, "WorkLists");
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var invoices = await _context.Invoices
                .AsNoTracking()
                .Where(x => x.UserId == CurrentUserId)
                .Select(x => new { x.InvoiceDate, x.SubTotal, x.GSTAmount, x.GrandTotal })
                .ToListAsync();

            var quotations = await _context.Quotations
                .AsNoTracking()
                .Where(x => x.UserId == CurrentUserId)
                .Select(x => new { x.QuotationDate, x.SubTotal, x.GSTAmount, x.GrandTotal })
                .ToListAsync();

            var expenses = await _context.WorkExpenses
                .AsNoTracking()
                .Where(x => x.UserId == CurrentUserId)
                .Select(x => new { x.ExpenseDate, x.Amount })
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var summary = workbook.Worksheets.Add("Summary");
            WriteHeader(summary, "Metric", "Count", "Amount");
            var summaryRows = new[]
            {
                new { Metric = "Invoices", Count = invoices.Count, Amount = invoices.Sum(x => x.GrandTotal) },
                new { Metric = "Invoice Subtotal", Count = invoices.Count, Amount = invoices.Sum(x => x.SubTotal) },
                new { Metric = "Invoice GST", Count = invoices.Count, Amount = invoices.Sum(x => x.GSTAmount) },
                new { Metric = "Quotations", Count = quotations.Count, Amount = quotations.Sum(x => x.GrandTotal) },
                new { Metric = "Quotation Subtotal", Count = quotations.Count, Amount = quotations.Sum(x => x.SubTotal) },
                new { Metric = "Quotation GST", Count = quotations.Count, Amount = quotations.Sum(x => x.GSTAmount) },
                new { Metric = "Expenses", Count = expenses.Count, Amount = expenses.Sum(x => x.Amount) },
                new { Metric = "Invoice Grand Total - Expenses (basic comparison)", Count = 0, Amount = invoices.Sum(x => x.GrandTotal) - expenses.Sum(x => x.Amount) }
            };

            for (int i = 0; i < summaryRows.Length; i++)
            {
                summary.Cell(i + 2, 1).Value = summaryRows[i].Metric;
                summary.Cell(i + 2, 2).Value = summaryRows[i].Count;
                summary.Cell(i + 2, 3).Value = summaryRows[i].Amount;
            }
            FormatSheet(summary, summaryRows.Length, 3, moneyColumns: new[] { 3 });

            var monthly = workbook.Worksheets.Add("Monthly Invoices");
            WriteHeader(monthly, "Month", "Invoice Count", "Subtotal", "GST", "Grand Total");
            var monthlyRows = invoices
                .GroupBy(x => new DateTime(x.InvoiceDate.Year, x.InvoiceDate.Month, 1))
                .OrderBy(x => x.Key)
                .ToList();

            for (int i = 0; i < monthlyRows.Count; i++)
            {
                monthly.Cell(i + 2, 1).Value = monthlyRows[i].Key;
                monthly.Cell(i + 2, 2).Value = monthlyRows[i].Count();
                monthly.Cell(i + 2, 3).Value = monthlyRows[i].Sum(x => x.SubTotal);
                monthly.Cell(i + 2, 4).Value = monthlyRows[i].Sum(x => x.GSTAmount);
                monthly.Cell(i + 2, 5).Value = monthlyRows[i].Sum(x => x.GrandTotal);
            }
            FormatSheet(monthly, monthlyRows.Count, 5, dateColumns: new[] { 1 }, moneyColumns: new[] { 3, 4, 5 });

            var monthlyExpenses = workbook.Worksheets.Add("Monthly Expenses");
            WriteHeader(monthlyExpenses, "Month", "Expense Count", "Total Expenses");
            var monthlyExpenseRows = expenses
                .GroupBy(x => new DateTime(x.ExpenseDate.Year, x.ExpenseDate.Month, 1))
                .OrderBy(x => x.Key)
                .ToList();

            for (int i = 0; i < monthlyExpenseRows.Count; i++)
            {
                monthlyExpenses.Cell(i + 2, 1).Value = monthlyExpenseRows[i].Key;
                monthlyExpenses.Cell(i + 2, 2).Value = monthlyExpenseRows[i].Count();
                monthlyExpenses.Cell(i + 2, 3).Value = monthlyExpenseRows[i].Sum(x => x.Amount);
            }
            FormatSheet(monthlyExpenses, monthlyExpenseRows.Count, 3, dateColumns: new[] { 1 }, moneyColumns: new[] { 3 });

            return Download(workbook, "Reports");
        }

        private static void WriteHeader(IXLWorksheet sheet, params string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
                sheet.Cell(1, i + 1).Value = headers[i];

            var header = sheet.Range(1, 1, 1, headers.Length);
            header.Style.Font.Bold = true;
            header.Style.Font.FontColor = XLColor.White;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E78");
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            sheet.SheetView.FreezeRows(1);
        }

        private static void FormatSheet(
            IXLWorksheet sheet,
            int dataCount,
            int columnCount,
            int[]? dateColumns = null,
            int[]? moneyColumns = null,
            int[]? decimalColumns = null)
        {
            sheet.Columns(1, columnCount).AdjustToContents();
            sheet.Row(1).Height = 24;

            if (dataCount > 0)
            {
                var used = sheet.Range(1, 1, dataCount + 1, columnCount);
                used.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                used.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
                used.SetAutoFilter();

                foreach (var column in dateColumns ?? Array.Empty<int>())
                    sheet.Column(column).Style.DateFormat.Format = "dd-MM-yyyy";

                foreach (var column in moneyColumns ?? Array.Empty<int>())
                    sheet.Column(column).Style.NumberFormat.Format = "#,##0.00";

                foreach (var column in decimalColumns ?? Array.Empty<int>())
                    sheet.Column(column).Style.NumberFormat.Format = "0.##";
            }
        }

        private FileContentResult Download(XLWorkbook workbook, string name)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{name}-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");
        }
    }
}
