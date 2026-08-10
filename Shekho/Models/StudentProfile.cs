using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class StudentProfile
    {
        [Key]
        public int StudentProfileId { get; set; }

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
        public string CurrentInstitution { get; set; }

        public string? ProfilePicturePath { get; set; }

    }
}
