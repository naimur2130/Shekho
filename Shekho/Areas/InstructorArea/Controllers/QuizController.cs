using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;
using Shekho.ViewModels;

namespace Shekho.Areas.InstructorArea.Controllers
{
    [Area("InstructorArea")]
    [Authorize(Roles = "Instructor")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuizController(ApplicationDbContext context,
                              UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);

            var course = await _context.Course
                .FirstOrDefaultAsync(c =>
                    c.CourseId == courseId &&
                    c.InstructorId == user!.Id &&
                    c.IsCompleted);

            if (course == null)
                return Unauthorized();

            return View(new CreateQuizViewModel { CourseId = courseId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQuizViewModel model)
        {
            if (!ModelState.IsValid || model.Quizzes.Count == 0)
            {
                ModelState.AddModelError("", "Please add quiz questions");
                return View(model);
            }

            foreach (var quiz in model.Quizzes)
            {
                quiz.CourseId = model.CourseId;
                _context.Quiz.Add(quiz);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Final quiz created successfully!";
            return RedirectToAction("Index", "Course");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);

            var course = await _context.Course
                .FirstOrDefaultAsync(c =>
                    c.CourseId == courseId &&
                    c.InstructorId == user!.Id);

            if (course == null)
                return Unauthorized();

            var quizzes = await _context.Quiz
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            var vm = new CreateQuizViewModel
            {
                CourseId = courseId,
                TotalQuestions = quizzes.Count,
                Quizzes = quizzes
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateQuizViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingQuizzes = await _context.Quiz
                .Where(q => q.CourseId == model.CourseId)
                .ToListAsync();

            var postedIds = model.Quizzes
                .Where(q => q.QuizId != 0)
                .Select(q => q.QuizId)
                .ToList();

            var toDelete = existingQuizzes
                .Where(q => !postedIds.Contains(q.QuizId))
                .ToList();

            if (toDelete.Any())
                _context.Quiz.RemoveRange(toDelete);

            foreach (var quizModel in model.Quizzes)
            {
                if (quizModel.QuizId == 0)
                {
                    quizModel.CourseId = model.CourseId;
                    _context.Quiz.Add(quizModel);
                }
                else
                {
                    var existingQuiz = existingQuizzes.First(q => q.QuizId == quizModel.QuizId);
                    existingQuiz.Question = quizModel.Question;
                    existingQuiz.OptionA = quizModel.OptionA;
                    existingQuiz.OptionB = quizModel.OptionB;
                    existingQuiz.OptionC = quizModel.OptionC;
                    existingQuiz.OptionD = quizModel.OptionD;
                    existingQuiz.CorrectAnswer = quizModel.CorrectAnswer;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Quiz updated successfully!";
            return RedirectToAction("Index", "Course");
        }


    }
}
