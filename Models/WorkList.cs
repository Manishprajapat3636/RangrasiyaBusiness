using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RangrasiyaBusiness.Models
{
    public class WorkList
    {
        [Key]
        public int WorkListId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string WorkOrderNo { get; set; } = string.Empty;

        public string QuotationNo { get; set; } = string.Empty;

        public string BillNo { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a customer")]
        public int CustomerId { get; set; }

        public string OfficerName { get; set; } = string.Empty;

        public string OfficerContact { get; set; } = string.Empty;

        public string WorkType { get; set; } = string.Empty;

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string Department { get; set; } = string.Empty;

        public string WorkSiteAddress { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        public ICollection<WorkListMeasurement> WorkListMeasurements { get; set; }
            = new List<WorkListMeasurement>();
    }
}