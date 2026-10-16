using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace RangrasiyaBusiness.Models
{
    public class CompanyProfile
    {
        [Key]
        public int CompanyProfileId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        public string CompanyName { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Pincode { get; set; } = string.Empty;

        public string GSTIN { get; set; } = string.Empty;

        public string PAN { get; set; } = string.Empty;

        public string UDYAMNumber { get; set; } = string.Empty;

        public string Website { get; set; } = string.Empty;

        public string BusinessType { get; set; } = string.Empty;

        public string LogoPath { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? LogoFile { get; set; }

        public string BankName { get; set; } = string.Empty;

        public string AccountNumber { get; set; } = string.Empty;

        public string IFSCCode { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;

        public string TermsAndConditions { get; set; } = string.Empty;
    }
}
