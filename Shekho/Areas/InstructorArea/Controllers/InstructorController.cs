using Microsoft.AspNetCore.Mvc;

namespace Shekho.Areas.InstructorArea.Controllers
{
    [Area("InstructorArea")]
    public class InstructorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
