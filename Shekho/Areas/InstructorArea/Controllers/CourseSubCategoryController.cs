using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;

namespace Shekho.Areas.InstructorArea.Controllers
{
    [Area("InstructorArea")]
    public class CourseSubCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseSubCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int categoryId)
        {
            var category = await _context.CourseCategory
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
                return NotFound();

            ViewBag.CategoryName = category.CategoryName;
            ViewBag.CategoryId = category.CategoryId;

            return View(category.SubCategories);
        }

        public IActionResult Create(int categoryId)
        {
            ViewBag.CategoryId = categoryId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseSubCategory subCategory)
        {
            if (!ModelState.IsValid)
                return View(subCategory);

            _context.CourseSubCategory.Add(subCategory);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { categoryId = subCategory.CategoryId });
        }
    }
}
