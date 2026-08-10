using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class LessonProgress
    {
        public int LessonProgressId { get; set; }
        public string StudentId { get; set; }

        public int CourseId { get; set; }
        public int LessonId { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        [ForeignKey("CourseId")]
        [ValidateNever]
        public Course Course { get; set; }
        [ForeignKey("LessonId")]
        [ValidateNever]
        public Lesson Lesson { get; set; }
        [ForeignKey("StudentId")]
        [ValidateNever]
        public IdentityUser Student { get; set; }

    }

}
