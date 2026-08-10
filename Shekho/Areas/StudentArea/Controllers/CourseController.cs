using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;
using Shekho.ViewModels;

namespace Shekho.Areas.StudentArea.Controllers
{
    [Area("StudentArea")]
    [Authorize(Roles = "Student")]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CourseController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            int? categoryId,
            int? subCategoryId,
            DifficultyLevel? difficulty,
            string searchTerm)
        {
            var query = _context.Course
                .Include(c => c.Category)
                .Include(c => c.SubCategory)
                .Where(c => c.IsApproved && c.IsPublished)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId);

            if (subCategoryId.HasValue)
                query = query.Where(c => c.SubCategoryId == subCategoryId);

            if (difficulty.HasValue)
                query = query.Where(c => c.DifficultyLevel == difficulty);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(c => c.CourseTitle.Contains(searchTerm));

            var courses = await query.ToListAsync();

            var userId = _userManager.GetUserId(User);

            var enrolledCourseIds = new List<int>();

            if (userId != null)
            {
                enrolledCourseIds = await _context.Enrollment
                    .Where(e => e.StudentId == userId)
                    .Select(e => e.CourseId)
                    .ToListAsync();
            }

            var model = new CourseBrowsingViewModel
            {
                Courses = courses,
                Categories = await _context.CourseCategory.ToListAsync(),
                SubCategories = await _context.CourseSubCategory.ToListAsync(),
                SelectedCategoryId = categoryId,
                SelectedSubCategoryId = subCategoryId,
                SelectedDifficulty = difficulty,
                SearchTerm = searchTerm,
                EnrolledCourseIds = enrolledCourseIds
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Course
                .Include(c => c.Category)
                .Include(c => c.SubCategory)
                .Include(c => c.courseSections)!
                    .ThenInclude(s => s.Lesson)
                .FirstOrDefaultAsync(c => c.CourseId == id && c.IsApproved);

            if (course == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            bool isEnrolled = false;
            if (userId != null)
            {
                isEnrolled = await _context.Enrollment
                    .AnyAsync(e => e.CourseId == id && e.StudentId == userId);
            }

            var model = new CourseDetailsViewModel
            {
                Course = course,
                IsEnrolled = isEnrolled
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var subCategories = await _context.CourseSubCategory
                .Where(s => s.CategoryId == categoryId)
                .Select(s => new
                {
                    subCategoryId = s.SubCategoryId,
                    subCategoryName = s.SubCategoryName
                })
                .ToListAsync();

            return Json(subCategories);
        }

    }
}
