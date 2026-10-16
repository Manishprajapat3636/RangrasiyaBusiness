using Microsoft.AspNetCore.Identity;

namespace RangrasiyaBusiness.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
