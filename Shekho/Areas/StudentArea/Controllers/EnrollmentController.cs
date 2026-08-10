using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;
using Shekho.Services;
using Shekho.ViewModels;
using Stripe.Checkout;
using Stripe.V2;
using System;

namespace Shekho.Areas.StudentArea.Controllers
{
    [Area("StudentArea")]
    [Authorize(Roles = "Student")]
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        public EnrollmentController(ApplicationDbContext context,
                                    UserManager<IdentityUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Enroll(int courseId)
        {
            if (!User.Identity!.IsAuthenticated)
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
                    StudentId = userId!,
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

            const decimal BdtToUsdRate = 0.00786m;
            decimal usdPrice = Math.Round(course.CoursePrice!.Value * BdtToUsdRate, 2);


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
                    UnitAmount = (long)(usdPrice * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = course.CourseTitle
                    }
                },
                Quantity = 1
            }
        },

                Mode = "payment",

                SuccessUrl = Url.Action("PaymentSuccess", "Enrollment", null, Request.Scheme),
                CancelUrl = Url.Action(
            "PaymentCancel",
            "Enrollment",
            new { id = course.CourseId },
            Request.Scheme
        )
            };

            var service = new SessionService();
            var session = service.Create(options);

            TempData["StripeSessionId"] = session.Id;
            TempData["CourseId"] = courseId;

            return Redirect(session.Url);
        }


        public async Task<IActionResult> PaymentSuccess()
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            if (TempData["StripeSessionId"] == null ||
                TempData["CourseId"] == null)
                return RedirectToAction("Index", "Course", new { area = "StudentArea" });

            var sessionId = TempData["StripeSessionId"]!.ToString();
            var courseId = (int)TempData["CourseId"]!;
            var userId = _userManager.GetUserId(User);

            var service = new SessionService();
            var session = service.Get(sessionId);

            if (session.PaymentStatus != "paid")
                return RedirectToAction("PaymentCancel");

            bool exists = await _context.Enrollment.AnyAsync(e =>
                e.CourseId == courseId && e.StudentId == userId);

            if (!exists)
            {
                decimal amount = session.AmountTotal!.Value / 100m;
                var enrollment = new Enrollment
                {
                    CourseId = courseId,
                    StudentId = userId!,
                    IsPaid = true,
                    AmountPaid = amount,
                    InstructorAmount = amount * 0.70m,
                    AdminAmount = amount * 0.30m,
                    PaymentIntentId = session.PaymentIntentId
                };

                _context.Enrollment.Add(enrollment);
                await _context.SaveChangesAsync();

                // 🔔 SEND EMAIL
                var user = await _userManager.GetUserAsync(User);
                var subject = "Payment Successful 🎉";
                var body = $@"
                <h2>Payment Successful</h2>
                <p>You have successfully enrolled in the course.</p>
                <p><strong>Amount Paid:</strong> ${amount}</p>
                <p>Thank you for learning with us!</p>
                ";

                await _emailService.SendEmailAsync(user!.Email!, subject, body);

            }

            return View("Success");
        }

        public IActionResult PaymentCancel(int id)
        {
            var model = new CourseBrowsingViewModel
            {
                SelectedCourseId = id
            };

            return View(model);
        }

    }
}
