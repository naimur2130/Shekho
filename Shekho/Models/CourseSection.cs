using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class CourseSection
    {
        public int CourseSectionId { get; set; }
        public string CourseSectionName { get; set; }
        public int Order {  get; set; }
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        [ValidateNever]
        public Course Course { get; set; }

        public ICollection<Lesson>? Lesson { get; set; }
    }
}
