using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;
using Stripe.Checkout;

namespace Shekho.Areas.StudentArea.Controllers
{
    [Area("StudentArea")]
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EnrollmentController(ApplicationDbContext context,
                                    UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Enroll(int courseId)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var course = await _context.Course
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.IsApproved);

            if (course == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            bool exists = await _context.Enrollment
                .AnyAsync(e => e.CourseId == courseId && e.StudentId == userId);

            if (exists)
                return RedirectToAction("Details", "Course", new { area = "StudentArea", id = courseId });

            if (course.IsFree || course.CoursePrice == 0)
            {
                var enrollment = new Enrollment
                {
                    CourseId = courseId,
                    StudentId = userId,
                    IsPaid = false
                };

                _context.Enrollment.Add(enrollment);
                await _context.SaveChangesAsync();

                return View("Success");
            }

            return RedirectToAction("Checkout", new { courseId });
        }

        public async Task<IActionResult> Checkout(int courseId)
        {
            var course = await _context.Course
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.IsApproved);

            if (course == null)
                return NotFound();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(course.CoursePrice * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = course.CourseTitle
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = Url.Action(
                    "PaymentSuccess",
                    "Enrollment",
                    null,
                    Request.Scheme),

                CancelUrl = Url.Action(
                    "PaymentCancel",
                    "Enrollment",
                    null,
                    Request.Scheme)
            };

            var service = new SessionService();
            var session = service.Create(options);

            // 🔐 Store session data securely
            TempData["StripeSessionId"] = session.Id;
            TempData["CourseId"] = courseId;

            return Redirect(session.Url);
        }

        public async Task<IActionResult> PaymentSuccess()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            if (TempData["StripeSessionId"] == null ||
                TempData["CourseId"] == null)
                return RedirectToAction("Index", "Course", new { area = "StudentArea" });

            var sessionId = TempData["StripeSessionId"].ToString();
            var courseId = (int)TempData["CourseId"];
            var userId = _userManager.GetUserId(User);

            var service = new SessionService();
            var session = service.Get(sessionId);

            if (session.PaymentStatus != "paid")
                return RedirectToAction("PaymentCancel");

            bool exists = await _context.Enrollment.AnyAsync(e =>
                e.CourseId == courseId && e.StudentId == userId);

            if (!exists)
            {
                var enrollment = new Enrollment
                {
                    CourseId = courseId,
                    StudentId = userId,
                    IsPaid = true,
                    PaymentIntentId = session.PaymentIntentId
                };

                _context.Enrollment.Add(enrollment);
                await _context.SaveChangesAsync();
            }

            return View("Success");
        }

        public IActionResult PaymentCancel()
        {
            return View();
        }
    }
}
