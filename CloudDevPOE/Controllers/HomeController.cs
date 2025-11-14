using Microsoft.AspNetCore.Mvc;

namespace CloudDevPOE.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/Index
        // Redirects to login if user is not authenticated
        public IActionResult Index()
        {
            // If user is not logged in, redirect to login page
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account");
            }

            // If user is logged in, show the home page
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
