
using System.ComponentModel.DataAnnotations;

namespace RangrasiyaBusiness.Models
{
    public class WorkExpense
    {
        [Key]
        public int WorkExpenseId { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int WorkId { get; set; }

        [Required]
        public string ExpenseType { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        public string Notes { get; set; } = string.Empty;

        public Work? Work { get; set; }
    }
}
