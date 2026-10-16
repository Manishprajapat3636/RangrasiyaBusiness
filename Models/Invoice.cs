using System.ComponentModel.DataAnnotations;

namespace RangrasiyaBusiness.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        public int CustomerId { get; set; }

        public decimal SubTotal { get; set; }

        public decimal GSTAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public string PaymentStatus { get; set; } = "Pending";

        public Customer? Customer { get; set; }

        public List<InvoiceItem> InvoiceItems { get; set; } = new();
        public string WorkOrderNo { get; set; } = string.Empty;

        public string PlaceOfWork { get; set; } = string.Empty;

        public string SACHSNCode { get; set; } = string.Empty;
    }
}