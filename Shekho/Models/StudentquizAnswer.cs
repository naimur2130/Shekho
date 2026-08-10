using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class StudentQuizAnswer
    {
        [Key]
        public int AnswerId { get; set; }
        public string StudentId { get; set; } = null!;
        [ForeignKey("StudentId")]
        [ValidateNever]
        public IdentityUser Student { get; set; } = null!;
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        [ValidateNever]
        public Course Course { get; set; } = null!;
        public int QuizId { get; set; }
        [ForeignKey("QuizId")]
        [ValidateNever]
        public Quiz Quiz { get; set; } = null!;
        public string SelectedAnswer { get; set; } = null!;
        public DateTime AttemptedAt { get; set; } = DateTime.Now;
    }

}
