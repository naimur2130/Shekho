using Shekho.Models;

namespace Shekho.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; }
        public bool IsEnrolled { get; set; } = false;
    }
}
