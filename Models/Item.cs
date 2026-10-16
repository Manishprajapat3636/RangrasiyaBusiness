using System.ComponentModel.DataAnnotations;

namespace RangrasiyaBusiness.Models
{
    public class Item
    {
        [Key]
        public int ItemId { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string ItemName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal Rate { get; set; }

        public decimal GSTPercentage { get; set; }
    }
}