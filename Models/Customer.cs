
using System.ComponentModel.DataAnnotations;

namespace RangrasiyaBusiness.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        public string ContactPerson { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string GSTIN { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
       
    }
}