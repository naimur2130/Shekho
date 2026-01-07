using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shekho.Data;
using Shekho.Models;

namespace Shekho.Areas.InstructorArea.Controllers
{
    [Area("InstructorArea")]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CourseController(ApplicationDbContext context, 
            UserManager<IdentityUser> userManager, 
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var CourseList = await _context.Course.Where(u=>u.InstructorId==user.Id).ToListAsync();
            return View(CourseList);
        }
        [HttpGet]
        public async Task<IActionResult> GetSubCategoriesByCategory(int categoryId)
        {
            var subCategories = await _context.CourseSubCategory
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new
                {
                    sc.SubCategoryId,
                    sc.SubCategoryName
                })
                .ToListAsync();

            return Json(subCategories);
        }


        [HttpGet]
        public IActionResult CreateCourse()
        {
            ViewBag.Categories = _context.CourseCategory.ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course, IFormFile? Thumbnail)
        {
            var user = await _userManager.GetUserAsync (User);
            course.InstructorId = user.Id; 
            if (user == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
               // var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(course);
            }
            if (Thumbnail != null)
            {
                string UploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Upload/Courses");
                Directory.CreateDirectory(UploadPath);
                string fileName = Guid.NewGuid()+Path.GetExtension(Thumbnail.FileName);
                string filePath = Path.Combine(UploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await Thumbnail.CopyToAsync(stream);
                course.ThumbnailPath = "/Upload/Courses/" + fileName;
            }
            course.InstructorId = user.Id;
            course.IsApproved = false;
            course.IsPublished = false;
            course.CreatedAt = DateTime.Now;
            if (course.IsFree)
            {
                course.CoursePrice = 0;
            }
            _context.Course.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Course.FirstOrDefaultAsync(u => u.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.Categories = _context.CourseCategory.ToList();

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Course course, IFormFile? Thumbnail)
        {
            var model = await _context.Course.FirstOrDefaultAsync(c => c.CourseId == id);

            if (model == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.CourseCategory.ToList();
                return View(model);
            }

            // ✅ BASIC FIELDS
            model.CourseTitle = course.CourseTitle;
            model.CourseDescription = course.CourseDescription;
            model.IsFree = course.IsFree;
            model.CoursePrice = course.IsFree ? 0 : course.CoursePrice;

            // ✅ CATEGORY & SUBCATEGORY (THIS WAS MISSING)
            model.CategoryId = course.CategoryId;
            model.SubCategoryId = course.SubCategoryId;

            // ✅ THUMBNAIL
            if (Thumbnail != null)
            {
                if (!string.IsNullOrEmpty(model.ThumbnailPath))
                {
                    var oldPath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        model.ThumbnailPath.TrimStart('/')
                    );

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Upload/Courses");
                Directory.CreateDirectory(uploadFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(Thumbnail.FileName);
                string filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await Thumbnail.CopyToAsync(stream);

                model.ThumbnailPath = "/Upload/Courses/" + fileName;
            }

            _context.Course.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Course.FirstOrDefaultAsync(u => u.CourseId == id && u.InstructorId==user.Id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync (User);
            var course = await _context.Course.FirstOrDefaultAsync(u => u.CourseId == id && u.InstructorId==user.Id);
            if (course == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(course.ThumbnailPath))
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, course.ThumbnailPath.TrimStart('/'));
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}
