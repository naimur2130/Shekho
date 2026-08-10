using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;
using Shekho.Services;
using Shekho.ViewModels;

namespace Shekho.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;
        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult PendingInstructors()
        {
            var instructors = _context.instructorProfile
                .Where(i => !i.IsApproved)
                .Include(i => i.User)
                .ToList();
            return View(instructors);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveInstructor(int id)
        {
            var instructor = await _context.instructorProfile.FindAsync(id);

            if (instructor == null)
                return NotFound();

            instructor.IsApproved = true;
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(instructor.UserId);

            if (user != null)
            {
                await _emailService.SendEmailAsync(
                    user.Email!,
                    "Instructor Account Approved 🎉",
                    $@"
            <h3>Congratulations!</h3>
            <p>Your instructor account has been approved by Admin.</p>
            <p>You can now login and start creating courses.</p>
            <br />
            <b>Shekho Team</b>"
                );
            }

            return RedirectToAction(nameof(PendingInstructors));
        }

        [HttpPost]
        public async Task<IActionResult> RejectInstructor(int id)
        {
            var instructor = await _context.instructorProfile.FindAsync(id);

            if (instructor == null)
                return NotFound();

            var user = await _context.Users.FindAsync(instructor.UserId);

            if (user != null)
            {
                await _emailService.SendEmailAsync(
                    user.Email!,
                    "Instructor Request Rejected",
                    $@"
            <h3>Sorry!</h3>
            <p>Your instructor registration request has been rejected.</p>
            <p>You may contact support for more details.</p>
            <br />
            <b>Shekho Team</b>"
                );

                _context.Users.Remove(user);
            }

            _context.instructorProfile.Remove(instructor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingInstructors));
        }

        public async Task<IActionResult> Users()
        {
            var allUsers = _userManager.Users.ToList();
            var vm = new AdminUsersPageViewModel();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                var userVm = new AdminUserViewModel
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    Role = role!,
                    IsBlocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now
                };

                if (role == "Instructor")
                {
                    var profile = await _context.instructorProfile
                                                .FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (profile != null)
                    {
                        userVm.FullName = profile.FullName;
                        userVm.PhoneNumber = profile.PhoneNumber;
                        userVm.Address = profile.Address;
                        userVm.Qualification = profile.Qualification;
                        userVm.IsApproved = profile.IsApproved;
                    }
                    vm.Instructors.Add(userVm);
                }
                else if (role == "Student")
                {
                    var profile = await _context.studentProfile
                                                .FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (profile != null)
                    {
                        userVm.FullName = profile.FullName;
                        userVm.PhoneNumber = profile.PhoneNumber;
                        userVm.Address = profile.Address;
                        userVm.CurrentInstitution = profile.CurrentInstitution;
                        userVm.ProfilePicturePath = profile.ProfilePicturePath;
                    }
                    vm.Students.Add(userVm);
                }
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
                await _userManager.UpdateAsync(user);

                // 📧 Email notification
                await _emailService.SendEmailAsync(
                    user.Email!,
                    "🚫 Account Blocked – Shekho",
                    $@"
                <p>Dear {user.UserName},</p>

                <p>Your account has been <strong>blocked</strong> by the administrator.</p>

                <p>You will not be able to log in until further notice.</p>

                <p>If you believe this is a mistake, please contact support.</p>

                <br/>
                <p>— Shekho Admin Team</p>
            "
                );
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                user.LockoutEnd = null;
                await _userManager.UpdateAsync(user);

                // 📧 Email notification
                await _emailService.SendEmailAsync(
                    user.Email!,
                    "✅ Account Unblocked – Shekho",
                    $@"
                <p>Dear {user.UserName},</p>

                <p>Your account has been <strong>unblocked</strong>.</p>

                <p>You can now log in and continue using the platform.</p>

                <br/>
                <p>— Shekho Admin Team</p>
            "
                );
            }

            return RedirectToAction(nameof(Users));
        }


        public IActionResult Dashboard()
        {
            var vm = new AdminAnalyticsViewModel();

            vm.TotalCourses = _context.Course.Count();

            vm.TotalEnrollments = _context.Enrollment.Count();

            var paidPayments = _context.Enrollment
                .Where(p => p.IsPaid);

            vm.TotalRevenue = paidPayments.Sum(p => p.AmountPaid);
            vm.AdminRevenue = paidPayments.Sum(p => p.AdminAmount);
            vm.InstructorRevenue = paidPayments.Sum(p => p.InstructorAmount);

            return View(vm);
        }
    }
}
