using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HumanResourceManagement.Models
{
    public class HRMDbContext : IdentityDbContext<User>
    {
        public HRMDbContext(DbContextOptions<HRMDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Department> Departments { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<OvertimeRequest> OvertimeRequests { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<SharedFile> SharedFiles { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Address)
                    .HasMaxLength(200);

                entity.Property(e => e.ProfileImgUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.Salary)
                    .HasColumnType("decimal(18,2)");

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

                // Index for performance
                entity.HasIndex(a => new { a.UserId, a.CheckIn })
                    .HasDatabaseName("IX_Attendance_UserId_CheckIn");
            });

            // LeaveRequest entity configuration
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.Property(e => e.Reason)
                    .HasMaxLength(500);

                entity.Property(e => e.Status)
					.HasConversion<string>() //map enum valuue
                    .HasDefaultValue(RequestStatus.Pending);

                // User - LeaveRequest relationship
                entity.HasOne(lr => lr.User)
                    .WithMany(u => u.LeaveRequests)
                    .HasForeignKey(lr => lr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index for performance
                entity.HasIndex(lr => new { lr.UserId, lr.Status })
                    .HasDatabaseName("IX_LeaveRequest_UserId_Status");
            });

            // OvertimeRequest entity configuration
            modelBuilder.Entity<OvertimeRequest>(entity =>
            {
                entity.Property(e => e.Reason)
                    .HasMaxLength(500);

                entity.Property(e => e.Status)
					.HasConversion<string>() //map enum valuue
                    .HasDefaultValue(RequestStatus.Pending);

                entity.Property(e => e.Hours)
                    .IsRequired();

                // User - OvertimeRequest relationship
                entity.HasOne(or => or.User)
                    .WithMany(u => u.OvertimeRequests)
                    .HasForeignKey(or => or.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Holiday entity configuration
            modelBuilder.Entity<Holiday>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Date)
                    .IsRequired();

                // Index for date queries
                entity.HasIndex(h => h.Date)
                    .HasDatabaseName("IX_Holiday_Date");
            });

            // CompanySettings configuration (single-row table)
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.Property(e => e.WorkStartTime)
                    .IsRequired();

                entity.Property(e => e.WorkEndTime)
                    .IsRequired();

                entity.Property(e => e.LateEarlyThresholdMinutes)
                    .IsRequired();

                entity.Property(e => e.LateEarlyDeductionPercent)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                entity.Property(e => e.MonthlyOvertimeHoursLimit)
                    .IsRequired();

                entity.Property(e => e.AnnualLeaveMaxDays)
                    .IsRequired();

                // Seed default single row with Id = 1
                entity.HasData(new Policy
                {
                    Id = 1,
                    WorkStartTime = new TimeSpan(9, 0, 0),
                    WorkEndTime = new TimeSpan(17, 0, 0),
                    LateEarlyThresholdMinutes = 15,
                    LateEarlyDeductionPercent = 10m,
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
                    .HasDefaultValueSql("GETUTCDATE()");

                // Department - SharedFile relationship
                entity.HasOne(sf => sf.Department)
                    .WithMany(d => d.SharedFiles)
                    .HasForeignKey(sf => sf.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // User - SharedFile relationship (UploadedBy)
                entity.HasOne(sf => sf.UploadedBy)
                    .WithMany()
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
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsRead)
                    .HasDefaultValue(false);

                // User - Notification relationship
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index for performance
                entity.HasIndex(n => new { n.UserId, n.IsRead })
                    .HasDatabaseName("IX_Notification_UserId_IsRead");
            });
        }
    }
}
