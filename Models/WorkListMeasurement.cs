using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RangrasiyaBusiness.Models
{
    public class WorkListMeasurement
    {
        [Key]
        public int WorkListMeasurementId { get; set; }

        [Required]
        public int WorkListId { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Unit { get; set; } = "Sq.Ft";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Length { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Width { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [ForeignKey(nameof(WorkListId))]
        public WorkList? WorkList { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; } = 1;
    }
}