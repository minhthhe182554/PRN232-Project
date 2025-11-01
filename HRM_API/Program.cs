using System.Text;
using System.Text.Json.Serialization;
using HRM_API.Models;
using HRM_API.Models.Enums;
using HRM_API.Repositories;
using HRM_API.Services;
using HRM_API.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<HRMDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<SalaryScaleRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PolicyRepository>();
builder.Services.AddScoped<PolicyService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("http://localhost:5050", "https://localhost:7050")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers().AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

// Seed initial users data
// await SeedData(app);

// Configure the HTTP request pipeline.

app.UseCors("AllowClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Seed data function
static async Task SeedData(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HRMDbContext>();
    
    // Clear existing data and reset identity
    await context.Database.ExecuteSqlRawAsync("DELETE FROM SharedFiles");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Notifications");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Requests");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Attendances");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Departments");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
    
    // Reset identity to 1
    await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Users', RESEED, 0)");
    await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Departments', RESEED, 0)");

    var admin = new User
    {
        Username = "minhth1",
        Password = PasswordService.HashPassword("Admin@123"),
        Role = Role.Admin,
        Level = 1,
        FullName = "Thanh Hung Minh",
        Address = "Ha Noi, Vietnam",
        ProfileImgUrl = "default-url",
        AnnualLeaveDays = 12,
        IsActive = true
    };

    var manager1 = new User
    {
        Username = "huynv1",
        Password = PasswordService.HashPassword("Manager@123"),
        Role = Role.Manager,
        Level = 2,
        FullName = "Nguyen Van Huy",
        Address = "Ho Chi Minh, Vietnam",
        ProfileImgUrl = "default-url",
        AnnualLeaveDays = 12,
        IsActive = true
    };

    var manager2 = new User
    {
        Username = "linhnt1",
        Password = PasswordService.HashPassword("Manager@123"),
        Role = Role.Manager,
        Level = 1,
        FullName = "Nguyen Thi Linh",
        Address = "Da Nang, Vietnam",
        ProfileImgUrl = "default-url",
        AnnualLeaveDays = 12,
        IsActive = true
    };

    var manager3 = new User
    {
        Username = "ducpham1",
        Password = PasswordService.HashPassword("Manager@123"),
        Role = Role.Manager,
        Level = 2,
        FullName = "Pham Van Duc",
        Address = "Can Tho, Vietnam",
        ProfileImgUrl = "default-url",
        AnnualLeaveDays = 12,
        IsActive = true
    };

    var employee = new User
    {
        Username = "ronaldocr1",
        Password = PasswordService.HashPassword("Employee@123"),
        Role = Role.Employee,
        Level = 2,
        FullName = "Cristiano Ronaldo",
        Address = "Ha Long, Vietnam",
        ProfileImgUrl = "default-url",
        AnnualLeaveDays = 12,
        IsActive = true
    };

    await context.Users.AddRangeAsync(new[] { admin, manager1, manager2, manager3, employee });
    await context.SaveChangesAsync();

    // Seed departments
    var departments = new List<Department>
    {
        new Department
        {
            Name = "Finance Department",
            ManagerId = manager1.Id
        },
        new Department
        {
            Name = "Information Technology (IT) Department",
            ManagerId = manager2.Id
        },
        new Department
        {
            Name = "Sales and Marketing Department",
            ManagerId = manager3.Id
        }
    };

    await context.Departments.AddRangeAsync(departments);
    await context.SaveChangesAsync();
}
