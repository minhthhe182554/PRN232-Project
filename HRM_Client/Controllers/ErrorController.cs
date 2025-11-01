using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/401")]
        public IActionResult Error401()
        {
            ViewBag.StatusCode = 401;
            ViewBag.Title = "Unauthorized";
            ViewBag.Message = "You don't have permission to access this resource.";
            ViewBag.Description = "Please sign in with an authorized account or contact your administrator.";
            return View("Error");
        }

        [Route("Error/403")]
        public IActionResult Error403()
        {
            ViewBag.StatusCode = 403;
            ViewBag.Title = "Access Denied";
            ViewBag.Message = "You don't have permission to access this page.";
            ViewBag.Description = "This resource requires additional privileges. Contact your administrator if you believe this is a mistake.";
            return View("Error");
        }

        [Route("Error/404")]
        public IActionResult Error404()
        {
            ViewBag.StatusCode = 404;
            ViewBag.Title = "Page Not Found";
            ViewBag.Message = "The page you're looking for doesn't exist.";
            ViewBag.Description = "The page may have been moved or deleted. Check the URL or return to the dashboard.";
            return View("Error");
        }

        [Route("Error/500")]
        public IActionResult Error500()
        {
            ViewBag.StatusCode = 500;
            ViewBag.Title = "Server Error";
            ViewBag.Message = "Something went wrong on our end.";
            ViewBag.Description = "We're working to fix the issue. Please try again later or contact support if the problem persists.";
            return View("Error");
        }

        [Route("Error")]
        public IActionResult Index()
        {
            ViewBag.StatusCode = 500;
            ViewBag.Title = "Error";
            ViewBag.Message = "An unexpected error occurred.";
            ViewBag.Description = "Please try again or contact support if the problem continues.";
            return View("Error");
        }
    }
}

