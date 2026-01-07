using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;

[Area("AdminArea")]
public class AdminCourseController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminCourseController(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index()
    {
        var pendingCourses = await _context.Course
            .Where(c => !c.IsApproved)
            .Include(c => c.Category)
            .Include(c => c.SubCategory)
            .ToListAsync();

        return View(pendingCourses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var course = await _context.Course.FirstOrDefaultAsync(c => c.CourseId == id);
        if (course == null) return NotFound();

        course.IsApproved = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var course = await _context.Course.FirstOrDefaultAsync(c => c.CourseId == id);
        if (course == null) return NotFound();

        if (!string.IsNullOrEmpty(course.ThumbnailPath))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.ThumbnailPath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        _context.Course.Remove(course);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
