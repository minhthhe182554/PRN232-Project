# PRN232 HRM System - Project Context

## Overview

Human Resource Management System với kiến trúc API-Client tách biệt.
- API: ASP.NET Core Web API (.NET 8)
- Client: ASP.NET Core MVC (.NET 8)
- Database: SQL Server + Entity Framework Core
- Auth: JWT Bearer Token

## Architecture

### Technology Stack
- ASP.NET Core 8.0
- Entity Framework Core (Code First)
- SQL Server
- BCrypt password hashing
- JWT authentication
- Session-based state management

### Project Structure
```
PRN232-Project/
├── HRM_API/
│   ├── Controllers/        # API endpoints
│   ├── Services/           # Business logic
│   ├── Repositories/       # Data access
│   ├── Models/             # Entity models + Enums
│   ├── Dtos/              # Data transfer objects
│   ├── Utils/             # Utilities
│   └── Migrations/        # EF migrations
│
└── HRM_Client/
    ├── Controllers/        # MVC controllers
    ├── Services/          # API client services
    ├── Models/            # View models & DTOs
    ├── Views/             # Razor views
    ├── Middleware/        # Custom middleware
    └── wwwroot/          # Static files
```

## Database Schema

### Core Entities

**User**
- Id (PK)
- Username (unique, required)
- Password (BCrypt hashed)
- Role (enum: Admin=1, Manager=2, Employee=3)
- Level (1-3)
- FullName, Address (required)
- ProfileImgUrl (nullable)
- AnnualLeaveDays (default: 12)
- IsActive (soft delete, default: true)
- DepartmentId (FK, nullable)

**Department**
- Id (PK)
- Name (required)
- ManagerId (FK, required)

**SalaryScale**
- Id (PK)
- Role (enum)
- Level (int)
- BaseSalary (decimal(18,2))
- Description (nullable)
- Unique: (Role, Level)

**Policy**
- Id (PK)
- WorkStartTime (TimeSpan)
- WorkEndTime (TimeSpan)
- LateThresholdMinutes (int)
- LateDeductionPercent (decimal(5,2))
- LeaveEarlyThresholdMinutes (int)
- LeaveEarlyDeductionPercent (decimal(5,2))
- MonthlyOvertimeHoursLimit (int)
- AnnualLeaveMaxDays (int)

**Attendance**
- Id (PK)
- UserId (FK)
- CheckIn (DateTime)
- CheckOut (DateTime, nullable)
- Status (enum: Present=1, Late=2, LeaveEarly=3, Absent=4)

**Request**
- Id (PK)
- UserId (FK)
- DepartmentId (FK, nullable)
- Type (enum: Leave=1, Resignation=2, Overtime=3)
- StartDate, EndDate, Content
- Status (enum: Pending=1, Approved=2, Rejected=3)

**Notification**
- Id (PK)
- UserId (FK)
- Message, IsRead (default: false)
- CreatedAt

**SharedFile**
- Id (PK)
- FileName, FileUrl
- DepartmentId (FK)
- UploadedById (FK)
- UploadedAt

### Relationships
- User → Department (Many-to-One)
- Department → User (Manager, One-to-One)
- User → Attendance/Request/Notification (One-to-Many)
- Department → SharedFile (One-to-Many)

### Key DTOs

**UserListDto**
- Id, Role, Level, FullName, IsActive
- DepartmentName (nullable)
- ManagedDepartmentName (nullable)
- TodayAttendanceStatus (nullable, enum: Present/Late/LeaveEarly/Absent)

**RegisterRequest/Response**
- Request: FirstName, MiddleName, Address, Role, Level, DepartmentId
- Response: UserId, Username, Password (plain), FullName, Role

**DashboardStatsResponse (Admin)**
- RequestStats (Today/ThisWeek): Total, Pending, Approved, Rejected + percentages
- AttendanceStats (Today/ThisWeek): Total, Present, Absent, Late, OnLeave + percentages

**ManagerDashboardStatsResponse**
- CheckInStatus: HasCheckedInToday, CheckInTime, IsCheckInOnTime, CheckInMessage, HasCheckedOutToday, CheckOutTime, IsCheckOutOnTime, CheckOutMessage
- TeamStats: TotalMembers, Level1/2/3Count
- RequestStats: Same as Admin but department-scoped
- AttendanceStats: Same as Admin but department-scoped
- TopAbsentees: List of { UserId, FullName, Level, AbsentCount }

**EmployeeDashboardStatsResponse**
- CheckInStatus: HasCheckedInToday, CheckInTime, IsCheckInOnTime, CheckInMessage, HasCheckedOutToday, CheckOutTime, IsCheckOutOnTime, CheckOutMessage

**AttendanceHistoryResponse**
- Period: "Last Week" or "Last Month"
- Records: List of AttendanceRecordDto

**AttendanceRecordDto**
- Id, Date, CheckIn, CheckOut (nullable), Status (string), WorkingHours (nullable TimeSpan)

