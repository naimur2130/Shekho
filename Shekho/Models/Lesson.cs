using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }
        public string LessonTitle { get; set; }
        public string? VideoUrl { get; set; }
        public string? ResourcePath { get; set; }
        public int Order {  get; set; }
        public int CourseSectionId { get; set; }
        [ForeignKey("CourseSectionId")]
        [ValidateNever]
        public CourseSection CourseSection { get; set; }

    }
}
