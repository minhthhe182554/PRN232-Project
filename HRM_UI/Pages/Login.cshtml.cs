using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HRM_UI.Enums;
using HRM_UI.Dtos.Auth;

namespace HRM_UI.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;


        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }


        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
            // Check if already authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                var roleString = User.FindFirst(ClaimTypes.Role)?.Value;
                if (!string.IsNullOrEmpty(roleString))
                {
                    RedirectToDashboard(roleString);
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Please enter both email and password.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("HRM_API");
                
                // create LoginRequestDto
                var loginRequest = new LoginRequest 
                { 
                    Email = this.Email, 
                    Password = this.Password 
                };
                
                var content = new StringContent(
                    JsonSerializer.Serialize(loginRequest), 
                    Encoding.UTF8, 
                    "application/json"
                );

                var response = await client.PostAsync("/api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse response to DTO 
                    var result = JsonSerializer.Deserialize<LoginResponse>(
                        responseContent, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (result?.User != null)
                    {
                        // Validate and parse role 
                        if (!IsValidRole(result.User.Role, out Role userRole))
                        {
                            ErrorMessage = "Invalid user role.";
                            return Page();
                        }

                        // Create claims with Role
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, result.User.Id),
                            new Claim(ClaimTypes.Email, result.User.Email),
                            new Claim(ClaimTypes.Name, result.User.FullName),
                            new Claim(ClaimTypes.Role, userRole.ToString())
                        };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, 
                            CookieAuthenticationDefaults.AuthenticationScheme
                        );
                        
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = false,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties
                        );

                        // Redirect using enum
                        return RedirectToDashboard(userRole);
                    }
                }

                ErrorMessage = "Invalid email or password.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }
        
        private bool IsValidRole(string roleString, out Role role)
        {
            // Try parse string to enum (case-insensitive)
            return Enum.TryParse<Role>(roleString, ignoreCase: true, out role);
        }

        
        private IActionResult RedirectToDashboard(Role role)
        {
            return role switch
            {
                Role.Admin => RedirectToPage("/Admin/Dashboard"),
                Role.Manager => RedirectToPage("/Manager/Dashboard"),
                Role.Employee => RedirectToPage("/Employee/Dashboard"),
                _ => RedirectToPage("/Error")
            };
        }
        
        private IActionResult RedirectToDashboard(string? roleString)
        {
            if (string.IsNullOrEmpty(roleString))
                return RedirectToPage("/Error");

            if (IsValidRole(roleString, out Role role))
                return RedirectToDashboard(role);

            return RedirectToPage("/Error");
        }
    }
}