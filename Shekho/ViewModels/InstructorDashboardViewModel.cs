namespace Shekho.ViewModels
{
    public class InstructorDashboardViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal InstructorRevenue { get; set; }
        public decimal AdminRevenue { get; set; }

        public List<CourseAnalyticsViewModel> Courses { get; set; }
    }
}