**RequestResponseDto**
- Id, UserId, UserFullName, UserLevel, Type (RequestType), StartDate, EndDate, Content, Status (RequestStatus), CreatedAt (nullable), ProcessedAt (nullable)

**RequestListResponse**
- Requests (List<RequestResponseDto>), Count

**UpdateRequestStatusRequest**
- RequestId, Status (RequestStatus)

**CreateRequestDto**
- Type (RequestType), StartDate, EndDate, Content

**IncomeStatisticsResponse**
- Today (IncomeTodayDto), Monthly (IncomeMonthlyDto)

**IncomeTodayDto**
- BaseSalary, DailySalary, AttendanceStatus (string), DeductionAmount, FinalSalary, DeductionReason (string)

**IncomeMonthlyDto**
- BaseSalary, TotalWorkingDays, PresentDays, LateDays, LeaveEarlyDays, AbsentDays, TotalDeduction, FinalSalary, CurrentMonth, CurrentYear

## API Endpoints

### Auth (`/api/auth`)
- POST `/login` - User login (public)
  - Request: `{ username, password }`
  - Response: `{ token, userId, username, fullName, role, level, departmentName }`
  - Returns 401 if invalid credentials or banned

- POST `/register` - Register new user (Admin)
  - Request: `{ firstName, middleName, role, level, address, departmentId, profileImgUrl, annualLeaveDays }`
  - Response: `{ userId, username, password (plain), fullName, role, message }`
  - Auto-generates username and password

### Admin (`/api/`)
- GET `/users` - List all users (Admin)
  - Response: `{ activeUsers[], inactiveUsers[], count }`

- PATCH `/users/ban` - Ban/unban user (Admin)
  - Request: `{ userId, isBanned }`

- POST `/users/reset-password` - Reset password (Admin)
  - Request: `{ userId }`
  - Response: `{ userId, username, newPassword (plain) }`

- POST `/admin/performance/general-evaluation` - General performance evaluation (Admin)
  - No request body required
  - Aggregates performance data for all employees (Manager + Employee) in last 30 days
  - Data includes: attendance metrics (Present/Late/LeaveEarly/Absent, working hours, attendance rate), request metrics (total, approved/rejected/pending, breakdown by type)
  - Calls Gemini API to generate comprehensive text evaluation report
  - Response: `{ evaluation: string }` - Formatted text report with sections: Executive Summary, Overall Performance Analysis, Key Strengths, Areas for Improvement, Top Performers, Employees Needing Attention, Department Performance Summary, Recommendations
  - Timeout: 10 minutes (long-running operation)

### Policy (`/api/policies`)
- GET `/` - Get policies (authenticated)
- PUT `/{id}` - Update policy (Admin)

### Salary Scale (`/api/salary-scales`)
- GET `/` - List all scales (Admin)
- PUT `/{id}` - Update scale (Admin)
  - Validation: baseSalary 1,000,000-1,000,000,000

### Department (`/api/departments`)
- GET `/` - List all departments (Admin)
- GET `/{id}/employees` - List employees in department (Admin)
- GET `/search-employees?username={}&departmentId={}` - Search employees (Admin, min 2 chars)
- POST `/` - Create department (Admin)
  - Request: `{ name, managerId }`
  - Logic: Promotes employee to Manager Level 1
  - Validation: name 2-100 chars, managerId must be active Employee
- PUT `/{id}/name` - Update name (Admin)
- PUT `/{id}/manager` - Change manager (Admin)
  - Logic: Old manager → Employee Level 3, New → Manager Level 1
  - Validation: newManagerId must be active Employee in same department
- DELETE `/{id}` - Delete if empty (Admin)

### Profile (`/api/profile`)
- GET `/` - Get current user profile (authenticated)
  - Response: `{ id, username, fullName, address, profileImgUrl }`
- PUT `/` - Update profile (authenticated)
  - Request: `{ fullName, address }`

### Manager (`/api/manager`)
- GET `/dashboard/stats` - Get manager dashboard statistics (Manager)
  - Response: Check-in/check-out status (with on-time/late messages), team stats, request/attendance stats, top absentees
- POST `/check-in` - Check in for today (Manager)
  - Validates against Policy (WorkStartTime + LateThresholdMinutes)
  - Returns: message (on-time/late), isOnTime flag
  - Creates attendance record with status (Present or Late)
- POST `/check-out` - Check out for today (Manager)
  - Requires check-in first
  - Validates against Policy (WorkEndTime - LeaveEarlyThresholdMinutes)
  - Returns: message (on-time/early), isOnTime flag
  - Updates attendance record with CheckOut time and status (LeaveEarly if applicable)
- GET `/employees` - Get department employees (Manager)
  - Response: List of employees with today's attendance status
- GET `/attendance/history?period={week|month}` - Get attendance history (Manager)
  - Period: "week" (last 7 days) or "month" (last 30 days)
  - Response: List of attendance records with date, check-in, check-out, status, working hours
- GET `/attendance/search?date={yyyy-MM-dd}` - Search attendance by date (Manager)
  - Returns 404 with message "Ngày đó công ty chưa mở cửa" if not found
  - Response: Single attendance record
