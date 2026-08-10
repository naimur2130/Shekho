using Shekho.Models;

namespace Shekho.ViewModels
{
    public class CourseLearningViewModel
    {
        public Course Course { get; set; } = null!;
        public List<int> CompletedLessonIds { get; set; } = new();
        public bool AllLessonsCompleted { get; set; }
        public bool QuizPassed { get; set; }
        public bool QuizAttempted { get; set; } = false; 
        public bool QuizRetakeAllowed { get; set; } = false;
    }

}
