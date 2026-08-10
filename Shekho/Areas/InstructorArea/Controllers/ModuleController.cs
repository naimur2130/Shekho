using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;

namespace Shekho.Areas.InstructorArea.Controllers
{
    [Area("InstructorArea")]
    [Authorize(Roles = "Instructor")]
    public class ModuleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ModuleController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Course.Include(u=>u.courseSections)
            .FirstOrDefaultAsync(u=>u.CourseId == courseId && u.InstructorId==user!.Id);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.CourseTitle;
            return View(course.courseSections!.OrderBy(u=>u.Order).ToList());
        }

        [HttpGet]
        public IActionResult CreateModule(int courseId) 
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModule(int courseId, CourseSection courseSection)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Course.FirstOrDefaultAsync
                (u => u.CourseId == courseId && u.InstructorId == user!.Id);
            if (course == null)
            {
                return NotFound();
            }
            courseSection.CourseId = courseId;  
            courseSection.Order= await _context.CourseSection.CountAsync(u=>u.CourseId==courseId)+1;
            _context.CourseSection.Add(courseSection);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new {courseId});
        }

        [HttpGet]
        public async Task<IActionResult> EditModule(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var module = await _context.CourseSection.Include(u=>u.Course).FirstOrDefaultAsync(u=>u.CourseSectionId==id);
            if (module == null)
            {
                return NotFound();
            }
            return View(module);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModule(int id, CourseSection courseSection)
        {
            var user = await _userManager.GetUserAsync(User);
            var module = await _context.CourseSection.Include(u => u.Course).FirstOrDefaultAsync(u => u.CourseSectionId == id);
            
            if (module == null)
            {
                return NotFound();
            }

            module.CourseSectionName = courseSection.CourseSectionName;
            _context.CourseSection.Update(module);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { courseId = module.CourseId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var module = await _context.CourseSection.Include(u => u.Course).FirstOrDefaultAsync(u => u.CourseSectionId == id);
            if (module == null)
            {
                return NotFound();
            }
            return View(module);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteModule(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var module = await _context.CourseSection.Include(u => u.Course).FirstOrDefaultAsync(u => u.CourseSectionId == id);

            if (module == null)
            {
                return NotFound();
            }

            _context.CourseSection.Remove(module);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { courseId = module.CourseId });
        }
    }
}
