using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;
using Shekho.ViewModels;

[Area("StudentArea")]
[Authorize(Roles = "Student")]
public class QuizController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public QuizController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> AttemptQuiz(int courseId)
    {
        var userId = _userManager.GetUserId(User);

        var quizzes = await _context.Quiz
            .Where(q => q.CourseId == courseId)
            .ToListAsync();

        // Check previous attempts
        var lastAttempt = await _context.QuizAttempt
            .Where(a => a.CourseId == courseId && a.StudentId == userId)
            .OrderByDescending(a => a.AttemptedAt)
            .FirstOrDefaultAsync();

        bool isRetake = lastAttempt != null && !lastAttempt.Passed;

        return View(new StudentQuizViewModel
        {
            CourseId = courseId,
            CourseTitle = (await _context.Course.FindAsync(courseId))?.CourseTitle ?? "Course",
            Quizzes = quizzes,
            IsRetake = isRetake
        });
    }

    [HttpPost]
    public async Task<IActionResult> Submit(StudentQuizViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        var quizzes = await _context.Quiz.Where(q => q.CourseId == model.CourseId).ToListAsync();

        int totalQuestions = quizzes.Count;
        int correctCount = 0;

        foreach (var quiz in quizzes)
        {
            if (model.Answers.TryGetValue(quiz.QuizId, out string? selected))
            {
                _context.StudentQuizAnswer.Add(new StudentQuizAnswer
                {
                    StudentId = userId!,
                    CourseId = model.CourseId,
                    QuizId = quiz.QuizId,
                    SelectedAnswer = selected,
                    AttemptedAt = DateTime.Now
                });

                if (selected == quiz.CorrectAnswer)
                    correctCount++;
            }
        }

        int score = (int)((correctCount * 100.0) / totalQuestions);
        bool passed = score >= 60;

        _context.QuizAttempt.Add(new QuizAttempt
        {
            StudentId = userId!,
            CourseId = model.CourseId,
            AttemptedAt = DateTime.Now,
            Score = score,
            Passed = passed
        });

        await _context.SaveChangesAsync();

        TempData["QuizResult"] = passed ? "pass" : "fail";

        return RedirectToAction("CourseLearn", "Learning", new { courseId = model.CourseId });
    }


}
