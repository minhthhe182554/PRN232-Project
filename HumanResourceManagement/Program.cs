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
            // builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //     .AddCookie(options =>
            //     {
            //         options.Cookie.Name = "HRM.Auth";
            //         options.Cookie.HttpOnly = true;
            //         options.Cookie.SameSite = SameSiteMode.Lax;
            //         options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            //         options.SlidingExpiration = true;
            //         options.Events = new CookieAuthenticationEvents
            //         {
            //             OnRedirectToLogin = ctx =>
            //             {
            //                 ctx.Response.Clear();
            //                 ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //                 return Task.CompletedTask;
            //             },
            //             OnRedirectToAccessDenied = ctx =>
            //             {
            //                 ctx.Response.Clear();
            //                 ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            //                 return Task.CompletedTask;
            //             }
            //         };
            //     });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "HRM.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                        {
                            ctx.Response.Clear();
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }
                        ctx.Response.Redirect(ctx.RedirectUri);
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                        {
                            ctx.Response.Clear();
                            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }
                        ctx.Response.Redirect(ctx.RedirectUri);
                        return Task.CompletedTask;
                    }
                };
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

            // 5. Configure AutoMapper
            builder.Services.AddAutoMapper(cfg => { cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzkyMDIyNDAwIiwiaWF0IjoiMTc2MDUwMzkyNCIsImFjY291bnRfaWQiOiIwMTk5ZTYzNTY0YTM3YjBmYTU1YTgxY2NiNTNkZjYyNCIsImN1c3RvbWVyX2lkIjoiY3RtXzAxazdrM2MxZjVrNGJuZnB0N2ZiemFjNjJqIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.AS58W9pznoRsHFUyVr3hKj73H3RIBAshV5tgO1_9EhSBJIFwwjFsZO9XJDAE-O6DJbYAdfTkGk4a8kTLQo85YpgwmU0IdVBU6rDDvnuh1kD63U_r3xUUO0KNyz1uvvufb0TR8Tz5MYJAc7TjgLv3MBYdFB5M1sVifDMq5-UEROxLoVPsW2HIC7cpAq5UsL5K03p1PU07S7apMp5sFavvAEfDRhq55tjJ8-JafidCsNdlrhdPKFcoeGtGdGK4WF96IzVU6nGCzAGOL5NUWb_Rkl_LTOquXwEhxkaFKqg0gdkoxo_y1rq5Ftu3p14aAHvEXZK5a4K7ISsdU6tQo8MsMA"; }, AppDomain.CurrentDomain.GetAssemblies());

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
