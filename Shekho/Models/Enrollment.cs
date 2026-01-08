using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }

        public string StudentId { get; set; }
        public int CourseId { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.Now;

        public bool IsPaid { get; set; }
        public string? PaymentIntentId { get; set; }
        [ForeignKey("CourseId")]
        [ValidateNever]
        public Course Course { get; set; }
        [ForeignKey("StudentId")]
        [ValidateNever]
        public IdentityUser Student { get; set; }
    }
}
