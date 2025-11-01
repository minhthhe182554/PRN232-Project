using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class DashboardController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

