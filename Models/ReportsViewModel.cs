
namespace RangrasiyaBusiness.Models
{
    public class ReportsViewModel
    {
        public DateTime FromDate { get; set; } =
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DateTime ToDate { get; set; } =
            DateTime.Now.Date;

        public int TotalInvoices { get; set; }

        public decimal TotalSales { get; set; }

        public decimal TotalExpenses { get; set; }

        public decimal NetProfit { get; set; }

        public int TotalWorks { get; set; }

        public List<Invoice> Invoices { get; set; } = new();

        public List<WorkExpense> Expenses { get; set; } = new();
    }
}
