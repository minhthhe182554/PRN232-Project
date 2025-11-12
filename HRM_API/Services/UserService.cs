using HRM_API.Dtos;
using HRM_API.Repositories;


namespace HRM_API.Services;

public class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserList> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllWithDetailsAsync();
        
        Console.WriteLine($"DEBUG: Found {users.Count} users in database");
        
        var userDtos = users.Select(u => new UserListDto
        {
            Id = u.Id,
            Role = u.Role,
            Level = u.Level,
            FullName = u.FullName,
            IsActive = u.IsActive,
            DepartmentName = u.Department?.Name,
            ManagedDepartmentName = u.ManagedDepartment?.Name
        }).ToList();
        
        Console.WriteLine($"DEBUG: Mapped {userDtos.Count} userDtos");

        return new UserList
        {
            ActiveUsers = userDtos.Where(u => u.IsActive).ToList(),
            InactiveUsers = userDtos.Where(u => !u.IsActive).ToList(),
            Count = userDtos.Count
        };
    }
}