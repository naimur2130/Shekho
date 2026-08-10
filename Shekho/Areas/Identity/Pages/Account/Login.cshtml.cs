using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shekho.Data;
using Shekho.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Shekho.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;

        public LoginModel(SignInManager<IdentityUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (Input == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login data.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1️⃣ Find the user by email
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            // 2️⃣ Check password manually
            if (!await _userManager.CheckPasswordAsync(user, Input.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // 3️⃣ Instructor approval check (if user is an Instructor)
            if (await _userManager.IsInRoleAsync(user, "Instructor"))
            {
                var profile = await _context.instructorProfile
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (profile == null || !profile.IsApproved)
                {
                    ModelState.AddModelError(string.Empty, "Your instructor account has not been approved yet.");
                    return Page();
                }
            }

            // 4️⃣ Create claims including temporary 1-day claim
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName ?? ""),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim("TemporaryAccess", "True"),
        new Claim("ExpiresAt", DateTime.UtcNow.AddDays(1).ToString("o")) // ISO 8601
    };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 5️⃣ Sign in with cookie authentication
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal,
                new AuthenticationProperties
                {
                    IsPersistent = Input.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1) // 1 day expiry
                });

            _logger.LogInformation("User logged in with temporary claim.");

            // 6️⃣ Redirect based on role
            if (roles.Contains("Admin"))
                return RedirectToAction("Dashboard", "Admin", new { area = "AdminArea" });
            if (roles.Contains("Instructor"))
                return RedirectToAction("Index", "Instructor", new { area = "InstructorArea" });
            if (roles.Contains("Student"))
                return RedirectToAction("Index", "Course", new { area = "StudentArea" });

            // Fallback
            return Redirect(returnUrl);
        }
    }
}


