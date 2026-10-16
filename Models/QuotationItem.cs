using System.ComponentModel.DataAnnotations;

namespace RangrasiyaBusiness.Models
{
    public class QuotationItem
    {
        [Key]
        public int QuotationItemId { get; set; }

        public int QuotationId { get; set; }

        public int ItemId { get; set; }

        // Item name as it appeared when this quotation line was saved.
        public string? ItemNameSnapshot { get; set; }

        public decimal Quantity { get; set; }
        [Required]
        public string Unit { get; set; } = "Nos";

        public decimal Rate { get; set; }

        public decimal GSTPercentage { get; set; }

        public decimal Amount { get; set; }

        public decimal GSTAmount { get; set; }

        public decimal TotalAmount { get; set; }

        // Navigation properties
        public Quotation? Quotation { get; set; }

        public Item? Item { get; set; }
    }
}