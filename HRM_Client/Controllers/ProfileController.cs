using Microsoft.AspNetCore.Mvc;

namespace HRM_Client.Controllers
{
    public class ProfileController : BaseController
    {
        public IActionResult Index()
        {
            
            return View();
        }
    }
}

