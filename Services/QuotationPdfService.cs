using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RangrasiyaBusiness.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using RangrasiyaBusiness.Data;

namespace RangrasiyaBusiness.Services
{
    public class QuotationPdfService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public QuotationPdfService(
            IWebHostEnvironment environment,
            ApplicationDbContext context)
        {
            _environment = environment;
            _context = context;
        }

        public byte[] GeneratePdf(Quotation quotation)
        {
            var company = _context.CompanyProfiles
                .FirstOrDefault(c => c.UserId == quotation.UserId);

            string companyName = company?.CompanyName ?? "RANGRASIYA";
            string address = company?.Address ?? "";
            string phone = company?.Phone ?? "";
            string email = company?.Email ?? "";
            string gstin = company?.GSTIN ?? "";
            string panNumber = company?.PAN ?? "";
            string udyamNumber = company?.UDYAMNumber ?? "";
            string businessType = company?.BusinessType ?? "";

            string logoPath = "";

            if (!string.IsNullOrWhiteSpace(company?.LogoPath))
            {
                logoPath = Path.Combine(
                    _environment.WebRootPath,
                    company.LogoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            }

            var (companyNameLine1, companyNameLine2) = SplitCompanyName(companyName);
            var (addressLine1, addressLine2) = SplitAddress(address);

            string servicesLine = string.IsNullOrWhiteSpace(businessType)
                ? ""
                : businessType;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(18);
                    page.MarginVertical(18);

                    // ---------------------------------------------------------
                    // HEADER - professional invoice-style header
                    // ---------------------------------------------------------
                    page.Header().Column(header =>
                    {
                        header.Item()
                            .Border(1)
                            .BorderColor("1F4E78")
                            .Padding(6)
                            .Row(row =>
                            {
                                if (!string.IsNullOrWhiteSpace(logoPath) &&
                                    File.Exists(logoPath))
                                {
                                    row.ConstantItem(175)
                                        .Height(88)
                                        .AlignMiddle()
                                        .AlignCenter()
                                        .Image(logoPath)
                                        .FitArea();
                                }
                                else
                                {
                                    row.ConstantItem(20);
                                }

                                row.ConstantItem(8);

                                row.RelativeItem()
                                    .AlignMiddle()
                                    .Column(info =>
                                    {
                                        info.Item()
                                            .AlignCenter()
                                            .Text(companyNameLine1)
                                            .FontSize(17)
                                            .Bold()
                                            .FontColor("1F4E78");

                                        if (!string.IsNullOrWhiteSpace(companyNameLine2))
                                        {
                                            info.Item()
                                                .PaddingTop(1)
                                                .AlignCenter()
                                                .Text(companyNameLine2)
                                                .FontSize(17)
                                                .Bold()
                                                .FontColor("1F4E78");
                                        }

                                        if (!string.IsNullOrWhiteSpace(addressLine1))
                                        {
                                            info.Item()
                                                .PaddingTop(4)
                                                .AlignCenter()
                                                .Text($"Registered Office : {addressLine1}")
                                                .FontSize(8.2f);
                                        }

                                        if (!string.IsNullOrWhiteSpace(addressLine2))
                                        {
                                            info.Item()
                                                .AlignCenter()
                                                .Text(addressLine2)
                                                .FontSize(8.2f);
                                        }

                                        info.Item()
                                            .PaddingTop(2)
                                            .AlignCenter()
                                            .Text($"Contact No.: {phone}  |  E-mail: {email}")
                                            .FontSize(8.2f);

                                        info.Item()
                                            .PaddingTop(2)
                                            .AlignCenter()
                                            .Text(
                                                $"GSTIN : {gstin}  |  PAN No. : {panNumber}  |  UDYAM No. : {udyamNumber}")
                                            .FontSize(8.2f);
                                    });
                            });

                        if (!string.IsNullOrWhiteSpace(servicesLine))
                        {
                            header.Item()
                                .PaddingTop(3)
                                .BorderBottom(1)
                                .BorderColor("1F4E78")
                                .AlignCenter()
                                .Text(servicesLine)
                                .FontSize(7.4f)
                                .Bold();
                        }
                    });

                    // ---------------------------------------------------------
                    // FOOTER
                    // ---------------------------------------------------------
                    page.Footer()
                        .PaddingTop(5)
                        .BorderTop(0.5f)
                        .BorderColor("AAB7C4")
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Quotation  •  ")
                                .FontSize(7)
                                .FontColor("666666");

                            text.Span($"Quotation No. {quotation.QuotationNumber}  •  ")
                                .FontSize(7)
                                .FontColor("666666");

                            text.Span("Page ")
                                .FontSize(7)
                                .FontColor("666666");

                            text.CurrentPageNumber();

                            text.Span(" of ")
                                .FontSize(7)
                                .FontColor("666666");

                            text.TotalPages();
                        });