- GET `/requests?type={Leave|Resignation|Overtime}` - Get department requests (Manager)
  - Optional type filter: Leave, Resignation, or Overtime
  - Response: List of requests from department employees with user info
  - Only returns requests from manager's department
- PUT `/requests/{requestId}/status` - Update request status (Manager)
  - Request: `{ requestId, status }` (Approved or Rejected)
  - Validates: request belongs to manager's department, status is Pending
  - Response: success message
- GET `/income` - Get income statistics (Manager)
  - Response: Income statistics for today and current month
  - Calculates based on SalaryScale, Policy deductions, and attendance records
  - Today: daily salary with deductions for Late/LeaveEarly/Absent
  - Monthly: base salary minus total deductions from attendance violations

### Employee (`/api/employee`)
- GET `/dashboard/stats` - Get employee dashboard statistics (Employee)
  - Response: Check-in/check-out status (with on-time/late messages)
- POST `/check-in` - Check in for today (Employee)
  - Validates against Policy (WorkStartTime + LateThresholdMinutes)
  - Returns: message (on-time/late), isOnTime flag
  - Creates attendance record with status (Present or Late)
- POST `/check-out` - Check out for today (Employee)
  - Requires check-in first
  - Validates against Policy (WorkEndTime - LeaveEarlyThresholdMinutes)
  - Returns: message (on-time/early), isOnTime flag
  - Updates attendance record with CheckOut time and status (LeaveEarly if applicable)
- GET `/attendance/history?period={week|month}` - Get attendance history (Employee)
  - Period: "week" (last 7 days) or "month" (last 30 days)
  - Response: List of attendance records with date, check-in, check-out, status, working hours
- GET `/attendance/search?date={yyyy-MM-dd}` - Search attendance by date (Employee)
  - Returns 404 with message "Ngày đó công ty chưa mở cửa" if not found
  - Response: Single attendance record
- GET `/requests` - Get my requests (Employee)
  - Response: List of requests created by the employee
- POST `/requests` - Create new request (Employee)
  - Request: `{ type, startDate, endDate, content }`
  - Validates: employee must be assigned to a department, startDate <= endDate
  - Auto-assigns DepartmentId from employee's department
  - Status defaults to Pending
  - Response: success message and created request
- GET `/income` - Get income statistics (Employee)
  - Response: Income statistics for today and current month
  - Calculates based on SalaryScale, Policy deductions, and attendance records
  - Today: daily salary with deductions for Late/LeaveEarly/Absent
  - Monthly: base salary minus total deductions from attendance violations

## Authentication & Authorization

### API
- JWT Bearer Token
- Token expiration: 8 hours
- Claims: UserId, Username, Role

### Client Session
- Session timeout: 8 hours
- Cookie-based auth token (HttpOnly, Secure in production)
- Session stores: UserId, Username, FullName, Role, Level, DepartmentName

### Role-Based Access
- Admin: Full access
- Manager: Department management, request approval, income statistics
- Employee: Personal data access, check-in/check-out, attendance history, request management, income statistics

## Business Logic

### Username Generation
- Algorithm: firstName + initials of middleName + counter
- Example: "Thanh Hung Minh" → "minhth1"
- Auto-increment if exists

### Password
- BCrypt hashing
- Random generation: 12 chars (uppercase, lowercase, numbers, special chars)
- Reset returns plain text (show once)

### User Ban/Unban
- Soft delete via IsActive flag
- Banned users cannot login
- Error message: "Your account has been disabled by administrator"

### Salary Scales (VND)
- Admin: Level 1 (50M)
- Manager: Levels 1-3 (30M, 40M, 50M)
- Employee: Levels 1-3 (15M, 20M, 25M)

### Manager Ban Logic
- Manager cannot be banned if managing a department
- Must assign new manager first (redirect to Department page with error toast)

### Income Calculation
- Base salary retrieved from SalaryScale based on Role and Level
- Daily salary = Base salary / 22 working days per month
- Today deduction:
  - Present: No deduction
  - Late: Deduct LateDeductionPercent% of daily salary (from Policy)
  - LeaveEarly: Deduct LeaveEarlyDeductionPercent% of daily salary (from Policy)
  - Absent (no check-in or status Absent): Deduct 100% of daily salary
- Monthly deduction:
  - Sum of all deductions from Late/LeaveEarly/Absent days in current month
  - Final salary = Base salary - Total deduction
- Only calculates for the manager themselves, not team members

## Client Features

### Implemented Pages

**Auth**
- Login with error handling
- Logout
- Session management

**Dashboard** (`/Dashboard/Index`)
- Landing page after login
- Displays current date and day of week
- Admin: Shows total employees and departments count
- Role-based navigation
- **Performance Evaluation Chat** (Admin only):
  - Floating button (bottom-right) with AI voice icon
  - Modal interface with two-level menu system
  - Main menu options:
    - General Evaluation: Overall performance analysis for last 30 days (shows "coming soon" toast)
    - Level Promotion: Opens sub-menu for role selection
  - Sub-menu (Level Promotion):
    - Evaluate Managers: Analyze manager performance for level promotion
    - Evaluate Employees: Analyze employee performance for level promotion
  - Back button navigation between menus

