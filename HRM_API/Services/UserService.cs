using HRM_API.Dtos;
using HRM_API.Repositories;
using HRM_API.Utils;

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

    public async Task<BanUserResponse?> BanUserAsync(BanUserRequest request)
    {
        var user = await _userRepository.GetByIdIncludingInactiveAsync(request.UserId);
        
        if (user == null)
            return null;

        var newStatus = !request.IsBanned;
        var success = await _userRepository.UpdateUserStatusAsync(request.UserId, newStatus);

        if (!success)
            return null;

        return new BanUserResponse
        {
            UserId = request.UserId,
            IsActive = newStatus,
            Message = request.IsBanned ? "User banned successfully" : "User unbanned successfully"
        };
    }

    public async Task<ResetPasswordResponse?> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByIdIncludingInactiveAsync(request.UserId);
        
        if (user == null)
            return null;

        // Generate random password
        var newPassword = UsernameGenerator.GenerateRandomPassword(12);
        var hashedPassword = PasswordService.HashPassword(newPassword);

        var success = await _userRepository.UpdatePasswordAsync(request.UserId, hashedPassword);

        if (!success)
            return null;

        return new ResetPasswordResponse
        {
            UserId = request.UserId,
            Username = user.Username,
            NewPassword = newPassword,
            Message = "Password reset successfully"
        };
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
            return null;

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Address = user.Address,
            ProfileImgUrl = user.ProfileImgUrl
        };
    }

    public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
            return false;

        user.FullName = request.FullName;
        user.Address = request.Address;

        return await _userRepository.UpdateUserProfileAsync(user);
    }

    public async Task<PromoteLevelResponse?> PromoteLevelAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
            return null;

        // Check if user is at max level
        const int maxLevel = 3;
        if (user.Level >= maxLevel)
        {
            return null; // Already at max level
        }

        var oldLevel = user.Level;
        var newLevel = oldLevel + 1;

        var success = await _userRepository.UpdateLevelAsync(userId, newLevel);

        if (!success)
            return null;

        return new PromoteLevelResponse
        {
            UserId = userId,
            Username = user.Username,
            FullName = user.FullName,
            OldLevel = oldLevel,
            NewLevel = newLevel,
            Message = $"Level promoted from {oldLevel} to {newLevel} successfully"
        };
    }
}