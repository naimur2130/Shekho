using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shekho.Data;
using Shekho.Models;
using Shekho.Services;
using Shekho.ViewModels;

[Area("StudentArea")]
[Authorize(Roles = "Student")]
public class LearningController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ICertificateService _certificateService;

    public LearningController(ApplicationDbContext context,
                              UserManager<IdentityUser> userManager,
                              ICertificateService certificateService)
    {
        _context = context;
        _userManager = userManager;
        _certificateService = certificateService;
    }

    public async Task<IActionResult> CourseLearn(int courseId)
    {
        var userId = _userManager.GetUserId(User);

        bool enrolled = await _context.Enrollment
            .AnyAsync(e => e.CourseId == courseId && e.StudentId == userId);

        if (!enrolled)
            return Unauthorized();

        var course = await _context.Course
            .Include(c => c.courseSections!)
                .ThenInclude(s => s.Lesson)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

        var completedLessons = await _context.LessonProgress
            .Where(lp =>
                lp.CourseId == courseId &&
                lp.StudentId == userId &&
                lp.IsCompleted)
            .Select(lp => lp.LessonId)
            .ToListAsync();

        bool allLessonsCompleted = completedLessons.Count == course!.courseSections!.Sum(s => s.Lesson!.Count);

        var lastAttempt = await _context.QuizAttempt
            .Where(a => a.CourseId == courseId && a.StudentId == userId)
            .OrderByDescending(a => a.AttemptedAt)
            .FirstOrDefaultAsync();

        bool quizAttempted = lastAttempt != null;
        bool quizPassed = lastAttempt != null && lastAttempt.Passed;
        bool quizRetakeAllowed = lastAttempt != null && !lastAttempt.Passed;

        var model = new CourseLearningViewModel
        {
            Course = course!,
            CompletedLessonIds = completedLessons,
            AllLessonsCompleted = allLessonsCompleted,
            QuizPassed = quizPassed,
            QuizAttempted = quizAttempted,
            QuizRetakeAllowed = quizRetakeAllowed
        };

        return View(model);
    }



    [HttpGet]
    public async Task<IActionResult> PlayLesson(int lessonId)
    {
        var userId = _userManager.GetUserId(User);

        var lesson = await _context.Lesson
            .Include(l => l.CourseSection)
                .ThenInclude(cs => cs.Course)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);

        if (lesson == null)
            return NotFound();

        int courseId = lesson.CourseSection.Course.CourseId;

        bool enrolled = await _context.Enrollment
            .AnyAsync(e => e.CourseId == courseId && e.StudentId == userId);

        if (!enrolled)
            return Forbid();

        var previousLesson = await _context.Lesson
            .Where(l =>
                l.CourseSectionId == lesson.CourseSectionId &&
                l.Order < lesson.Order)
            .OrderByDescending(l => l.Order)
            .FirstOrDefaultAsync();

        if (previousLesson != null)
        {
            bool completed = await _context.LessonProgress.AnyAsync(lp =>
                lp.LessonId == previousLesson.LessonId &&
                lp.StudentId == userId &&
                lp.IsCompleted);

            if (!completed)
                return Forbid();
        }

        var filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            lesson.ResourcePath!.TrimStart('/'));

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(stream, "video/mp4", enableRangeProcessing: true);
    }

    [HttpPost]
    public async Task<IActionResult> CompleteLesson(int lessonId, int courseId)
    {
        var userId = _userManager.GetUserId(User);

        var progress = await _context.LessonProgress
            .FirstOrDefaultAsync(x =>
                x.LessonId == lessonId &&
                x.CourseId == courseId &&
                x.StudentId == userId);

        if (progress == null)
        {
            _context.LessonProgress.Add(new LessonProgress
            {
                LessonId = lessonId,
                CourseId = courseId,
                StudentId = userId!,
                IsCompleted = true,
                CompletedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    public async Task<bool> HasPassedQuiz(string studentId, int courseId)
    {
        var quizzes = await _context.Quiz
            .Where(q => q.CourseId == courseId)
            .ToListAsync();

        if (!quizzes.Any())
            return false; 

        var studentAnswers = await _context.StudentQuizAnswer
            .Where(a => a.StudentId == studentId && a.CourseId == courseId)
            .ToListAsync();

        if (!studentAnswers.Any())
            return false; 

        int totalQuestions = quizzes.Count;
        int correctAnswers = studentAnswers
            .Count(a => quizzes.Any(q => q.QuizId == a.QuizId && q.CorrectAnswer == a.SelectedAnswer));

        double score = (double)correctAnswers / totalQuestions * 100;

        const double passingScore = 60; 

        return score >= passingScore;
    }

    public async Task<IActionResult> DownloadCertificate(int courseId)
    {
        var user = await _userManager.GetUserAsync(User);

        var totalLessons = _context.Lesson
            .Count(l => l.CourseSection.CourseId == courseId);

        var completedLessons = _context.LessonProgress
            .Count(c => c.CourseId == courseId && c.StudentId == user!.Id && c.IsCompleted);

        if (completedLessons != totalLessons)
            return Unauthorized();

        var course = await _context.Course
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

        var pdfBytes = _certificateService.GenerateCertificate(
            user!.UserName ?? "Student",
            course?.CourseTitle ?? "Course",
            DateTime.Now
        );

        return File(pdfBytes, "application/pdf", $"{course?.CourseTitle}_Certificate.pdf");
    }
}

