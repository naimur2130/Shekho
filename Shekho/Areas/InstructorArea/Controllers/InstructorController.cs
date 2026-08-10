using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.ViewModels;

namespace Shekho.Areas.InstructorArea.Controllers
{
    [Area("InstructorArea")]
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public InstructorController(ApplicationDbContext context,
                                   UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var instructorId = _userManager.GetUserId(User);

            var courses = await _context.Course
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();

            var enrollments = await _context.Enrollment
                .Include(e => e.Course)
                .Where(e => e.Course.InstructorId == instructorId && e.IsPaid)
                .ToListAsync();

            var vm = new InstructorDashboardViewModel
            {
                TotalCourses = courses.Count,
                TotalStudents = enrollments
                    .Select(e => e.StudentId)
                    .Distinct()
                    .Count(),

                TotalRevenue = enrollments.Sum(e => e.AmountPaid),
                InstructorRevenue = enrollments.Sum(e => e.InstructorAmount),
                AdminRevenue = enrollments.Sum(e => e.AdminAmount),

                Courses = courses.Select(c => new CourseAnalyticsViewModel
                {
                    CourseId = c.CourseId,
                    CourseTitle = c.CourseTitle,
                    EnrolledStudents = enrollments
                        .Count(e => e.CourseId == c.CourseId),
                    Revenue = enrollments
                        .Where(e => e.CourseId == c.CourseId)
                        .Sum(e => e.InstructorAmount)
                }).ToList()
            };

            return View(vm);
        }
    }

}
