using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shekho.Data;

namespace Shekho.Areas.StudentArea.Controllers
{
    [Area("StudentArea")]
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
