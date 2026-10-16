using System.ComponentModel.DataAnnotations;

namespace RangrasiyaBusiness.Models
{
    public class InvoiceItem
    {
        [Key]
        public int InvoiceItemId { get; set; }

        public int InvoiceId { get; set; }

        public int ItemId { get; set; }

        // Item name as it appeared when this invoice line was saved.
        public string? ItemNameSnapshot { get; set; }

        public decimal Quantity { get; set; }

        public decimal Rate { get; set; }

        public decimal GSTPercentage { get; set; }

        public decimal Amount { get; set; }

        public decimal GSTAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public Invoice? Invoice { get; set; }

        public Item? Item { get; set; }
    }
}