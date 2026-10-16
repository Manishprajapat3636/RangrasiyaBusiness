using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RangrasiyaBusiness.Data;
using RangrasiyaBusiness.Models;
using System.IO;
using System.Linq;

namespace RangrasiyaBusiness.Services
{
    public class InvoicePdfService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public InvoicePdfService(IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _environment = environment;
            _context = context;
        }

        public byte[] GeneratePdf(Invoice invoice)
        {
            var company = _context.CompanyProfiles.FirstOrDefault(c => c.UserId == invoice.UserId);

            string companyName = company?.CompanyName ?? "RANGRASIYA";
            string ownerName = company?.OwnerName ?? "";
            string address = company?.Address ?? "";
            string phone = company?.Phone ?? "";
            string email = company?.Email ?? "";
            string gstin = company?.GSTIN ?? "";
            string panNumber = company?.PAN ?? "";
            string udyamNumber = company?.UDYAMNumber ?? "";
            string businessType = company?.BusinessType ?? "";

            // Customer phone may currently contain phone and email separated by "/".
            // Keep the database unchanged and display them as separate fields in the PDF.
            string customerPhone = invoice.Customer?.Phone ?? "";
            string customerEmail = "";

            int customerContactSeparator = customerPhone.IndexOf('/');
            if (customerContactSeparator >= 0)
            {
                customerEmail = customerPhone[(customerContactSeparator + 1)..].Trim();
                customerPhone = customerPhone[..customerContactSeparator].Trim();
            }

            // Company name: first line ends before AND, second line starts with AND.
            string upperCompanyName = companyName.ToUpperInvariant();
            string companyNameLine1 = upperCompanyName;
            string companyNameLine2 = "";
            int andIndex = upperCompanyName.IndexOf(" AND ", StringComparison.OrdinalIgnoreCase);

            if (andIndex >= 0)
            {
                companyNameLine1 = upperCompanyName[..andIndex].Trim();
                companyNameLine2 = upperCompanyName[andIndex..].Trim();
            }

            // Header address is shown in two lines.
            string addressLine1 = address;
            string addressLine2 = "";
            if (address.Contains(","))
            {
                int commaIndex = address.IndexOf(',');
                addressLine1 = address[..commaIndex].Trim().TrimEnd(',');
                addressLine2 = address[(commaIndex + 1)..].Trim();
            }

            string? logoPath = null;

            if (!string.IsNullOrWhiteSpace(company?.LogoPath))
            {
                string savedPath = company.LogoPath
                    .TrimStart('/')
                    .Replace("/", Path.DirectorySeparatorChar.ToString());

                logoPath = Path.Combine(
                    _environment.WebRootPath,
                    savedPath
                );
            }

            if (string.IsNullOrWhiteSpace(logoPath) || !File.Exists(logoPath))
            {
                string imagesFolder = Path.Combine(
                    _environment.WebRootPath,
                    "images"
                );

                if (Directory.Exists(imagesFolder))
                {
                    string[] imageFiles = Directory.GetFiles(imagesFolder);

                    logoPath = imageFiles.FirstOrDefault(file =>
                        file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".webp", StringComparison.OrdinalIgnoreCase));
                }
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(28);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(header =>
                    {
                        header.Item()
                            .Border(1)
                            .BorderColor("1F4E78")
                            .Padding(8)
                            .Row(row =>
                            {
                                // LEFT: Company logo
                                if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
                                {
                                    row.ConstantItem(150)
                                        .Height(92)
                                        .AlignMiddle()
                                        .AlignCenter()
                                        .Image(logoPath)
                                        .FitArea();
                                }

                                // RIGHT: Complete company information
                                row.RelativeItem()
                                    .PaddingLeft(12)
                                    .AlignRight()
                                    .Column(info =>
                                    {
                                        info.Item()
                                            .AlignRight()
                                            .Text(companyNameLine1)
                                            .FontSize(16)
                                            .Bold()
                                            .FontColor("1F4E78");

                                        if (!string.IsNullOrWhiteSpace(companyNameLine2))
                                        {
                                            info.Item()
                                                .AlignRight()
                                                .Text(companyNameLine2)
                                                .FontSize(16)
                                                .Bold()
                                                .FontColor("1F4E78");
                                        }

                                        info.Item()
                                            .PaddingTop(5)
                                            .AlignRight()
                                            .Text($"Registered Office : {addressLine1}")
                                            .FontSize(8);

                                        if (!string.IsNullOrWhiteSpace(addressLine2))
                                        {
                                            info.Item()
                                                .AlignRight()
                                                .Text(addressLine2)
                                                .FontSize(8);
                                        }

                                        info.Item()
                                            .AlignRight()
                                            .Text($"Contact No.: {phone}  |  E-mail: {email}")
                                            .FontSize(8);

                                        info.Item()
                                            .AlignRight()
                                            .Text($"GSTIN : {gstin}  |  PAN : {panNumber}  |  UDYAM : {udyamNumber}")
                                            .FontSize(8);
                                    });
                            });

                        header.Item()
                            .PaddingTop(5)
                            .BorderBottom(1)
                            .BorderColor("1F4E78")
                            .AlignCenter()
                            .Text(businessType)
                            .FontSize(8)
                            .Bold();
                    });

                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem()
                                .Background("D9EAF7")
                                .Padding(7)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text("TAX INVOICE")
                                .FontSize(19)
                                .Bold()
                                .FontColor("1F4E78");

                            row.ConstantItem(190)
                                .Background("1F4E78")
                                .Padding(7)
                                .Column(details =>
                                {
                                    details.Item().Text($"Invoice No.      : {invoice.InvoiceNumber}")
                                        .FontColor("FFFFFF").Bold();
                                    details.Item().Text($"Date of Issue   : {invoice.InvoiceDate:dd / MM / yyyy}")
                                        .FontColor("FFFFFF");
                                    details.Item().Text("Place of Supply : Rajasthan (08)")
                                        .FontColor("FFFFFF");
                                });
                        });

                        column.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.15f);
                                columns.RelativeColumn(1.15f);
                            });

                            table.Cell().Border(1).BorderColor("B7C9D6").Padding(7).Column(left =>
                            {
                                left.Item().Background("D9EAF7").Padding(4)
                                    .Text("Bill To (Customer Details)").Bold().FontColor("1F4E78");

                                left.Item().PaddingTop(4).Table(details =>
                                {
                                    details.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(82);
                                        columns.ConstantColumn(7);
                                        columns.RelativeColumn();
                                    });

                                    details.Cell().Text("Name").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(invoice.Customer?.CustomerName ?? "-")
                                        .FontSize(8.5f).Bold();

                                    details.Cell().Text("Contact Person").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(invoice.Customer?.ContactPerson ?? "-")
                                        .FontSize(8.5f);

                                    details.Cell().Text("Address").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(invoice.Customer?.Address ?? "-")
                                        .FontSize(8f).FontColor("333333");

                                    details.Cell().Text("State").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(invoice.Customer?.City ?? "-")
                                        .FontSize(8.5f);

                                    details.Cell().Text("Phone").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(string.IsNullOrWhiteSpace(customerPhone) ? "-" : customerPhone)
                                        .FontSize(8.5f);

                                    details.Cell().Text("Email").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(string.IsNullOrWhiteSpace(customerEmail) ? "-" : customerEmail)
                                        .FontSize(8.5f);

                                    details.Cell().Text("GSTIN").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(invoice.Customer?.GSTIN ?? "-")
                                        .FontSize(8.5f);
                                });
                            });

                            table.Cell().Border(1).BorderColor("B7C9D6").Padding(7).Column(right =>
                            {
                                right.Item().Background("D9EAF7").Padding(4)
                                    .Text("Work Details").Bold().FontColor("1F4E78");

                                right.Item().PaddingTop(4).Table(details =>
                                {
                                    details.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(92);
                                        columns.ConstantColumn(7);
                                        columns.RelativeColumn();
                                    });

                                    details.Cell().Text("Work Order/Date No.").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(Display(invoice.WorkOrderNo))
                                        .FontSize(8.5f);

                                    //details.Cell().Text("Work Order Date").FontSize(8.5f);
                                    //details.Cell().Text(":").FontSize(8.5f);
                                    //details.Cell().Text("__________________")
                                    //    .FontSize(8.5f);

                                    details.Cell().Text("SAC Code").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(Display(invoice.SACHSNCode))
                                        .FontSize(8.5f);

                                    details.Cell().Text("HSN Code").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(Display(invoice.SACHSNCode))
                                        .FontSize(8.5f);

                                    details.Cell().Text("Place of Work").FontSize(8.5f);
                                    details.Cell().Text(":").FontSize(8.5f);
                                    details.Cell().Text(Display(invoice.PlaceOfWork))
                                        .FontSize(8.5f);
                                });
                            });
                        });

                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(35);
                                columns.RelativeColumn(3.2f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.4f);
                            });

                            table.Header(header =>
                            {
                                string[] headings = { "Sr. No.", "Description of Work / Items", "Unit", "Quantity", "Rate (₹)", "Amount (₹)" };
                                foreach (var heading in headings)
                                {
                                    header.Cell().Background("D9EAF7").Border(1).BorderColor("B7C9D6")
                                        .Padding(5).AlignCenter().Text(heading).Bold().FontColor("1F2F46");
                                }
                            });

                            int serialNumber = 1;
                            foreach (var item in invoice.InvoiceItems)
                            {
                                var background = serialNumber % 2 == 0
                                      ? Colors.Grey.Lighten4
                                      : Colors.White;
                                int currentSerial = serialNumber++;

                                table.Cell().Background(background).Border(1).BorderColor("D0D7DE").Padding(5)
                                    .AlignCenter().Text(currentSerial.ToString());
                                table.Cell().Background(background).Border(1).BorderColor("D0D7DE").Padding(5)
                                    .Text(item.ItemNameSnapshot ?? item.Item?.ItemName ?? "Item / Service");
                                table.Cell().Background(background).Border(1).BorderColor("D0D7DE").Padding(5)
                                    .AlignCenter().Text(string.IsNullOrWhiteSpace(item.Item?.Unit) ? "Nos" : item.Item.Unit);
                                table.Cell().Background(background).Border(1).BorderColor("D0D7DE").Padding(5)
                                    .AlignCenter().Text(item.Quantity.ToString("0.##"));
                                table.Cell().Background(background).Border(1).BorderColor("D0D7DE").Padding(5)
                                    .AlignRight().Text($"{item.Rate:N2}");
                                table.Cell().Background(background).Border(1).BorderColor("D0D7DE").Padding(5)
                                    .AlignRight().Text($"{item.TotalAmount:N2}");
                            }
                        });

                        column.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.6f);
                                columns.RelativeColumn(1.1f);
                            });

                            table.Cell().Border(1).BorderColor("B7C9D6").Padding(8).AlignMiddle().Column(words =>
                            {
                                words.Item().Text("Amount in Words").Bold().FontColor("1F4E78");
                                words.Item().PaddingTop(6).Text($"{NumberToIndianWords(invoice.GrandTotal)} Only").Italic();
                            });

                            table.Cell().Border(1).BorderColor("B7C9D6").Column(totals =>
                            {
                                totals.Item().Padding(6).Row(row =>
                                {
                                    row.RelativeItem().Text("Amount Before Tax");
                                    row.AutoItem().Text($"₹{invoice.SubTotal:N2}");
                                });
                                totals.Item().Padding(6).Row(row =>
                                {
                                    row.RelativeItem().Text("Add : CGST @ 9%");
                                    row.AutoItem().Text($"₹{invoice.GSTAmount / 2m:N2}");
                                });
                                totals.Item().Padding(6).Row(row =>
                                {
                                    row.RelativeItem().Text("Add : SGST @ 9%");
                                    row.AutoItem().Text($"₹{invoice.GSTAmount / 2m:N2}");
                                });
                                totals.Item().Padding(6).Row(row =>
                                {
                                    row.RelativeItem().Text("Total Tax Amount").Bold();
                                    row.AutoItem().Text($"₹{invoice.GSTAmount:N2}");
                                });
                                totals.Item().Background("D9EAF7").Padding(6).Row(row =>
                                {
                                    row.RelativeItem().Text("TOTAL AMOUNT AFTER TAX").Bold().FontColor("1F4E78");
                                    row.AutoItem().Text($"₹{invoice.GrandTotal:N2}").Bold().FontSize(12).FontColor("1F4E78");
                                });
                            });
                        });

                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Border(1).BorderColor("B7C9D6").Padding(8).Column(bank =>
                            {
                                bank.Item().Background("D9EAF7").Padding(4).Text("BANK DETAILS").Bold().FontColor("1F4E78");
                                bank.Item().PaddingTop(5).Text($"Bank Name: {company?.BankName ?? "-"}");

                                bank.Item().Text($"Account No.: {company?.AccountNumber ?? "-"}");

                                bank.Item().Text($"IFSC Code: {company?.IFSCCode ?? "-"}");

                                bank.Item().Text($"Branch: {company?.Branch ?? "-"}");
                            });

                            table.Cell().Border(1).BorderColor("B7C9D6").Padding(8).Column(terms =>
                            {
                                terms.Item().Background("D9EAF7").Padding(4).Text("TERMS & CONDITIONS").Bold().FontColor("1F4E78");
                                terms.Item().PaddingTop(5).Text(company?.TermsAndConditions ?? "Payment to be made as mutually agreed.");

                                terms.Item().PaddingTop(5).Text("This is a computer-generated invoice.");
                            });
                        });

                        column.Item().PaddingTop(28).AlignRight().Column(right =>
                        {
                            right.Item().AlignRight().Text($"For {companyName}").Bold();
                            right.Item().PaddingTop(25).AlignRight().Text("________________________");
                            right.Item().AlignRight().Text("Authorized Signatory").Bold();
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Thank you for your business | ").FontSize(8).FontColor("666666");
                        text.Span(companyName).Bold().FontSize(8).FontColor("666666");
                    });
                });
            });

            return document.GeneratePdf();
        }


        private static string Display(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private static string NumberToIndianWords(decimal amount)
        {
            long rupees = (long)Math.Floor(amount);
            int paise = (int)Math.Round((amount - rupees) * 100, MidpointRounding.AwayFromZero);

            if (paise == 100)
            {
                rupees++;
                paise = 0;
            }

            string result = NumberToWords(rupees) + " Rupees";

            if (paise > 0)
                result += " and " + NumberToWords(paise) + " Paise";

            return result;
        }

        private static string NumberToWords(long number)
        {
            if (number == 0)
                return "Zero";

            string[] ones =
            {
                "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
                "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
                "Seventeen", "Eighteen", "Nineteen"
            };

            string[] tens =
            {
                "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
            };

            string ConvertBelowThousand(long value)
            {
                string words = "";

                if (value >= 100)
                {
                    words += ones[value / 100] + " Hundred";
                    value %= 100;
                    if (value > 0)
                        words += " ";
                }

                if (value >= 20)
                {
                    words += tens[value / 10];
                    value %= 10;
                    if (value > 0)
                        words += " " + ones[value];
                }
                else if (value > 0)
                {
                    words += ones[value];
                }

                return words;
            }

            string result = "";

            if (number >= 10000000)
            {
                result += ConvertBelowThousand(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if (number >= 100000)
            {
                result += ConvertBelowThousand(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if (number >= 1000)
            {
                result += ConvertBelowThousand(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if (number > 0)
                result += ConvertBelowThousand(number);

            return result.Trim();
        }
    }
}