**User Management** (`/User/Index`, Admin)
- List users grouped by role (Admin, Manager, Employee)
- Collapsible sections for Active/Banned users (Manager/Employee only)
- Level badges display number only (1, 2, 3)
- Actions: Ban/Unban (with confirmation), Reset password (shows new password)
- Manager ban: Redirects to Department page if managing department

**Policy Management** (`/Policy/Index`, Admin)
- View and update annual leave days and max overtime hours
- Form validation

**Compensation** (`/Compensation/Index`, Admin)
- View salary scales grouped by role
- Collapsible sections for each role
- Level badges (number only)
- Update base salary with validation
- Toast notifications

**Department Management** (`/Department/Index`, Admin)
- List departments with manager and employee count
- Create department (search/select employee to promote)
- Update department name (inline edit)
- Change manager (separate page)
- Delete department (only if empty)
- No collapsible sections (flat display)

**Change Manager** (`/Department/ChangeManager/{id}`, Admin)
- Search employees in department
- Select employee to promote to Manager Level 1
- Old manager demoted to Employee Level 3
- Level badges (number only)
- Confirmation modal

**Profile** (`/Profile/Index`, All roles)
- View username (disabled), fullName, address
- Circular avatar with initials (click shows "Coming soon" toast)
- Edit fullName and address
- Save updates user session FullName
- Client-side validation (fullName min 2 chars, address min 5 chars)

**Manager Dashboard** (`/Dashboard/Index`, Manager role)
- Check-In Status card:
  - If not checked in: "Check In Now" button
  - If checked in: ✓ icon + "On-time" badge (green) or "Late" badge (yellow), time, message
- Check-Out Status card:
  - If not checked in: "Please check in first" message
  - If checked in but not out: "Check Out Now" button
  - If checked out: ✓ icon + "On-time" badge (green) or "Early" badge (yellow), time, message
- Team Members card: Total count
- Requests Today card: Total + pending count
- Attendance Today card: Present count + percentage
- Request Statistics section: Toggle Today/This Week, breakdown by status (Pending/Approved/Rejected) with percentages
- Attendance Statistics section: Toggle Today/This Week, breakdown by status (Present/Late/On Leave/Absent) with percentages
- Most Absences card: Displays #1 most absent employee with count (circular avatar, name, level badge)
- Toast notifications: Success (green) for on-time, Warning (yellow) for late/early, Error (red) for failures

**Manager Employees** (`/Manager/Employees`, Manager role)
- List all employees in manager's department
- Grouped by role (Managers, Employees)
- Table columns: ID, Full Name, Level, Today's Status
- Today's Status: Badge with color-coded attendance status
  - Present (green), Late (yellow), On Leave (blue), Absent (red), Not checked in (gray)
- Total members count

**Manager Attendance** (`/Manager/Attendance`, Manager role)
- View personal attendance history
- Period toggle: "Last Week" (7 days) or "Last Month" (30 days)
- Table columns: Date, Check-In, Check-Out, Status, Working Hours
- Status badges: Present (green), Late (yellow), On Leave (blue), Absent (red)
- Date search: Input field with date picker
  - Search by specific date
  - If not found: Toast notification "Ngày đó công ty chưa mở cửa"
  - If found: Display single record card with all details
- Working hours calculated from check-in to check-out
- Empty state when no records found

**Manager Requests** (`/Manager/Requests`, Manager role)
- View and manage requests from department employees
- Sidebar filter: Pending Requests, Processed Requests (by status)
- Type filter: All, Leave, Resignation, Overtime (buttons in view)
- Table columns: ID, Employee, Type, Start Date, End Date, Content, Status, Actions
- Status badges: Pending (yellow), Approved (green), Rejected (red)
- Actions: Approve/Reject buttons (only for Pending requests)
- Confirmation modal before approve/reject with employee name and request type
- Toast notifications for success/error
- Only shows requests from manager's department
- Processed requests show "Processed" text instead of action buttons

**Manager Income** (`/Manager/Income`, Manager role)
- View income statistics for manager only (not team)
- Period toggle: Today, This Month
- Today view:
  - Cards: Base Salary (Monthly), Daily Salary, Today's Final Salary, Deduction Amount (if any)
  - Details: Attendance Status, Daily Salary, Deduction (if any), Reason, Final Salary Today
  - Deductions: Late (Policy.LateDeductionPercent%), LeaveEarly (Policy.LeaveEarlyDeductionPercent%), Absent (100%)
- Monthly view:
  - Cards: Base Salary, Total Working Days, Final Salary, Total Deduction (if any)
  - Details: Base Salary, Total Working Days, Total Deduction, Final Salary
  - Attendance Breakdown: Present Days, Late Days, Leave Early Days, Absent Days
  - Calculates total deductions from all attendance violations in current month