                    // ---------------------------------------------------------
                    // CONTENT
                    // ---------------------------------------------------------
                    page.Content().Column(column =>
                    {
                        // TITLE BAND
                        column.Item()
                            .PaddingTop(6)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Background("D9EAF7")
                                    .PaddingVertical(7)
                                    .AlignCenter()
                                    .Text("QUOTATION")
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor("1F3557");

                                row.ConstantItem(205)
                                    .Background("1F4E78")
                                    .PaddingHorizontal(10)
                                    .PaddingVertical(6)
                                    .Column(info =>
                                    {
                                        info.Item()
                                            .Text($"Quotation No. : {quotation.QuotationNumber}")
                                            .FontColor("FFFFFF")
                                            .FontSize(9.5f)
                                            .Bold();

                                        info.Item()
                                            .PaddingTop(2)
                                            .Text($"Date : {quotation.QuotationDate:dd-MM-yyyy}")
                                            .FontColor("FFFFFF")
                                            .FontSize(9.5f);
                                    });
                            });

                        // QUOTATION + CUSTOMER DETAILS
                        column.Item()
                            .PaddingTop(7)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell()
                                    .Border(1)
                                    .BorderColor("AAB7C4")
                                    .Padding(8)
                                    .Column(left =>
                                    {
                                        left.Item()
                                            .Background("1F4E78")
                                            .Padding(5)
                                            .Text("QUOTATION DETAILS")
                                            .FontColor("FFFFFF")
                                            .Bold()
                                            .FontSize(9);

                                        AddDetailRow(left, "Date", quotation.QuotationDate.ToString("dd-MM-yyyy"), true);

                                        if (!string.IsNullOrWhiteSpace(quotation.WorkOrderNo))
                                            AddDetailRow(left, "Work Order No.", quotation.WorkOrderNo);

                                        if (!string.IsNullOrWhiteSpace(quotation.QuotationValidity))
                                            AddDetailRow(left, "Validity", quotation.QuotationValidity);

                                        //if (!string.IsNullOrWhiteSpace(quotation.Status))
                                        //    AddDetailRow(left, "Status", quotation.Status);

                                        if (!string.IsNullOrWhiteSpace(quotation.PlaceOfWork))
                                            AddDetailRow(left, "Place of Work", quotation.PlaceOfWork);
                                    });

