using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;

public class ProfileController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;

    public ProfileController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Index", "Home");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();

        if (role == "Student")
        {
            var profile = await _context.studentProfile.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
                return RedirectToAction("StudentDetails", "Account", new {area = "", userId = user.Id });
            return RedirectToAction("StudentDetails", "Account", new { area = "", userId = user.Id });
        }
        else if (role == "Instructor")
        {
            var profile = await _context.instructorProfile.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
                return RedirectToAction("InstructorDetails", "Account", new { area = "", userId = user.Id });
            return RedirectToAction("InstructorDetails", "Account", new { area = "", userId = user.Id });
        }

        return RedirectToAction("Index", "Home");
    }
}
