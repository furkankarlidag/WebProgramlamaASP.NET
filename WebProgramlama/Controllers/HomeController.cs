using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebProgramlama.Models;

namespace WebProgramlama.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            string currentTime = DateTime.Now.ToString("HH:mm");

            // Session'a değer atama
            HttpContext.Session.SetString("SessionKeyName", "Ürün yönetim uygulamamıza hoşgeldiniz...");

            // Cookie oluşturma
            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddMinutes(30);
            Response.Cookies.Append("CurrentTime", currentTime, option);

            // Session'dan değer okuma
            var sessionValue = HttpContext.Session.GetString("SessionKeyName");

            // Cookie okuma
            var cookieValue = Request.Cookies["CurrentTime"];

            ViewBag.SessionValue = sessionValue;
            ViewBag.CookieValue = cookieValue;

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Solutions()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
