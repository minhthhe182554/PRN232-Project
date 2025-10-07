using HumanResourceManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HumanResourceManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            // builder.Services.AddSwaggerGen();

            // 1. Configure Database Connection
            builder.Services.AddDbContext<HRMDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("HRM_Db")));

            // 2. Configure Identity 
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Username settings
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true; // User email setting

                // Email confirmation 
                options.SignIn.RequireConfirmedEmail = false; 
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<HRMDbContext>()
            .AddDefaultTokenProviders();

            // 3. Configure Cookie Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "HRM.Auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.SlidingExpiration = true;
                    options.LoginPath = "/api/auth/unauthorized";
                    options.AccessDeniedPath = "/api/auth/forbidden";
                });

            builder.Services.AddAuthorization();

            // 4. Configure CORS to allow credentials
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowUI", policy =>
                {
                    policy.WithOrigins("http://localhost:5229") // HRM_UI 
                          .AllowCredentials()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //app.UseSwagger();
                //app.UseSwaggerUI();
            }

            app.UseCors("AllowUI");
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
