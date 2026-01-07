using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        [Required]
        public string CourseTitle { get; set; }
        public string CourseDescription { get; set; }
        public decimal CoursePrice { get; set; }
        public bool IsFree { get; set; }
        public string? ThumbnailPath { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? InstructorId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [ValidateNever]
        public CourseCategory? Category { get; set; }

        [ForeignKey("SubCategoryId")]
        [ValidateNever]
        public CourseSubCategory? SubCategory { get; set; }


        [ForeignKey("InstructorId")]
        [ValidateNever]
        public IdentityUser User { get; set; }
        public ICollection<CourseSection>? courseSections { get; set; }

    }
}
