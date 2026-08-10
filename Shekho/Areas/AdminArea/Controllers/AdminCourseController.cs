using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Services;

namespace Shekho.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    [Authorize(Roles = "Admin")]
    public class AdminCourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        public AdminCourseController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
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
            var course = await _context.Course
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            course.IsApproved = true;
            await _context.SaveChangesAsync();

            
            var instructor = await _userManager.FindByIdAsync(course.InstructorId!);

            if (instructor != null)
            {
                var subject = "🎉 Your Course Has Been Approved!";
                var body = $@"
            <h2>Congratulations!</h2>
            <p>Your course <strong>{course.CourseTitle}</strong> has been approved.</p>
            <p>It is now visible to students.</p>
        ";

                await _emailService.SendEmailAsync(instructor.Email!, subject, body);
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var course = await _context.Course
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            
            var instructor = await _userManager.FindByIdAsync(course.InstructorId!);
            if (instructor != null)
            {
                var subject = "❌ Course Rejected";
                var body = $@"
            <h2>Course Rejected</h2>
            <p>Unfortunately, your course <strong>{course.CourseTitle}</strong> was rejected.</p>
            <p>Please review the guidelines and resubmit.</p>
        ";

                await _emailService.SendEmailAsync(instructor.Email!, subject, body);
            }

            
            if (!string.IsNullOrEmpty(course.ThumbnailPath))
            {
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    course.ThumbnailPath.TrimStart('/'));

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Course.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }

}
