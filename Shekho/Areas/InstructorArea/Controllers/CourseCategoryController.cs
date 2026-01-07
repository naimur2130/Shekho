using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;

namespace Shekho.Areas.InstructorArea.Controllers
{
    [Area("InstructorArea")]
    public class CourseCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CourseCategory.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseCategory category)
        {
            if (!ModelState.IsValid)
                return View(category);

            _context.CourseCategory.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
