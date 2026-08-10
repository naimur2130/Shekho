using Shekho.Models;

namespace Shekho.ViewModels
{
    public class CreateQuizViewModel
    {
        public int CourseId { get; set; }
        public int TotalQuestions { get; set; }
        public List<Quiz> Quizzes { get; set; } = new();
    }
}
