using Microsoft.AspNetCore.Mvc;

namespace RangrasiyaBusiness.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}