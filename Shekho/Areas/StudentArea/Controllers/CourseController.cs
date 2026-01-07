using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;
using Shekho.ViewModels;

namespace Shekho.Areas.StudentArea.Controllers
{
    [Area("StudentArea")]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
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
                .Where(c => c.IsApproved) 
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId.Value);

            if (subCategoryId.HasValue)
                query = query.Where(c => c.SubCategoryId == subCategoryId.Value);

            if (difficulty.HasValue)
                query = query.Where(c => c.DifficultyLevel == difficulty.Value);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(c => c.CourseTitle.Contains(searchTerm));

            var model = new CourseBrowsingViewModel
            {
                Courses = await query.ToListAsync(),
                Categories = await _context.CourseCategory.ToListAsync(),
                SubCategories = await _context.CourseSubCategory.ToListAsync(),
                SelectedCategoryId = categoryId,
                SelectedSubCategoryId = subCategoryId,
                SelectedDifficulty = difficulty,
                SearchTerm = searchTerm
            };

            return View(model);
        }
    }
}
