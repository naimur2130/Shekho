using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.EventSource;
using Shekho.Data;
using Shekho.Models;
using System.Runtime.InteropServices;

namespace Shekho.Areas.InstructorArea.Controllers
{
    [Area("InstructorArea")]
    public class LessonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public LessonController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int moduleId)
        {
            var user = await _userManager.GetUserAsync(User);
            var lesson = await _context.CourseSection.Include(u => u.Course).Include(u => u.Lesson)
                .FirstOrDefaultAsync(u => u.CourseSectionId == moduleId && u.Course.InstructorId==user.Id);
            
            if (lesson == null)
            {
                return NotFound();
            }

            ViewBag.CourseSectionId = moduleId;
            ViewBag.CourseSectionTitle = lesson.CourseSectionName;

            return View(lesson.Lesson.OrderBy(u=>u.Order).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> CreateLesson(int moduleId)
        {
            ViewBag.CourseSectionId = moduleId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(2L * 1024 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024)]
        public async Task<IActionResult> CreateLesson(
    int moduleId,
    Lesson lesson,
    IFormFile? resource)
        {
            var user = await _userManager.GetUserAsync(User);

            var lessons = await _context.CourseSection
                .Include(u => u.Course)
                .FirstOrDefaultAsync(u =>
                    u.CourseSectionId == moduleId &&
                    u.Course.InstructorId == user.Id);

            if (lessons == null)
                return NotFound();

            if (resource != null && resource.Length > 0)
            {
                if (resource.Length > 2L * 1024 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File size exceeds limit.");
                    return View(lesson);
                }

                var allowedTypes = new[]
                {
                    "video/mp4",
                    "video/mkv",
                    "video/webm"
                };

                if (!allowedTypes.Contains(resource.ContentType))
                {
                    ModelState.AddModelError("", "Invalid video format.");
                    return View(lesson);
                }

                string uploadPath = Path.Combine(
                    _env.WebRootPath,
                    "Upload",
                    "VideoResource");

                Directory.CreateDirectory(uploadPath);

                string fileName = Guid.NewGuid() + Path.GetExtension(resource.FileName);
                string filePath = Path.Combine(uploadPath, fileName);

                await using var stream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

                await resource.CopyToAsync(stream);

                lesson.ResourcePath = "/Upload/VideoResource/" + fileName;
            }

            lesson.CourseSectionId = moduleId;
            lesson.Order = await _context.Lesson
                .CountAsync(u => u.CourseSectionId == moduleId) + 1;

            _context.Lesson.Add(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { moduleId });
        }


        [HttpGet]
        public async Task<IActionResult> EditLesson(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var lesson = await _context.Lesson
                .Include(l => l.CourseSection)
                .ThenInclude(cs => cs.Course)
                .FirstOrDefaultAsync(l =>
                    l.LessonId == id &&
                    l.CourseSection.Course.InstructorId == user.Id);

            if (lesson == null)
                return NotFound();

            return View(lesson);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(2L * 1024 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024)]
        public async Task<IActionResult> EditLesson(
    int id,
    Lesson lesson,
    IFormFile? resource)
        {
            var user = await _userManager.GetUserAsync(User);

            var lessons = await _context.Lesson
                .Include(l => l.CourseSection)
                .ThenInclude(cs => cs.Course)
                .FirstOrDefaultAsync(l =>
                    l.LessonId == id &&
                    l.CourseSection.Course.InstructorId == user.Id &&
                    l.LessonId == lesson.LessonId);

            if (lessons == null)
                return NotFound();

            if (resource != null && resource.Length > 0)
            {
                if (resource.Length > 2L * 1024 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File size exceeds limit.");
                    return View(lesson);
                }

                var allowedTypes = new[]
                {
                    "video/mp4",
                    "video/mkv",
                    "video/webm"
                };

                if (!allowedTypes.Contains(resource.ContentType))
                {
                    ModelState.AddModelError("", "Invalid video format.");
                    return View(lesson);
                }

                string uploadPath = Path.Combine(
                    _env.WebRootPath,
                    "Upload",
                    "VideoResource");

                Directory.CreateDirectory(uploadPath);

                string fileName = Guid.NewGuid() + Path.GetExtension(resource.FileName);
                string filePath = Path.Combine(uploadPath, fileName);

                await using var stream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

                await resource.CopyToAsync(stream);

                if (!string.IsNullOrEmpty(lessons.ResourcePath))
                {
                    string oldFilePath = Path.Combine(
                        _env.WebRootPath,
                        lessons.ResourcePath.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                lessons.ResourcePath = "/Upload/VideoResource/" + fileName;
            }

            lessons.LessonTitle = lesson.LessonTitle;
            lessons.VideoUrl = lesson.VideoUrl;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index),
                new { moduleId = lessons.CourseSectionId });
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var lessons = await _context.Lesson.Include(u => u.CourseSection).ThenInclude(u => u.Course)
                .FirstOrDefaultAsync(u => u.LessonId == id && u.CourseSection.Course.InstructorId == user.Id);
            if (lessons == null)
            {
                return NotFound();
            }

            return View(lessons);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var lessons = await _context.Lesson.FindAsync(id);
            if (lessons == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(lessons.ResourcePath))
            {
                var path = Path.Combine(_env.WebRootPath, lessons.ResourcePath.TrimStart('/'));
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            _context.Lesson.Remove(lessons);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { moduleId = lessons.CourseSectionId });

        }
    }
}
