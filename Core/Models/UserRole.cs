using Microsoft.AspNetCore.Identity;

namespace API.Core.Models
{
    public class UserRole: IdentityUserRole<string>
    {
        public User User { get; set; }
        public Role Role { get; set; }
    }
}