using System.ComponentModel.DataAnnotations;

namespace RangrasiyaBusiness.Models
{
    public class Work
    {
        [Key]
        public int WorkId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        public string WorkName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a customer")]
        public int CustomerId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime? EndDate { get; set; }

        public string Status { get; set; } = "Proposed";

        public string Description { get; set; } = string.Empty;

        public string WorkLocation { get; set; } = string.Empty;

        public decimal EstimatedAmount { get; set; }

        public decimal ActualAmount { get; set; }

        public Customer? Customer { get; set; }

        public List<WorkExpense> WorkExpenses { get; set; } = new();
    }
}