- Income calculation:
  - Base salary from SalaryScale (Role + Level)
  - Daily salary = Base salary / 22 working days
  - Final salary = Base salary - total deductions
- Currency format: VND with thousand separators

**Employee Dashboard** (`/Dashboard/Index`, Employee role)
- Check-In Status card:
  - If not checked in: "Check In Now" button
  - If checked in: ✓ icon + "On-time" badge (green) or "Late" badge (yellow), time, message
- Check-Out Status card:
  - If not checked in: "Please check in first" message
  - If checked in but not out: "Check Out Now" button
  - If checked out: ✓ icon + "On-time" badge (green) or "Early" badge (yellow), time, message
- Toast notifications: Success (green) for on-time, Warning (yellow) for late/early, Error (red) for failures

**Employee Attendance** (`/Employee/Attendance`, Employee role)
- View personal attendance history
- Period toggle: "Last Week" (7 days) or "Last Month" (30 days)
- Table columns: Date, Check-In, Check-Out, Status, Working Hours
- Status badges: Present (green), Late (yellow), On Leave (blue), Absent (red)
- Date search: Input field with date picker
  - Search by specific date
  - If not found: Toast notification "Ngày đó công ty chưa mở cửa"
  - If found: Display single record card with all details
- Working hours calculated from check-in to check-out
- Empty state when no records found

**Employee Requests** (`/Employee/Requests`, Employee role)
- View all requests created by the employee
- Filter by status: All, Pending, Processed (buttons in view)
- Table columns: ID, Type, Start Date, End Date, Content, Status
- Status badges: Pending (yellow), Approved (green), Rejected (red)
- Create New Request button:
  - Modal form with fields: Type (Leave/Resignation/Overtime), Start Date, End Date, Content
  - Form validation (required fields, date validation)
  - Confirmation modal before submit showing request details
  - Toast notifications for success/error
  - Request automatically assigned to employee's department
- Only shows requests created by the employee

**Employee Income** (`/Employee/Income`, Employee role)
- View income statistics for employee only
- Period toggle: Today, This Month
- Today view:
  - Cards: Base Salary (Monthly), Daily Salary, Today's Final Salary, Deduction Amount (if any)
  - Details: Attendance Status, Daily Salary, Deduction (if any), Reason, Final Salary Today
  - Deductions: Late (Policy.LateDeductionPercent%), LeaveEarly (Policy.LeaveEarlyDeductionPercent%), Absent (100%)
- Monthly view:
  - Cards: Base Salary, Total Working Days, Final Salary, Total Deduction (if any)
  - Details: Base Salary, Total Working Days, Total Deduction, Final Salary
  - Attendance Breakdown: Present Days, Late Days, Leave Early Days, Absent Days
  - Calculates total deductions from all attendance violations in current month
- Income calculation:
  - Base salary from SalaryScale (Role + Level)
  - Daily salary = Base salary / 22 working days
  - Final salary = Base salary - total deductions
- Currency format: VND with thousand separators

### UI Components

**Modals**
- Confirmation for destructive actions
- Password display (reset)
- Create department (search/select)
- Overlay click to close

**Toast Notifications**
- Success/Error with icons
- Auto-hide after 4s
- Top-right positioning

**Forms**
- Inline validation
- Error messages below inputs
- Focus states with box-shadow

**Tables**
- Hover states
- Badges for status/role/level
- Action buttons

