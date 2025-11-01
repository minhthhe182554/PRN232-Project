using HRM_API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Models
{
    public class HRMDbContext : DbContext
    {
        public HRMDbContext(DbContextOptions<HRMDbContext> options) : base(options) { }
        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<SharedFile> SharedFiles { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SalaryScale> SalaryScales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.Property(e => e.Role)
                    .HasConversion<string>();

                entity.Property(e => e.Password)
                    .IsRequired();
                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Address)
                    .HasMaxLength(200);

                entity.Property(e => e.ProfileImgUrl)
                    .HasMaxLength(500)
                    .HasDefaultValue("default-url");

                entity.Property(e => e.Level)
                    .HasDefaultValue(1);

                entity.Property(e => e.AnnualLeaveDays)
                    .HasDefaultValue(12); // default number of leave days in a year

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
            });

            // Department entity configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                // Department - Manager relationship (1:1)
                entity.HasOne(d => d.Manager)
                    .WithOne(u => u.ManagedDepartment)
                    .HasForeignKey<Department>(d => d.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Department - Employees relationship (1:Many) 
                entity.HasMany(d => d.Employees)
                    .WithOne(u => u.Department)
                    .HasForeignKey(u => u.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict); // Cannot delete Department if there is an emp in this Department
            });

            // Attendance entity configuration
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.Property(e => e.CheckIn)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasConversion<string>() //map enum valuue
                    .HasDefaultValue(AttendanceStatus.Present);

                // User - Attendance relationship
                entity.HasOne(a => a.User)
                    .WithMany(u => u.Attendances)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // LeaveRequest entity configuration
            modelBuilder.Entity<Request>(entity =>
            {
                entity.Property(e => e.Type).HasConversion<string>();

                entity.Property(e => e.Content)
                    .HasMaxLength(500);

                entity.Property(e => e.Status)
                    .HasConversion<string>() //map enum valuue
                    .HasDefaultValue(RequestStatus.Pending);

                // User - Request relationship
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Requests)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

            });

            // Policy configuration (single-row table)
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.Property(e => e.WorkStartTime)
                    .IsRequired();

                entity.Property(e => e.WorkEndTime)
                    .IsRequired();

                entity.Property(e => e.LateThresholdMinutes)
                    .IsRequired();

                entity.Property(e => e.LateDeductionPercent)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                entity.Property(e => e.LeaveEarlyThresholdMinutes)
                    .IsRequired();

                entity.Property(e => e.LeaveEarlyDeductionPercent)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                entity.Property(e => e.MonthlyOvertimeHoursLimit)
                    .IsRequired();

                entity.Property(e => e.AnnualLeaveMaxDays)
                    .IsRequired();

                // Seed default policy
                entity.HasData(new Policy
                {
                    Id = 1,
                    WorkStartTime = new TimeSpan(9, 0, 0),  // 09:00
                    WorkEndTime = new TimeSpan(17, 0, 0),   // 17:00
                    LateThresholdMinutes = 15,
                    LateDeductionPercent = 10m,
                    LeaveEarlyThresholdMinutes = 15,
                    LeaveEarlyDeductionPercent = 10m,
                    MonthlyOvertimeHoursLimit = 40,
                    AnnualLeaveMaxDays = 12
                });
            });

            // SharedFile entity configuration
            modelBuilder.Entity<SharedFile>(entity =>
            {
                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FileUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.UploadedAt)
                    .IsRequired();

                // Department - SharedFile relationship
                entity.HasOne(sf => sf.Department)
                    .WithMany(d => d.SharedFiles)
                    .HasForeignKey(sf => sf.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // User - SharedFile relationship (UploadedBy)
                entity.HasOne(sf => sf.UploadedBy)
                    .WithMany(u => u.SharedFiles)
                    .HasForeignKey(sf => sf.UploadedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification entity configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.IsRead)
                    .HasDefaultValue(false);

                // User - Notification relationship
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SalaryScale entity configuration
            modelBuilder.Entity<SalaryScale>(entity =>
            {
                entity.Property(e => e.Role)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(e => e.Level)
                    .IsRequired();

                entity.Property(e => e.BaseSalary)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(200);

                // Unique constraint: Role + Level combination
                entity.HasIndex(e => new { e.Role, e.Level })
                    .IsUnique();

                // Seed salary scales
                entity.HasData(
                    // Admin - Level 1 only
                    new SalaryScale { Id = 1, Role = Role.Admin, Level = 1, BaseSalary = 50000000m, Description = "Admin Level 1" },
                    
                    // Manager - Level 1, 2
                    new SalaryScale { Id = 2, Role = Role.Manager, Level = 1, BaseSalary = 30000000m, Description = "Manager Level 1" },
                    new SalaryScale { Id = 3, Role = Role.Manager, Level = 2, BaseSalary = 40000000m, Description = "Manager Level 2" },
                    
                    // Employee - Level 1, 2, 3
                    new SalaryScale { Id = 4, Role = Role.Employee, Level = 1, BaseSalary = 15000000m, Description = "Employee Level 1" },
                    new SalaryScale { Id = 5, Role = Role.Employee, Level = 2, BaseSalary = 20000000m, Description = "Employee Level 2" },
                    new SalaryScale { Id = 6, Role = Role.Employee, Level = 3, BaseSalary = 25000000m, Description = "Employee Level 3" }
                );
            });
        }
    }
}
