using Microsoft.AspNetCore.Mvc;
using Shekho.Data;

namespace Shekho.Areas.StudentArea.Controllers
{
    [Area("StudentArea")]
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
