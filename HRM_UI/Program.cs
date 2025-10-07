using Microsoft.AspNetCore.Authentication.Cookies;

namespace HRM_UI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddRazorPages();

			// Configure HttpClient for API calls
			builder.Services.AddHttpClient("HRM_API", client =>
			{
				client.BaseAddress = new Uri("http://localhost:5228/api");
				client.DefaultRequestHeaders.Add("Accept", "application/json");
			})
			.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
			{
				UseCookies = true,
				CookieContainer = new System.Net.CookieContainer()
			});

			// Configure Cookie Authentication
			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.Cookie.Name = "HRM.UI.Auth";
					options.LoginPath = "/Login";
					options.LogoutPath = "/Logout";
					options.AccessDeniedPath = "/AccessDenied";
					options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
					options.SlidingExpiration = true;
				});

			builder.Services.AddAuthorization();

			// Add Session
			builder.Services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(60);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Error");
			}
			app.UseStaticFiles();

			app.UseRouting();

			app.UseSession();
			app.UseAuthentication();
			app.UseAuthorization();

			app.MapRazorPages();
			
			app.Run();
		}
	}
}