using System;
using System.ComponentModel.DataAnnotations;

namespace API.Data.Dtos
{
    public class UserRegisterDto
    {
        [Required]
        [StringLength(maximumLength: 20, ErrorMessage = "Must be at least 3 to 20 characters.", MinimumLength = 3)]
        public string UserName { get; set; }

        [Required]
        [StringLength(maximumLength: 30, ErrorMessage = "Must be at least 8 to 30 characters.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(maximumLength: 255, ErrorMessage = "Must be at least 2 to 255 characters.", MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(maximumLength: 255, ErrorMessage = "Must be at least 2 to 255 characters.", MinimumLength = 2)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}