**Level Badges** (defined in common-styles.css)
- Display number only (1, 2, 3)
- Level 1: Blue (#ddf4ff bg, #0969da text)
- Level 2: Green (#dafbe1 bg, #1a7f37 text)
- Level 3: Orange (#fff8c5 bg, #bf8700 text)

**Collapsible Sections** (common-styles.css)
- Clickable header with arrow icon
- Smooth expand/collapse (0.3s)
- Icon rotation on toggle
- Applied in User and Compensation pages

## UI Design System

### Colors
- Primary: #0969da (links, CTAs)
- Hover: #0860ca
- Text Primary: #24292f
- Text Secondary: #57606a
- Background: #ffffff
- Background Secondary: #f6f8fa
- Border: #d0d7de
- Success: #1a7f37
- Error: #cf222e
- Warning: #bf8700

### Typography
- Font: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif
- Weights: 300 (light), 400 (normal), 500 (medium), 600 (semibold)
- Sizes: Title (24-32px), Heading (18-20px), Body (14px), Small (12-13px)

### Layout
- Border radius: 6px (buttons/inputs), 12px (cards/modals)
- Spacing: 4px, 8px, 12px, 16px, 24px, 32px
- Navbar: Fixed top, 60px
- Sidebar: Fixed left, 240px
- Content: Margin-left 240px, padding 32px

## Code Patterns

### Repository Pattern
```csharp
public class EntityRepository
{
    private readonly HRMDbContext _context;
    public async Task<Entity?> GetByIdAsync(int id) {}
    public async Task<List<Entity>> GetAllAsync() {}
}
```

### Service Pattern
```csharp
public class EntityService
{
    private readonly EntityRepository _repository;
    public async Task<EntityResponse> GetAsync(int id) {}
    public async Task<EntityResponse?> CreateAsync(CreateRequest request) {}
}
```

### Controller (API)
```csharp
[Authorize(Roles = "Admin")]
[Route("api/entities")]
[ApiController]
public class EntityController : ControllerBase
{
    private readonly EntityService _service;
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        request.Id = id;
        var response = await _service.UpdateAsync(request);
        if (response == null) return NotFound(new { message = "Not found" });
        return Ok(response);
    }
}
```

### Client Service
```csharp
public class EntityService
{
    private readonly ApiClient _apiClient;
    
    public async Task<EntityResponse?> GetAsync()
    {
        var client = _apiClient.GetClient();
        var response = await client.GetAsync("/api/entities");
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<EntityResponse>(content, JsonOptions.DefaultOptions);
    }
}
```

### Client Controller
```csharp
public class EntityController : BaseController
{
    private readonly EntityService _service;
    
    public async Task<IActionResult> Index()
    {
        if (ViewBag.Role != "Admin")
            return RedirectToAction("Index", "Dashboard");
        var data = await _service.GetAsync();
        return View(data);
    }
    
    [HttpPost]
    public async Task<IActionResult> Update([FromBody] UpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }
        var response = await _service.UpdateAsync(request);
        if (response == null)
            return Json(new { success = false, message = "Failed to update" });
        return Json(new { success = true, message = "Updated successfully" });
    }
}
```

## Configuration

### API (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=PRN232_HRM;..."
  },
  "Jwt": {
    "Key": "...",
    "Issuer": "HRM_API",
    "Audience": "HRM_Client"
  },
  "Gemini": {
    "ApiKey": "AIzaSyBCHwBoFEVxC_AzjUaiLkIfoN1GaHjxnx4",
    "Model": "gemini-2.5-flash"
  }
}
```

### Client
- API Base URL: http://localhost:5051
- Session timeout: 8 hours
- CORS enabled

## Middleware

### API
- CORS (AllowClient policy)
- Authentication
- Authorization

### Client
- ApiErrorHandlerMiddleware
- Session
- Static files

## Validation

### Server-Side
- [Required], [Range(min, max)]
- Custom error messages

### Client-Side
- Input validation before API calls
- Real-time error display

## Seeding Data

Located in Program.cs (API), currently commented out.
- 1 Admin (minhth1, password: mothaiba1)
- 5 Managers (random levels 1-3)
- 50 Employees (random levels 1-3)
- 5 Departments
- 7 Salary Scales

### Manager Sidebar Navigation
- Department section: Employees, Attendance
- Requests section: Pending Requests, Processed Requests (filter by status)
- Income section: Income Statistics
- Documents section: Shared Files (not implemented)

### Employee Sidebar Navigation
- My Work section: Attendance, My Requests
- Income section: Income Statistics
- Documents section: Shared Files (not implemented)

## Performance Evaluation

### Overview
AI-powered performance evaluation system using Google Gemini API for comprehensive employee performance analysis.

### General Evaluation (Implemented)
- **Endpoint**: `POST /api/admin/performance/general-evaluation`
- **Access**: Admin only
- **Functionality**: 
  - Aggregates performance data for all employees (Manager + Employee) in last 30 days
  - Data includes: attendance metrics (attendance rate, punctuality, working hours), request behavior (approval rates, types breakdown)
  - Sends aggregated data to Gemini API for analysis
  - Returns formatted text report with comprehensive evaluation

**Data Aggregation**:
- Attendance: Total working days, Present/Late/LeaveEarly/Absent days, average working hours, attendance rate
- Requests: Total count, Approved/Rejected/Pending, breakdown by type (Leave/Resignation/Overtime)

**Prompt Engineering**:
- System prompt: Defines role as professional HR performance analyst
- User prompt: Provides structured employee data with evaluation criteria
- Output format: Structured text report with clear sections (Executive Summary, Overall Analysis, Strengths, Weaknesses, Top Performers, Employees Needing Attention, Department Analysis, Recommendations)
- Language: English only
- Style: Direct, professional, data-driven (no icons, no fluff)

**UI Implementation**:
- Floating button on Admin dashboard (bottom-right corner)
- Modal chat interface with:
  - Main menu: General Evaluation, Level Promotion (sub-menu)
  - Loading state: Spinner with "Analyzing performance data..." message
  - Results display: Formatted text in scrollable container (max-height 500px, white-space: pre-wrap)
- Error handling: Timeout notifications, error messages

**Timeout Configuration**:
- GeminiService HttpClient: 10 minutes
- Kestrel Server: KeepAliveTimeout and RequestHeadersTimeout = 10 minutes
- RequestTimeout Policy: "LongRunning" = 10 minutes for endpoint
- Client HttpClient: 10 minutes
- Client Fetch: 10 minutes with AbortController
- Note: Gemini API typically responds in 30-40 seconds for full evaluation

**Known Issues**:
- First request may succeed, subsequent requests may timeout at ~32 seconds despite Gemini API responding in ~37 seconds
- Possible causes: Network/proxy timeout, rate limiting, or timeout configuration not fully propagated
- Workaround: Restart API server after timeout configuration changes

### Level Promotion (Complete)
- **Evaluate Managers**: Analyze manager performance for level promotion eligibility
- **Evaluate Employees**: Analyze employee performance for level promotion eligibility
- **Endpoint**: `POST /api/admin/performance/level-promotion-evaluation`
- **Access**: Admin only
- **Functionality**:
  - Aggregates performance data for last 30 days
  - Filters candidates: only users with level < 3 (max level)
  - Pre-filters: attendance rate > 85%, late days ≤ 2, avg hours ≥ 7.0h
  - Sends top 10 candidates to Gemini API for evaluation
  - Returns JSON response with recommendation
- **Data Aggregation**:
  - Attendance: Total working days, Present/Late/LeaveEarly/Absent days, average working hours, attendance rate (30 days)
  - Requests: Total count, Approved/Rejected/Pending (ALL requests for approval rate calculation)
- **Prompt Engineering**:
  - System prompt: Defines role as HR analyst, requires JSON response format
  - User prompt: Compact format (one line per candidate), includes evaluation criteria
  - Output format: JSON with hasRecommendation, recommendedUserId, recommendedUserName, currentLevel, recommendedLevel, reason
  - Language: English only
  - Criteria: Attendance >85%, Late ≤2 days, AvgHours ≥7h, ApprovalRate >80%
- **UI Implementation**:
  - Sub-menu option in Performance Evaluation modal
  - Loading state during evaluation
  - Results display:
    - If no recommendation: Shows "No Promotion Recommendation" message
    - If has recommendation: Displays highlighted card with candidate name, level info, reason, and Accept/Reject buttons
  - Accept button: Promotes user level (calls `/api/admin/performance/promote-level`)
  - Reject button: Closes modal without action
- **Promotion Endpoint**: `POST /api/admin/performance/promote-level`
  - Request: `{ userId }`
  - Validates: User exists, level < 3
  - Action: Increments user level by 1
  - Response: User details with old and new level
- **Optimization**:
  - Limits to top 10 candidates to avoid MAX_TOKENS error
  - Pre-filters candidates before sending to LLM
  - Fallback: If no candidates pass filter, sends top 5 anyway
  - maxOutputTokens: 3072 to allow for model's internal reasoning
- **Status**: Complete (UI + Backend + Gemini integration)

### Technology
- **AI Model**: Google Gemini 2.5 Flash
- **API Key**: Stored in `appsettings.json` (HRM_API)
- **Integration**: GeminiService with HTTP client, prompt engineering for structured text output

### UI Components
- Floating button with AI voice icon (`ai-voice-Stroke-Rounded-3.png`)
- Modal chat interface with navigation
- Back button for sub-menu navigation
- Loading spinner for long operations
- Icons:
  - Main menu: `chart-line-data-02-Stroke-Rounded.png` (General Evaluation), `add-money-circle-Stroke-Rounded.png` (Level Promotion)
  - Sub-menu: `manager-Stroke-Rounded-2.png` (Managers), `user-Stroke-Rounded-2.png` (Employees)

### Status
- General Evaluation: Complete (UI + Backend + Gemini integration)
- Level Promotion: Complete (UI + Backend + Gemini integration)

## Not Yet Implemented

### Backend
- Notifications
- Shared files

### Frontend
- Notification system
- File sharing
- Report generation
- Avatar upload (Profile page)
- Auto-reject requests if not approved before endDate (not implemented)

## Development Workflow

1. Update API models/migrations
2. Run: `dotnet ef database update`
3. Implement Repository → Service → DTOs → Controller
4. Test API (Swagger)
5. Create Client models/service/controller/views
6. Test end-to-end

## Dependencies

### API
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.AspNetCore.Authentication.JwtBearer
- BCrypt.Net-Next
- Google Gemini API (via HTTP client) - For performance evaluation
  - Model: gemini-2.5-flash
  - Integration: GeminiService with prompt engineering
  - Output: Formatted text reports (not JSON)

### Client
- System.Text.Json
- Microsoft.AspNetCore.Session

### Shared
- `/wwwroot/css/common-styles.css` - Level badges, collapsible sections, attendance status badges

## Notes for AI Agents

- Follow existing patterns (Repository → Service → Controller)
- Use design system colors/spacing exactly
- Validate on both client and server
- Keep DTOs separate from entities
- Use async/await consistently
- Handle null cases
- Return proper HTTP status codes
- Follow C# naming conventions
- Add appropriate error handling
- Test with different roles
- Use common-styles.css for level badges and collapsible sections
- Level badges show number only (1, 2, 3)
- Apply collapsible sections where appropriate (User, Compensation)

## Recent Updates

### Admin Features (Complete)
- User management: List, Ban/Unban, Reset password, Create new user
- Policy management: Update leave days and overtime hours
- Compensation: Update salary scales
- Department: Full CRUD with manager change logic
- Dashboard: Shows total employees/departments count, request/attendance statistics (Today/This Week)
- Performance Evaluation (Complete):
  - General Evaluation: AI-powered evaluation for all employees in last 30 days
    - Endpoint: `POST /api/admin/performance/general-evaluation`
    - Data aggregation: Attendance + Request metrics for all employees
    - Gemini API integration: Returns formatted text report with comprehensive analysis
    - UI: Modal chat interface with loading state and formatted text display
    - Timeout: 10 minutes (configured at all levels)
    - Known issue: May timeout at ~32s despite Gemini responding in ~37s (needs optimization)
  - Level Promotion: Complete (UI + Backend + Gemini integration)
    - Evaluate Managers/Employees for level promotion
    - JSON response format with recommendation
    - Accept/Reject UI with promotion action
    - Pre-filtering and optimization for large datasets
  - Floating button on dashboard (bottom-right)
  - Two-level menu system: Main menu → Sub-menu
  - Icons: AI voice, chart-line-data (general), add-money-circle (promotion), manager, user

### Profile Management (Complete)
- API endpoints: GET/PUT `/api/profile`
- Profile page: View/edit fullName and address
- Circular avatar with initials
- Session updates on save
- Client-side validation
- Available to all roles

### UI Enhancements (Complete)
- common-styles.css: Level badges, collapsible sections
- Level badges show number only (1, 2, 3)
- Collapsible sections in User (Active/Banned) and Compensation (Role groups)
- Department uses flat display (no collapsible)
- Consistent styling across all pages
- Toast notifications for user feedback
- Manager ban redirects to Department page if managing department

### Manager Features (Complete)
- Manager dashboard with check-in/check-out, team stats, request/attendance statistics
- Check-in functionality with Policy validation:
  - On-time if check-in <= WorkStartTime + LateThresholdMinutes
  - Late if check-in > WorkStartTime + LateThresholdMinutes
  - Status: Present (on-time) or Late
  - Toast notification with on-time/late message
- Check-out functionality with Policy validation:
  - On-time if check-out >= WorkEndTime - LeaveEarlyThresholdMinutes
  - Early if check-out < WorkEndTime - LeaveEarlyThresholdMinutes
  - Updates status to LeaveEarly if applicable
  - Toast notification with on-time/early message
- Department employees list with today's attendance status
- Request statistics (Today/This Week) for department
- Attendance statistics (Today/This Week) for department
- Top absentee display (single most absent employee)
- Real-time attendance status badges (Present, Late, On Leave, Absent)
- Visual status indicators: "On-time" badge (green), "Late"/"Early" badge (yellow)
- Attendance history page:
  - Period toggle: Last Week (7 days) / Last Month (30 days)
  - Table view with date, check-in, check-out, status, working hours
  - Date search: Search by specific date
  - Error message: "Ngày đó công ty chưa mở cửa" if date not found
  - Working hours calculated from check-in to check-out
- Request management page:
  - View all requests from department employees
  - Filter by status (Pending/Processed) via sidebar
  - Filter by type (All/Leave/Resignation/Overtime) via buttons
  - Approve/Reject actions with confirmation modal
  - Only shows requests from manager's department
  - Only allows processing Pending requests
- Income statistics page:
  - Today and Monthly income calculations
  - Based on SalaryScale and Policy deductions
  - Shows attendance breakdown and deduction details
  - Personal income only (not team)

### Employee Features (Complete)
- Employee dashboard with check-in/check-out functionality
- Check-in functionality with Policy validation:
  - On-time if check-in <= WorkStartTime + LateThresholdMinutes
  - Late if check-in > WorkStartTime + LateThresholdMinutes
  - Status: Present (on-time) or Late
  - Toast notification with on-time/late message
- Check-out functionality with Policy validation:
  - On-time if check-out >= WorkEndTime - LeaveEarlyThresholdMinutes
  - Early if check-out < WorkEndTime - LeaveEarlyThresholdMinutes
  - Updates status to LeaveEarly if applicable
  - Toast notification with on-time/early message
- Attendance history page:
  - Period toggle: Last Week (7 days) / Last Month (30 days)
  - Table view with date, check-in, check-out, status, working hours
  - Date search: Search by specific date
  - Error message: "Ngày đó công ty chưa mở cửa" if date not found
  - Working hours calculated from check-in to check-out
- Request management page:
  - View all requests created by the employee
  - Filter by status: All, Pending, Processed
  - Create New Request button with modal form
  - Form fields: Type (Leave/Resignation/Overtime), Start Date, End Date, Content
  - Form validation and confirmation modal before submit
  - Requests automatically assigned to employee's department
  - Toast notifications for success/error
- Income statistics page:
  - Today and Monthly income calculations
  - Based on SalaryScale and Policy deductions
  - Shows attendance breakdown and deduction details
  - Personal income only
