using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;
using Stripe;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AccountController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> InstructorDetails(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest();

        var profile = await _context.instructorProfile
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new InstructorProfile { UserId = userId };
        }

        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> InstructorDetails(InstructorProfile model, IFormFile? IdDocument, IFormFile? ProfilePicture)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existingProfile = await _context.instructorProfile
            .FirstOrDefaultAsync(p => p.UserId == model.UserId);

        string docUploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Upload/InstructorDocuments");
        Directory.CreateDirectory(docUploadFolder);

        string picUploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Upload/InstructorProfilePictures");
        Directory.CreateDirectory(picUploadFolder);

        if (IdDocument != null && IdDocument.Length > 0)
        {
            if (!string.IsNullOrEmpty(existingProfile?.IdDocumentPath))
            {
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath,
                    existingProfile.IdDocumentPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }

            string fileName = Guid.NewGuid() + Path.GetExtension(IdDocument.FileName);
            string filePath = Path.Combine(docUploadFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await IdDocument.CopyToAsync(stream);

            model.IdDocumentPath = "/Upload/InstructorDocuments/" + fileName;
        }

        if (ProfilePicture != null && ProfilePicture.Length > 0)
        {
            if (!string.IsNullOrEmpty(existingProfile?.ProfilePicturePath))
            {
                string oldPicPath = Path.Combine(_webHostEnvironment.WebRootPath,
                    existingProfile.ProfilePicturePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(oldPicPath))
                    System.IO.File.Delete(oldPicPath);
            }

            string picFileName = Guid.NewGuid() + Path.GetExtension(ProfilePicture.FileName);
            string picFilePath = Path.Combine(picUploadFolder, picFileName);

            using var stream = new FileStream(picFilePath, FileMode.Create);
            await ProfilePicture.CopyToAsync(stream);

            model.ProfilePicturePath = "/Upload/InstructorProfilePictures/" + picFileName;
        }

        if (existingProfile != null)
        {
            existingProfile.FullName = model.FullName;
            existingProfile.PhoneNumber = model.PhoneNumber;
            existingProfile.Address = model.Address;
            existingProfile.Qualification = model.Qualification;

            if (!string.IsNullOrEmpty(model.IdDocumentPath))
                existingProfile.IdDocumentPath = model.IdDocumentPath;

            if (!string.IsNullOrEmpty(model.ProfilePicturePath))
                existingProfile.ProfilePicturePath = model.ProfilePicturePath;

            _context.instructorProfile.Update(existingProfile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Instructor", new { area = "InstructorArea" });
        }
        else
        {
            model.IsApproved = false;
            _context.instructorProfile.Add(model);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Your profile has been saved. Admin approval is required.";
            return RedirectToAction("Index", "Home");
        }
    }

    public async Task<IActionResult> StudentDetails(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest();

        var profile = await _context.studentProfile
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            profile = new StudentProfile { UserId = userId };

        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> StudentDetails(StudentProfile model, IFormFile? ProfilePicturePath)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existingProfile = await _context.studentProfile
            .FirstOrDefaultAsync(p => p.UserId == model.UserId);

        if (ProfilePicturePath != null && ProfilePicturePath.Length > 0)
        {
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Upload/StudentProfilePicture");
            Directory.CreateDirectory(uploadPath);

            string fileName = Guid.NewGuid() + Path.GetExtension(ProfilePicturePath.FileName);
            string filePath = Path.Combine(uploadPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await ProfilePicturePath.CopyToAsync(stream);

            model.ProfilePicturePath = "/Upload/StudentProfilePicture/" + fileName;

            if (existingProfile != null && !string.IsNullOrEmpty(existingProfile.ProfilePicturePath))
            {
                string oldPath = Path.Combine(_webHostEnvironment.WebRootPath,
                    existingProfile.ProfilePicturePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }
        }

        if (existingProfile != null)
        {
            existingProfile.FullName = model.FullName;
            existingProfile.PhoneNumber = model.PhoneNumber;
            existingProfile.Address = model.Address;
            existingProfile.CurrentInstitution = model.CurrentInstitution;

            if (!string.IsNullOrEmpty(model.ProfilePicturePath))
                existingProfile.ProfilePicturePath = model.ProfilePicturePath;

            _context.studentProfile.Update(existingProfile);
        }
        else
        {
            _context.studentProfile.Add(model);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Course", new { area = "StudentArea" });
    }

}
