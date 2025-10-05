== SEED DATABASE ==

- mở project lên (cái API) thì ông đừng run luôn, phải tạo db bằng code first đã.
- dùng cái lệnh: "dotnet ef database update" để tạo db từ code-first
- nếu tạo thành công thì từ từ hẵng run
- dùng đoạn code dưới đây của tôi cho haàm main trong Program.cs để ông tạo trước các data mẫu luôn

// 🌱 SEED DATA MAIN - CHỈ CHẠY 1 LẦN ĐẦU ĐỂ KHỞI TẠO DATA MẪU
// Sau khi seed xong, comment lại và uncomment Main cũ bên trên
public static async Task Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // 1. Configure Database Connection
    builder.Services.AddDbContext<HRMDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("HRM_Db")));

    // 2. Configure Identity (với RequireConfirmedEmail = false cho seed)
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

        // User settings
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        // ⚠️ Email confirmation DISABLED cho seed data
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
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
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
            policy.WithOrigins("http://localhost:5229", "https://localhost:7229")
                  .AllowCredentials()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    // 🌱🌱🌱 SEED DATABASE 🌱🌱🌱
    Console.WriteLine("========================================");
    Console.WriteLine("🌱 STARTING DATABASE SEEDING...");
    Console.WriteLine("========================================");
    
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            await SeedDatabase(services);
            Console.WriteLine("========================================");
            Console.WriteLine("✅ DATABASE SEEDING COMPLETED!");
            Console.WriteLine("========================================");
            Console.WriteLine("");
            Console.WriteLine("📝 NEXT STEPS:");
            Console.WriteLine("1. Stop the application (Ctrl+C)");
            Console.WriteLine("2. Open Program.cs");
            Console.WriteLine("3. Comment this Main function");
            Console.WriteLine("4. Uncomment the original Main function");
            Console.WriteLine("5. Run the application normally");
            Console.WriteLine("");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "❌ An error occurred while seeding the database.");
            Console.WriteLine($"❌ ERROR: {ex.Message}");
        }
    }

    // Không start app, chỉ seed data rồi thoát
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

/// <summary>
/// Seed initial data for development
/// </summary>
private static async Task SeedDatabase(IServiceProvider services)
{
    var context = services.GetRequiredService<HRMDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // 1. SEED ROLES (Identity không tự tạo roles)
    await SeedRoles(roleManager);

    // 2. SEED USERS
    var (admin, manager, employee) = await SeedUsers(userManager);

    // 3. SEED DEPARTMENTS
    await SeedDepartments(context, manager);

    // 4. SEED HOLIDAYS
    await SeedHolidays(context);

    // 5. SEED POLICIES
    await SeedPolicies(context);

    await context.SaveChangesAsync();
}

/// <summary>
/// Seed Roles - QUAN TRỌNG: Identity không tự tạo roles
/// </summary>
private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
{
    Console.WriteLine("\n📋 Seeding Roles...");
    string[] roles = { "Admin", "Manager", "Employee" };

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            Console.WriteLine($"   ✓ Created role: {roleName}");
        }
        else
        {
            Console.WriteLine($"   ⊙ Role already exists: {roleName}");
        }
    }
}

