using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Shekho.Models
{
    public class InstructorProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public IdentityUser User { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Qualification { get; set; }

        public string? IdDocumentPath { get; set; } 
        public string? ProfilePicturePath { get; set; }

        public bool IsApproved { get; set; } = false; 
    }
}
