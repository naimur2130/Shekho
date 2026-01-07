using System.ComponentModel.DataAnnotations;

namespace Shekho.Models
{
    public class CourseCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string CategoryName { get; set; }

        public ICollection<CourseSubCategory>? SubCategories { get; set; } = null;
    }
}
