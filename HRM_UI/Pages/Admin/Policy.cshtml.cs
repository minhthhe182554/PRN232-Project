using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace HRM_UI.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class PolicyModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public string? ErrorMessage { get; set; }

        public PolicyModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
            
        }
    }
}
