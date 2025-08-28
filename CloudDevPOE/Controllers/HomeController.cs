using Microsoft.AspNetCore.Mvc;

namespace CloudDevPOE.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/Index
        // Displays the main home page of the application
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/Privacy
        // Displays the privacy policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Error
        // Displays the error page in case of exceptions
        // ResponseCache attributes ensure no caching of error responses
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
