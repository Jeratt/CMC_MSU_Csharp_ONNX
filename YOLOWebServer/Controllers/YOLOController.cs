using Microsoft.AspNetCore.Mvc;

namespace YOLOWebServer.Controllers
{
    public class YOLOController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
