using RangrasiyaBusiness.Models;
using System.ComponentModel.DataAnnotations;

namespace RangrasiyaBusiness.ViewModels
{
    public class InvoiceCreateViewModel
    {
        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Required]
        public int CustomerId { get; set; }

        public string PaymentStatus { get; set; } = "Pending";

        public List<InvoiceItemViewModel> Items { get; set; } = new();
        public string WorkOrderNo { get; set; } = string.Empty;

        public string PlaceOfWork { get; set; } = string.Empty;

        public string SACHSNCode { get; set; } = string.Empty;
    }

    public class InvoiceItemViewModel
    {
        [Required]
        public int ItemId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Rate { get; set; }

        public decimal GSTPercentage { get; set; }
    }
}