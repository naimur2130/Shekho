using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shekho.Models
{
    public class Quiz
    {
        public int QuizId { get; set; }
        public string Question { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; }
        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        [ValidateNever]
        public Lesson Lesson { get; set; }

    }
}
