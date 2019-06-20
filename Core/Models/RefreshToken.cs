using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Core.Models
{
    [Table("AspNetRefreshTokens")]
    public class RefreshToken: ModelBase
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        [Required]
        public int TotalRefresh { get; set; }

        [Required]
        public bool Revoked { get; set; }

        public User User { get; set; }
    }
}