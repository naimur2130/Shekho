using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class Quiz
    {
        public int QuizId { get; set; }
        [Required]
        public string Question { get; set; }
        [Required]
        public string OptionA { get; set; }
        [Required]
        public string OptionB { get; set; }
        [Required]
        public string OptionC { get; set; }
        [Required]
        public string OptionD { get; set; }
        [Required]
        public string CorrectAnswer { get; set; }
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        [ValidateNever]
        public Course Course { get; set; }

    }
}
