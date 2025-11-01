using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRM_Client.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Request.Cookies["AuthToken"];
            
            if (string.IsNullOrEmpty(token))
            {
                // Redirect to 401 error page
                context.Result = new RedirectResult("/Error/401");
                return;
            }

            // Check if session has required data
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
            {
                // Token exists but session expired - redirect to login
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Populate ViewBag for all pages
            ViewBag.Role = role;
            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Level = HttpContext.Session.GetInt32("Level") ?? 1;
            ViewBag.DepartmentName = HttpContext.Session.GetString("DepartmentName");
            
            base.OnActionExecuting(context);
        }
    }
}

