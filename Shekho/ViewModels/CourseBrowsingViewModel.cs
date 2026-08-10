using Shekho.Models;

namespace Shekho.ViewModels
{
    public class CourseBrowsingViewModel
    {
        public IEnumerable<Course> Courses { get; set; }

        public IEnumerable<CourseCategory> Categories { get; set; }
        public IEnumerable<CourseSubCategory> SubCategories { get; set; }

        public int? SelectedCategoryId { get; set; }
        public int? SelectedSubCategoryId { get; set; }

        public DifficultyLevel? SelectedDifficulty { get; set; }

        public string SearchTerm { get; set; }
        public List<int> EnrolledCourseIds { get; set; } = new();
        public int SelectedCourseId { get; set; }
    }
}
