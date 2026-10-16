using System.ComponentModel.DataAnnotations;

namespace RangrasiyaBusiness.Models
{
    public class Quotation
    {
        [Key]
        public int QuotationId { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string QuotationNumber { get; set; } = string.Empty;

        public DateTime QuotationDate { get; set; } = DateTime.Now;

        public int CustomerId { get; set; }

        public decimal SubTotal { get; set; }

        public decimal GSTAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public string Status { get; set; } = "Draft";
        public string WorkOrderNo { get; set; } = string.Empty;

        public string PlaceOfWork { get; set; } = string.Empty;

        public string QuotationValidity { get; set; } = "15 Days";

        public string TermsAndConditions { get; set; } = string.Empty;
        public List<QuotationItem> QuotationItems { get; set; } = new();
        public Customer? Customer { get; set; }
    }
}