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
builder.Services.AddScoped<DepartmentRepository>();
builder.Services.AddScoped<RequestRepository>();
builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PolicyRepository>();
builder.Services.AddScoped<PolicyService>();
builder.Services.AddScoped<SalaryScaleService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ManagerService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<PerformanceEvaluationService>();
builder.Services.AddHttpClient(); // Register IHttpClientFactory
builder.Services.AddScoped<GeminiService>();

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

// Configure request timeout for long-running operations
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
});

// Configure request timeout policies for long-running operations
builder.Services.AddRequestTimeouts(options =>
{
    options.AddPolicy("LongRunning", TimeSpan.FromMinutes(10));
});

var app = builder.Build();

// // Seed initial users data
// await SeedData(app);

// Configure the HTTP request pipeline.

app.UseCors("AllowClient");

app.UseRequestTimeouts(); // Enable request timeout middleware

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Seed data function
static async Task SeedData(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HRMDbContext>();
    
    // Clear existing data - clear foreign keys first
    // await context.Database.ExecuteSqlRawAsync("DELETE FROM SharedFiles");
    // await context.Database.ExecuteSqlRawAsync("DELETE FROM Notifications");
    // await context.Database.ExecuteSqlRawAsync("DELETE FROM Requests");
    // await context.Database.ExecuteSqlRawAsync("DELETE FROM Attendances");
    
    // Set DepartmentId to NULL for Users before deleting
    // await context.Database.ExecuteSqlRawAsync("UPDATE Users SET DepartmentId = NULL");
    
    // Delete Departments and Users
    // await context.Database.ExecuteSqlRawAsync("DELETE FROM Departments");
    // await context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM SalaryScales");
    
    // Reset identity to 1
    // await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Users', RESEED, 0)");
    // await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Departments', RESEED, 0)");
    await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('SalaryScales', RESEED, 0)");

    // Seed SalaryScales
    var salaryScales = new List<SalaryScale>
    {
        // Admin - Level 1 only
        new SalaryScale { Role = Role.Admin, Level = 1, BaseSalary = 50000000m, Description = "Admin Level 1" },
        
        // Manager - Level 1, 2, 3
        new SalaryScale { Role = Role.Manager, Level = 1, BaseSalary = 30000000m, Description = "Manager Level 1" },
        new SalaryScale { Role = Role.Manager, Level = 2, BaseSalary = 40000000m, Description = "Manager Level 2" },
        new SalaryScale { Role = Role.Manager, Level = 3, BaseSalary = 50000000m, Description = "Manager Level 3" },
        
        // Employee - Level 1, 2, 3
        new SalaryScale { Role = Role.Employee, Level = 1, BaseSalary = 15000000m, Description = "Employee Level 1" },
        new SalaryScale { Role = Role.Employee, Level = 2, BaseSalary = 20000000m, Description = "Employee Level 2" },
        new SalaryScale { Role = Role.Employee, Level = 3, BaseSalary = 25000000m, Description = "Employee Level 3" }
    };
    
    await context.SalaryScales.AddRangeAsync(salaryScales);
    await context.SaveChangesAsync();

    // var random = new Random();
    // var addresses = new[] { "Ha Noi", "Ho Chi Minh", "Da Nang", "Hai Phong", "Can Tho", "Nha Trang", "Hue", "Vung Tau" };

    // // 1. Create Admin
    // var admin = new User
    // {
    //     Username = "minhth1",
    //     Password = PasswordService.HashPassword("mothaiba1"),
    //     Role = Role.Admin,
    //     Level = 1,
    //     FullName = "Thanh Hung Minh",
    //     Address = $"{addresses[0]}, Vietnam",
    //     ProfileImgUrl = "default-url",
    //     AnnualLeaveDays = 12,
    //     IsActive = true
    // };

    // await context.Users.AddAsync(admin);
    // await context.SaveChangesAsync();

    // // 2. Create 5 Managers with generated usernames
    // var managerNames = new[]
    // {
    //     new { FirstName = "Huy", MiddleName = "Nguyen Van", Address = addresses[1] },
    //     new { FirstName = "Linh", MiddleName = "Nguyen Thi", Address = addresses[2] },
    //     new { FirstName = "Duc", MiddleName = "Pham Van", Address = addresses[3] },
    //     new { FirstName = "Anh", MiddleName = "Tran Minh", Address = addresses[4] },
    //     new { FirstName = "Lan", MiddleName = "Le Thi", Address = addresses[5] }
    // };

    // var managers = new List<User>();
    // var usernameCountMap = new Dictionary<string, int>();

    // foreach (var name in managerNames)
    // {
    //     var baseUsername = UsernameGenerator.GenerateBaseUsername(name.FirstName, name.MiddleName);
        
    //     // Generate unique username
    //     if (!usernameCountMap.ContainsKey(baseUsername))
    //     {
    //         usernameCountMap[baseUsername] = 0;
    //     }
    //     usernameCountMap[baseUsername]++;
    //     var username = baseUsername + usernameCountMap[baseUsername];

    //     var manager = new User
    //     {
    //         Username = username,
    //         Password = PasswordService.HashPassword("mothaiba1"),
    //         Role = Role.Manager,
    //         Level = random.Next(1, 4),
    //         FullName = $"{name.MiddleName} {name.FirstName}",
    //         Address = $"{name.Address}, Vietnam",
    //         ProfileImgUrl = "default-url",
    //         AnnualLeaveDays = 12,
    //         IsActive = true
    //     };

    //     managers.Add(manager);
    // }

    // await context.Users.AddRangeAsync(managers);
    // await context.SaveChangesAsync();

    // // 3. Create 5 Departments with correct ManagerId from the start
    // var departmentNames = new[]
    // {
    //     "Finance Department",
    //     "Information Technology (IT) Department",
    //     "Sales and Marketing Department",
    //     "Human Resources Department",
    //     "Operations Department"
    // };

    // var departments = new List<Department>();
    // for (int i = 0; i < 5; i++)
    // {
    //     var dept = new Department
    //     {
    //         Name = departmentNames[i],
    //         ManagerId = managers[i].Id
    //     };
    //     departments.Add(dept);
    // }

    // await context.Departments.AddRangeAsync(departments);
    // await context.SaveChangesAsync();

    // // 4. Update managers' DepartmentId using raw SQL to avoid EF tracking issues
    // for (int i = 0; i < 5; i++)
    // {
    //     await context.Database.ExecuteSqlRawAsync(
    //         $"UPDATE Users SET DepartmentId = {departments[i].Id} WHERE Id = {managers[i].Id}");
    // }

    // // 6. Create 50 Employees with random levels and departments
    // var employeeFirstNames = new[]
    // {
    //     "An", "Binh", "Chi", "Dung", "Em", "Giang", "Ha", "Hoa", "Khanh", "Khoi",
    //     "Linh", "Mai", "Nam", "Nga", "Oanh", "Phuong", "Quang", "Son", "Tam", "Thao",
    //     "Trang", "Trinh", "Tuan", "Van", "Vy", "Xuan", "Yen", "Bao", "Cuong", "Dat",
    //     "Duy", "Duc", "Hai", "Hieu", "Hoang", "Hung", "Kien", "Long", "Manh", "Minh",
    //     "Nhan", "Phong", "Quan", "Thai", "Thanh", "Thong", "Tien", "Vinh", "Vuong", "Yen"
    // };

    // var employeeMiddleNames = new[]
    // {
    //     "Nguyen Van", "Tran Thi", "Le Van", "Pham Minh", "Hoang Thi",
    //     "Vu Van", "Dang Thi", "Bui Van", "Do Minh", "Ngo Thi",
    //     "Duong Van", "Ly Thi", "Trinh Van", "Vo Minh", "Truong Thi"
    // };

    // var employees = new List<User>();
    
    // for (int i = 0; i < 50; i++)
    // {
    //     var firstName = employeeFirstNames[i];
    //     var middleName = employeeMiddleNames[random.Next(employeeMiddleNames.Length)];
    //     var baseUsername = UsernameGenerator.GenerateBaseUsername(firstName, middleName);
        
    //     // Generate unique username
    //     if (!usernameCountMap.ContainsKey(baseUsername))
    //     {
    //         usernameCountMap[baseUsername] = 0;
    //     }
    //     usernameCountMap[baseUsername]++;
    //     var username = baseUsername + usernameCountMap[baseUsername];

    //     var employee = new User
    //     {
    //         Username = username,
    //         Password = PasswordService.HashPassword("mothaiba1"),
    //         Role = Role.Employee,
    //         Level = random.Next(1, 4), // Random level 1-3
    //         FullName = $"{middleName} {firstName}",
    //         Address = $"{addresses[random.Next(addresses.Length)]}, Vietnam",
    //         ProfileImgUrl = "default-url",
    //         AnnualLeaveDays = 12,
    //         IsActive = true,
    //         DepartmentId = departments[random.Next(departments.Count)].Id // Random department
    //     };

    //     employees.Add(employee);
    // }

    // await context.Users.AddRangeAsync(employees);
    // await context.SaveChangesAsync();

    Console.WriteLine($"✓ Seeded 7 Salary Scales (Admin: 1, Manager: 3, Employee: 3)");
}
