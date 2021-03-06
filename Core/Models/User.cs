using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace API.Core.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(maximumLength: 255, ErrorMessage = "Must be at least 2 to 255 characters.", MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(maximumLength: 255, ErrorMessage = "Must be at least 2 to 255 characters.", MinimumLength = 2)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public ICollection<UserRole> Roles { get; set; }
    }
}