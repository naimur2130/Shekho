namespace Shekho.ViewModels
{
    public class AdminAnalyticsViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal AdminRevenue { get; set; }
        public decimal InstructorRevenue { get; set; }
    }

}