                                table.Cell()
                                    .Border(1)
                                    .BorderColor("AAB7C4")
                                    .Padding(8)
                                    .Column(right =>
                                    {
                                        right.Item()
                                            .Background("1F4E78")
                                            .Padding(5)
                                            .Text("CUSTOMER DETAILS")
                                            .FontColor("FFFFFF")
                                            .Bold()
                                            .FontSize(9);

                                        AddDetailRow(
                                            right,
                                            "Name",
                                            quotation.Customer?.CustomerName ?? "-",
                                            true);

                                        if (!string.IsNullOrWhiteSpace(quotation.Customer?.ContactPerson))
                                            AddDetailRow(right, "Contact Person", quotation.Customer.ContactPerson);

                                        if (!string.IsNullOrWhiteSpace(quotation.Customer?.Phone))
                                            AddDetailRow(right, "Phone", quotation.Customer.Phone);

                                        if (!string.IsNullOrWhiteSpace(quotation.Customer?.Address))
                                            AddDetailRow(right, "Address", quotation.Customer.Address);

                                        if (!string.IsNullOrWhiteSpace(quotation.Customer?.GSTIN))
                                            AddDetailRow(right, "GSTIN", quotation.Customer.GSTIN);
                                    });
                            });

                        // ITEMS TABLE
                        column.Item()
                            .PaddingTop(10)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(38);
                                    columns.RelativeColumn(3.2f);
                                    columns.RelativeColumn(0.8f);
                                    columns.RelativeColumn(0.9f);
                                    columns.RelativeColumn(1.05f);
                                    columns.RelativeColumn(0.9f);
                                    columns.RelativeColumn(1.35f);
                                });

                                table.Header(header =>
                                {
                                    string[] headers =
                                    {
                                        "Sr.",
                                        "Description of Work / Items",
                                        "Qty",
                                        "Unit",
                                        "Rate (₹)",
                                        "GST",
                                        "Amount (₹)"
                                    };

                                    foreach (var title in headers)
                                    {
                                        header.Cell()
                                            .Background("D9EAF7")
                                            .Border(1)
                                            .BorderColor("AAB7C4")
                                            .Padding(4)
                                            .AlignCenter()
                                            .Text(title)
                                            .FontColor("1F3557")
                                            .Bold()
                                            .FontSize(8);
                                    }
                                });

                                int serialNumber = 1;

                                foreach (var item in quotation.QuotationItems)
                                {
                                    table.Cell()
                                        .Border(1)
                                        .BorderColor("D0D7DE")
                                        .Padding(4)
                                        .AlignCenter()
                                        .Text(serialNumber++.ToString())
                                        .FontSize(8);

                                    table.Cell()
                                        .Border(1)
                                        .BorderColor("D0D7DE")
                                        .Padding(4)
                                        .Text(item.ItemNameSnapshot ?? item.Item?.ItemName ?? "Item")
                                        .FontSize(8);

                                    table.Cell()
                                        .Border(1)
                                        .BorderColor("D0D7DE")
                                        .Padding(4)
                                        .AlignCenter()
                                        .Text(item.Quantity.ToString("0.##"))
                                        .FontSize(8);

                                    table.Cell()
                                        .Border(1)
                                        .BorderColor("D0D7DE")
                                        .Padding(4)
                                        .AlignCenter()
                                        .Text(item.Unit ?? "Nos")
                                        .FontSize(8);

                                    table.Cell()
                                        .Border(1)
                                        .BorderColor("D0D7DE")
                                        .Padding(4)
                                        .AlignRight()
                                        .Text($"₹{item.Rate:N2}")
                                        .FontSize(8);

                                    table.Cell()
                                        .Border(1)
                                        .BorderColor("D0D7DE")
                                        .Padding(4)
                                        .AlignCenter()
                                        .Text($"{item.GSTPercentage:0.##}%")
                                        .FontSize(8);

                                    table.Cell()
                                        .Border(1)
                                        .BorderColor("D0D7DE")
                                        .Padding(4)
                                        .AlignRight()
                                        .Text($"₹{item.TotalAmount:N2}")
                                        .FontSize(8);
                                }
                            });

                        // AMOUNT + TOTALS
                        column.Item()
                            .PaddingTop(8)
                            .Row(row =>
                            {
                                row.RelativeItem(1.55f)
                                    .Border(1)
                                    .BorderColor("AAB7C4")
                                    .Background("FAFCFE")
                                    .Padding(10)
                                    .MinHeight(100)
                                    .Column(left =>
                                    {
                                        left.Item()
                                            .Text("AMOUNT IN WORDS")
                                            .Bold()
                                            .FontColor("1F3557")
                                            .FontSize(9);

                                        left.Item()
                                            .PaddingTop(10)
                                            .Text($"{NumberToWords(quotation.GrandTotal)} Rupees Only")
                                            .Bold()
                                            .FontSize(9.5f)
                                            .FontColor("222222");
                                    });

                                row.ConstantItem(230)
                                    .Border(1)
                                    .BorderColor("AAB7C4")
                                    .Padding(10)
                                    .MinHeight(100)
                                    .Column(totals =>
                                    {
                                        totals.Item()
                                            .Row(line =>
                                            {
                                                line.RelativeItem()
                                                    .Text("Subtotal")
                                                    .FontSize(8.8f);

                                                line.ConstantItem(95)
                                                    .AlignRight()
                                                    .Text($"₹{quotation.SubTotal:N2}")
                                                    .FontSize(8.8f);
                                            });

                                        totals.Item()
                                            .PaddingTop(6)
                                            .Row(line =>
                                            {
                                                line.RelativeItem()
                                                    .Text("GST Amount")
                                                    .FontSize(8.8f);

                                                line.ConstantItem(95)
                                                    .AlignRight()
                                                    .Text($"₹{quotation.GSTAmount:N2}")
                                                    .FontSize(8.8f);
                                            });

                                        totals.Item()
                                            .PaddingTop(7)
                                            .BorderTop(1)
                                            .BorderColor("1F4E78")
                                            .PaddingTop(7)
                                            .Row(line =>
                                            {
                                                line.RelativeItem()
                                                    .Text("GRAND TOTAL")
                                                    .Bold()
                                                    .FontColor("1F3557")
                                                    .FontSize(11);

                                                line.ConstantItem(95)
                                                    .AlignRight()
                                                    .Text($"₹{quotation.GrandTotal:N2}")
                                                    .Bold()
                                                    .FontColor("1F3557")
                                                    .FontSize(13);
                                            });
                                    });
                            });

                        // SHORT NOTE
                        column.Item()
                            .PaddingTop(8)
                            .Background("F5F8FB")
                            .Border(1)
                            .BorderColor("D6DEE6")
                            .Padding(7)
                            .Text("We appreciate the opportunity to submit this quotation and look forward to working with you.")
                            .FontSize(8)
                            .Italic()
                            .FontColor("555555")
                            .AlignCenter();

                        // SIGNATURES
                        column.Item()
                            .PaddingTop(16)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .AlignLeft()
                                    .Column(left =>
                                    {
                                        left.Item()
                                            .Text("Prepared By")
                                            .Bold()
                                            .FontSize(9);

                                        left.Item()
                                            .PaddingTop(22)
                                            .Text("________________________")
                                            .FontSize(8);

                                        left.Item()
                                            .PaddingTop(2)
                                            .Text("Name / Signature")
                                            .FontSize(7.5f)
                                            .FontColor("666666");
                                    });

                                row.ConstantItem(245)
                                    .AlignRight()
                                    .Column(right =>
                                    {
                                        right.Item()
                                            .Text("For " + companyName)
                                            .Bold()
                                            .FontSize(9)
                                            .AlignCenter();

                                        right.Item()
                                            .PaddingTop(22)
                                            .Text("________________________")
                                            .FontSize(8)
                                            .AlignCenter();

                                        right.Item()
                                            .PaddingTop(2)
                                            .Text("Authorized Signatory")
                                            .FontSize(8)
                                            .AlignCenter();
                                    });
                            });

                        // -----------------------------------------------------
                        // PAGE 2 - SPECIFIC TERMS & CONDITIONS
                        // -----------------------------------------------------
                        column.Item().PageBreak();

                        column.Item()
                            .PaddingTop(4)
                            .Background("1F4E78")
                            .PaddingVertical(8)
                            .AlignCenter()
                            .Text("TERMS & CONDITIONS")
                            .FontSize(17)
                            .Bold()
                            .FontColor("FFFFFF");

                        column.Item()
                            .PaddingTop(10)
                            .Border(1)
                            .BorderColor("AAB7C4")
                            .Padding(12)
                            .Column(terms =>
                            {
                                if (!string.IsNullOrWhiteSpace(quotation.TermsAndConditions))
                                {
                                    var lines = quotation.TermsAndConditions
                                        .Replace("\r\n", "\n")
                                        .Replace("\r", "\n")
                                        .Split('\n', StringSplitOptions.RemoveEmptyEntries);

                                    int termNo = 1;

                                    foreach (var line in lines)
                                    {
                                        string cleanLine = line.Trim();

                                        if (string.IsNullOrWhiteSpace(cleanLine))
                                            continue;

                                        terms.Item()
                                            .PaddingBottom(5)
                                            .Row(row =>
                                            {
                                                row.ConstantItem(24)
                                                    .Background("D9EAF7")
                                                    .Padding(4)
                                                    .AlignCenter()
                                                    .Text(termNo.ToString())
                                                    .Bold()
                                                    .FontColor("1F3557")
                                                    .FontSize(8.5f);

                                                row.ConstantItem(7);

                                                row.RelativeItem()
                                                    .Text(cleanLine)
                                                    .FontSize(9);
                                            });

                                        termNo++;
                                    }
                                }
                                else
                                {
                                    AddDefaultTerms(terms);
                                }
                            });

                        // FINAL DECLARATION
                        column.Item()
                            .PaddingTop(15)
                            .AlignCenter()
                            .Text("This quotation is prepared for the above-mentioned customer and work requirements.")
                            .FontSize(8)
                            .Italic()
                            .FontColor("666666");

                        column.Item()
                            .PaddingTop(24)
                            .AlignRight()
                            .Row(row =>
                            {
                                row.ConstantItem(245)
                                    .AlignRight()
                                    .Column(sign =>
                                    {
                                        sign.Item()
                                            .Text("For " + companyName)
                                            .Bold()
                                            .FontSize(9)
                                            .AlignCenter();

                                        sign.Item()
                                            .PaddingTop(20)
                                            .Text("________________________")
                                            .FontSize(8)
                                            .AlignCenter();

                                        sign.Item()
                                            .PaddingTop(2)
                                            .Text("Authorized Signatory")
                                            .FontSize(8)
                                            .AlignCenter();
                                    });
                            });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static (string Line1, string Line2) SplitCompanyName(string companyName)
        {
            string clean = (companyName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(clean))
                return ("RANGRASIYA", "");

            string upper = clean.ToUpper();

            int andIndex = upper.IndexOf(" AND ", StringComparison.Ordinal);

            if (andIndex > 0)
            {
                string line1 = clean[..andIndex].Trim();
                string line2 = clean[(andIndex + 5)..].Trim();

                if (!string.IsNullOrWhiteSpace(line1) &&
                    !string.IsNullOrWhiteSpace(line2))
                {
                    return (line1.ToUpper(), "AND " + line2.ToUpper());
                }
            }

            // If the name is long but has no "AND", split near the middle.
            if (clean.Length > 42)
            {
                int middle = clean.Length / 2;
                int split = clean.LastIndexOf(' ', middle);

                if (split > 0)
                {
                    return (
                        clean[..split].Trim().ToUpper(),
                        clean[(split + 1)..].Trim().ToUpper()
                    );
                }
            }

            return (clean.ToUpper(), "");
        }

        private static (string Line1, string Line2) SplitAddress(string address)
        {
            string clean = (address ?? "").Trim();

            if (string.IsNullOrWhiteSpace(clean))
                return ("", "");

            // Prefer a natural split around the first comma after a reasonable length.
            if (clean.Length <= 62)
                return (clean, "");

            int split = clean.LastIndexOf(',', Math.Min(clean.Length - 1, 62));

            if (split <= 0)
                split = clean.LastIndexOf(' ', Math.Min(clean.Length - 1, 62));

            if (split <= 0)
                return (clean, "");

            return (
                clean[..split].Trim().TrimEnd(','),
                clean[(split + 1)..].Trim()
            );
        }

        private static void AddDetailRow(
            ColumnDescriptor column,
            string key,
            string value,
            bool first = false)
        {
            column.Item()
                .PaddingTop(first ? 5 : 2)
                .Row(row =>
                {
                    row.ConstantItem(92)
                        .Text(key)
                        .FontSize(8.6f)
                        .FontColor("444444");

                    row.ConstantItem(10)
                        .AlignCenter()
                        .Text(":")
                        .FontSize(8.6f)
                        .Bold()
                        .FontColor("1F4E78");

                    row.RelativeItem()
                        .Text(value)
                        .FontSize(8.6f)
                        .Bold();
                });
        }

        private static void AddDefaultTerms(ColumnDescriptor terms)
        {
            string[] defaults =
            {
                "This quotation is valid for the mentioned validity period.",
                "Work will be started after customer confirmation.",
                "Any additional work will be charged separately.",
                "Rates are subject to the agreed work specifications.",
                "Terms may be discussed and finalized mutually."
            };

            for (int i = 0; i < defaults.Length; i++)
            {
                int number = i + 1;

                terms.Item()
                    .PaddingBottom(7)
                    .Row(row =>
                    {
                        row.ConstantItem(24)
                            .Background("D9EAF7")
                            .Padding(4)
                            .AlignCenter()
                            .Text(number.ToString())
                            .Bold()
                            .FontColor("1F3557")
                            .FontSize(8.5f);

                        row.ConstantItem(7);

                        row.RelativeItem()
                            .Text(defaults[i])
                            .FontSize(9);
                    });
            }
        }

        private string NumberToWords(decimal number)
        {
            long rupees = (long)Math.Floor(number);

            if (rupees == 0)
                return "Zero";

            string[] ones =
            {
                "", "One", "Two", "Three", "Four", "Five",
                "Six", "Seven", "Eight", "Nine", "Ten",
                "Eleven", "Twelve", "Thirteen", "Fourteen",
                "Fifteen", "Sixteen", "Seventeen", "Eighteen",
                "Nineteen"
            };

            string[] tens =
            {
                "", "", "Twenty", "Thirty", "Forty",
                "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
            };

            string Convert(long n)
            {
                if (n < 20)
                    return ones[n];

                if (n < 100)
                    return tens[n / 10] +
                           (n % 10 > 0 ? " " + ones[n % 10] : "");

                if (n < 1000)
                    return ones[n / 100] + " Hundred" +
                           (n % 100 > 0 ? " " + Convert(n % 100) : "");

                if (n < 100000)
                    return Convert(n / 1000) + " Thousand" +
                           (n % 1000 > 0 ? " " + Convert(n % 1000) : "");

                if (n < 10000000)
                    return Convert(n / 100000) + " Lakh" +
                           (n % 100000 > 0 ? " " + Convert(n % 100000) : "");

                return Convert(n / 10000000) + " Crore" +
                       (n % 10000000 > 0 ? " " + Convert(n % 10000000) : "");
            }

            return Convert(rupees);
        }
    }
}