/// <summary>
/// Seed demo users with different roles
/// </summary>
private static async Task<(User admin, User manager, User employee)> SeedUsers(UserManager<User> userManager)
{
    Console.WriteLine("\n👥 Seeding Users...");
    
    // Admin User
    var admin = await userManager.FindByEmailAsync("admin@hrm.com");
    if (admin == null)
    {
        admin = new User
        {
            UserName = "admin@hrm.com",
            Email = "admin@hrm.com",
            FullName = "System Administrator",
            Address = "123 Admin Street, Hanoi, Vietnam",
            PhoneNumber = "0123456789",
            Salary = 50000000,
            AnnualLeaveDays = 15,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            Console.WriteLine("   ✓ Created user: admin@hrm.com / Admin@123 [Role: Admin]");
        }
        else
        {
            Console.WriteLine($"   ✗ Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        Console.WriteLine("   ⊙ User already exists: admin@hrm.com");
    }

    // Manager User
    var manager = await userManager.FindByEmailAsync("manager@hrm.com");
    if (manager == null)
    {
        manager = new User
        {
            UserName = "manager@hrm.com",
            Email = "manager@hrm.com",
            FullName = "John Manager",
            Address = "456 Manager Avenue, Hanoi, Vietnam",
            PhoneNumber = "0987654321",
            Salary = 30000000,
            AnnualLeaveDays = 12,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(manager, "Manager@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(manager, "Manager");
            Console.WriteLine("   ✓ Created user: manager@hrm.com / Manager@123 [Role: Manager]");
        }
        else
        {
            Console.WriteLine($"   ✗ Failed to create manager: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        Console.WriteLine("   ⊙ User already exists: manager@hrm.com");
    }

    // Employee User
    var employee = await userManager.FindByEmailAsync("employee@hrm.com");
    if (employee == null)
    {
        employee = new User
        {
            UserName = "employee@hrm.com",
            Email = "employee@hrm.com",
            FullName = "Jane Employee",
            Address = "789 Employee Road, Hanoi, Vietnam",
            PhoneNumber = "0369852147",
            Salary = 15000000,
            AnnualLeaveDays = 12,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(employee, "Employee@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(employee, "Employee");
            Console.WriteLine("   ✓ Created user: employee@hrm.com / Employee@123 [Role: Employee]");
        }
        else
        {
            Console.WriteLine($"   ✗ Failed to create employee: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        Console.WriteLine("   ⊙ User already exists: employee@hrm.com");
    }

    return (admin!, manager!, employee!);
}

/// <summary>
/// Seed departments
/// </summary>
private static async Task SeedDepartments(HRMDbContext context, User manager)
{
    Console.WriteLine("\n🏢 Seeding Departments...");
    
    if (!context.Departments.Any())
    {
        var departments = new[]
        {
            new Department { Name = "Information Technology", ManagerId = manager.Id },
            new Department { Name = "Human Resources", ManagerId = manager.Id },
            new Department { Name = "Finance", ManagerId = manager.Id },
            new Department { Name = "Marketing", ManagerId = manager.Id },
            new Department { Name = "Sales", ManagerId = manager.Id }
        };

        await context.Departments.AddRangeAsync(departments);
        Console.WriteLine($"   ✓ Created {departments.Length} departments");
    }
    else
    {
        Console.WriteLine("   ⊙ Departments already exist");
    }
}

/// <summary>
/// Seed holidays
/// </summary>
private static async Task SeedHolidays(HRMDbContext context)
{
    Console.WriteLine("\n🎉 Seeding Holidays...");
    
    if (!context.Holidays.Any())
    {
        var currentYear = DateTime.Now.Year;
        var holidays = new[]
        {
            new Holiday { Name = "New Year's Day", Date = new DateTime(currentYear, 1, 1) },
            new Holiday { Name = "Tet Holiday (Day 1)", Date = new DateTime(currentYear, 2, 10) },
            new Holiday { Name = "Tet Holiday (Day 2)", Date = new DateTime(currentYear, 2, 11) },
            new Holiday { Name = "Tet Holiday (Day 3)", Date = new DateTime(currentYear, 2, 12) },
            new Holiday { Name = "Hung Kings' Festival", Date = new DateTime(currentYear, 4, 18) },
            new Holiday { Name = "Reunification Day", Date = new DateTime(currentYear, 4, 30) },
            new Holiday { Name = "International Labor Day", Date = new DateTime(currentYear, 5, 1) },
            new Holiday { Name = "National Day", Date = new DateTime(currentYear, 9, 2) }
        };

        await context.Holidays.AddRangeAsync(holidays);
        Console.WriteLine($"   ✓ Created {holidays.Length} holidays");
    }
    else
    {
        Console.WriteLine("   ⊙ Holidays already exist");
    }
}

/// <summary>
/// Seed company policies
/// </summary>
private static async Task SeedPolicies(HRMDbContext context)
{
    Console.WriteLine("\n📜 Seeding Policies...");
    
    if (!context.Policies.Any())
    {
        var policies = new[]
        {
            new Policy { Name = "WorkingHoursPerDay", Value = "8" },
            new Policy { Name = "WorkingDaysPerWeek", Value = "5" },
            new Policy { Name = "AnnualLeaveDays", Value = "12" },
            new Policy { Name = "OvertimeRateWeekday", Value = "1.5" },
            new Policy { Name = "OvertimeRateWeekend", Value = "2.0" },
            new Policy { Name = "LateArrivalGracePeriodMinutes", Value = "15" }
        };

        await context.Policies.AddRangeAsync(policies);
        Console.WriteLine($"   ✓ Created {policies.Length} policies");
    }
    else
    {
        Console.WriteLine("   ⊙ Policies already exist");
    }
}

- chạy xong lần đầu nếu đọc cái log console, thành công thì ông bỏ đi để lại cái hàm main trong Program.cs cũ của tôi