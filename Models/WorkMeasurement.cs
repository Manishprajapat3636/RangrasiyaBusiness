using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RangrasiyaBusiness.Models
{
    public class WorkMeasurement
    {
        [Key]
        public int WorkMeasurementId { get; set; }

        [Required]
        public int WorkId { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Length { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Width { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [ForeignKey("WorkId")]
        public Work? Work { get; set; }
    }
}