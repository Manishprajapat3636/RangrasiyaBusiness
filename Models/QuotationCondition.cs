using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RangrasiyaBusiness.Models
{
    public class QuotationCondition
    {
        [Key]
        public int QuotationConditionId { get; set; }

        [Required]
        public int QuotationId { get; set; }

        [Required]
        public string ConditionText { get; set; } = string.Empty;

        // Navigation
        [ForeignKey("QuotationId")]
        public Quotation? Quotation { get; set; }
    }
}