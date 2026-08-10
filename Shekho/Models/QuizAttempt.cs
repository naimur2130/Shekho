using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class QuizAttempt
    {
        [Key]
        public int AttemptId { get; set; }
        public string StudentId { get; set; } = null!;
        [ForeignKey("StudentId")]
        public IdentityUser Student { get; set; } = null!;
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;
        public DateTime AttemptedAt { get; set; } = DateTime.Now;
        public int Score { get; set; }
        public bool Passed { get; set; }
    }

}
