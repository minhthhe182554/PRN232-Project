using HRM_API.Dtos;
using HRM_API.Models;
using HRM_API.Repositories;

namespace HRM_API.Services
{
    public class DepartmentService
    {
        private readonly DepartmentRepository _departmentRepository;
        private readonly UserRepository _userRepository;

        public DepartmentService(DepartmentRepository departmentRepository, UserRepository userRepository)
        {
            _departmentRepository = departmentRepository;
            _userRepository = userRepository;
        }

        public async Task<List<DepartmentResponse>> GetAllAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            
            return departments.Select(d => new DepartmentResponse
            {
                Id = d.Id,
                Name = d.Name,
                ManagerId = d.ManagerId,
                ManagerName = d.Manager?.FullName,
                EmployeeCount = d.Employees?.Count(e => e.IsActive) ?? 0
            }).ToList();
        }

        public async Task<DepartmentResponse?> UpdateNameAsync(UpdateDepartmentNameRequest request)
        {
            var success = await _departmentRepository.UpdateNameAsync(request.Id, request.Name!);
            
            if (!success)
                return null;

            var department = await _departmentRepository.GetByIdAsync(request.Id);
            if (department == null) return null;

            return new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                ManagerId = department.ManagerId,
                ManagerName = department.Manager?.FullName,
                EmployeeCount = department.Employees?.Count(e => e.IsActive) ?? 0
            };
        }

        public async Task<DepartmentResponse?> UpdateManagerAsync(UpdateDepartmentManagerRequest request)
        {
            // Get department with current manager
            var department = await _departmentRepository.GetByIdAsync(request.DepartmentId);
            if (department == null) return null;

            // Get current manager
            var currentManager = await _userRepository.GetByIdIncludingInactiveAsync(department.ManagerId);
            if (currentManager == null) return null;

            // Get new manager (must be an active employee)
            var newManager = await _userRepository.GetByIdAsync(request.NewManagerId!.Value);
            if (newManager == null) return null;
            
            // New manager must be an Employee (not already a Manager or Admin)
            if (newManager.Role != Models.Enums.Role.Employee)
                return null;

            // New manager must belong to the same department
            if (newManager.DepartmentId != request.DepartmentId)
                return null;

            // Transaction: Demote old manager, Promote new manager, Update department
            // 1. Demote current manager to Employee Level 3 (highest employee level)
            await _userRepository.UpdateRoleAndLevelAsync(currentManager.Id, Models.Enums.Role.Employee, 3);

            // 2. Promote new employee to Manager Level 1
            await _userRepository.UpdateRoleAndLevelAsync(newManager.Id, Models.Enums.Role.Manager, 1);

            // 3. Update department manager
            await _departmentRepository.UpdateManagerAsync(request.DepartmentId, newManager.Id);

            // Return updated department
            var updatedDepartment = await _departmentRepository.GetByIdAsync(request.DepartmentId);
            if (updatedDepartment == null) return null;

            return new DepartmentResponse
            {
                Id = updatedDepartment.Id,
                Name = updatedDepartment.Name,
                ManagerId = updatedDepartment.ManagerId,
                ManagerName = updatedDepartment.Manager?.FullName,
                EmployeeCount = updatedDepartment.Employees?.Count(e => e.IsActive) ?? 0
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employeeCount = await _departmentRepository.GetEmployeeCountAsync(id);
            
            // Cannot delete if department has employees
            if (employeeCount > 0)
                return false;

            return await _departmentRepository.DeleteAsync(id);
        }

        public async Task<List<EmployeeSearchResult>> SearchEmployeesAsync(string username, int? departmentId = null)
        {
            var employees = await _userRepository.SearchEmployeesByUsernameAsync(username, departmentId);
            
            return employees.Select(e => new EmployeeSearchResult
            {
                Id = e.Id,
                Username = e.Username,
                FullName = e.FullName,
                Level = e.Level,
                DepartmentName = e.Department?.Name
            }).ToList();
        }

        public async Task<List<EmployeeSearchResult>> GetEmployeesByDepartmentAsync(int departmentId)
        {
            var employees = await _userRepository.GetEmployeesByDepartmentAsync(departmentId);
            
            return employees.Select(e => new EmployeeSearchResult
            {
                Id = e.Id,
                Username = e.Username,
                FullName = e.FullName,
                Level = e.Level,
                DepartmentName = e.Department?.Name
            }).ToList();
        }

        public async Task<DepartmentResponse?> CreateAsync(CreateDepartmentRequest request)
        {
            // Get employee to promote to manager
            var employee = await _userRepository.GetByIdAsync(request.ManagerId!.Value);
            if (employee == null) return null;

            // Employee must be an Employee role (not already Manager or Admin)
            if (employee.Role != Models.Enums.Role.Employee)
                return null;

            // Create department
            var department = new Department
            {
                Name = request.Name!,
                ManagerId = employee.Id
            };

            await _departmentRepository.CreateAsync(department);

            // Promote employee to Manager Level 1 and assign to new department
            await _userRepository.UpdateRoleAndLevelAsync(employee.Id, Models.Enums.Role.Manager, 1);
            await _userRepository.UpdateDepartmentAsync(employee.Id, department.Id);

            // Return created department
            var createdDepartment = await _departmentRepository.GetByIdAsync(department.Id);
            if (createdDepartment == null) return null;

            return new DepartmentResponse
            {
                Id = createdDepartment.Id,
                Name = createdDepartment.Name,
                ManagerId = createdDepartment.ManagerId,
                ManagerName = createdDepartment.Manager?.FullName,
                EmployeeCount = 0 // New department, only manager
            };
        }
    }
}

