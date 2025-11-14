using HRM_Client.Middleware;
using HRM_Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// HttpContext accessor
builder.Services.AddHttpContextAccessor();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpClient configuration
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromMinutes(10); // Increase timeout for long operations
});

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromMinutes(10); // Increase timeout for long operations
});

builder.Services.AddScoped<PolicyService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SalaryScaleService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ManagerService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<PerformanceEvaluationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

app.UseStaticFiles();

app.UseRouting();

// Custom API error handler middleware
app.UseMiddleware<ApiErrorHandlerMiddleware>();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
