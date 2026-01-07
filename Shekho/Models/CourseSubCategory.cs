using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class CourseSubCategory
    {
        [Key]
        public int SubCategoryId { get; set; }

        [Required]
        public string? SubCategoryName { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [ValidateNever]
        public CourseCategory Category { get; set; }

        public ICollection<Course>? Courses { get; set; }
    }
}
