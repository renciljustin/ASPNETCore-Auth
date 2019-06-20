using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace API.Core.Models
{
    public class Role: IdentityRole
    {
        public ICollection<UserRole> Users { get; set; }
    }
}