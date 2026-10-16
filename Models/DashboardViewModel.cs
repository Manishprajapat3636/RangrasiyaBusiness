using RangrasiyaBusiness.Models;

namespace RangrasiyaBusiness.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalItems { get; set; }
        public int TotalQuotations { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalWorks { get; set; }

        public decimal TotalInvoiceAmount { get; set; }

        // Recent Data
        public List<Invoice> RecentInvoices { get; set; } = new();

        public List<Quotation> RecentQuotations { get; set; } = new();
    }
}