using Microsoft.AspNetCore.Mvc;

namespace Sports_Analysis.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 