using Shekho.Models;

namespace Shekho.ViewModels
{
    public class StudentQuizViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = null!;
        public List<Quiz> Quizzes { get; set; } = new List<Quiz>();

        public int TotalTimeMinutes { get; set; } = 10;
        public Dictionary<int, string> Answers { get; set; } = new Dictionary<int, string>();

        public bool IsRetake { get; set; } = false;
    }
}
