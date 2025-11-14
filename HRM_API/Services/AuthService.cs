using HRM_API.Dtos;
using HRM_API.Models;
using HRM_API.Repositories;
using HRM_API.Utils;

namespace HRM_API.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly JwtService _jwtService;

        public AuthService(UserRepository userRepository, JwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // find User by username (including inactive to check if banned)
            var user = await _userRepository.GetByUsernameIncludingInactiveAsync(request.Username);
            if (user == null)
                return null;

            // Verify password 
            if (!PasswordService.VerifyPassword(request.Password, user.Password))
                return null;

            // Check if user is banned
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Your account has been disabled by administrator");
            }

            // Create JWT token 
            var token = _jwtService.GenerateToken(user);

            return new LoginResponse
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Level = user.Level,
                DepartmentName = user.Department?.Name
            };
        }

        public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
        {
            // Generate base username from FirstName and MiddleName
            var baseUsername = UsernameGenerator.GenerateBaseUsername(request.FirstName, request.MiddleName);
            
            // Find next available username with number suffix
            var username = await GenerateUniqueUsernameAsync(baseUsername);
            
            // Generate random password
            var plainPassword = UsernameGenerator.GenerateRandomPassword(12);
            
            // Create full name
            var fullName = $"{request.MiddleName} {request.FirstName}";

            // Create new user
            var user = new User
            {
                Username = username,
                Password = PasswordService.HashPassword(plainPassword),
                Role = request.Role,
                Level = request.Level,
                FullName = fullName,
                Address = request.Address,
                DepartmentId = request.DepartmentId,
                ProfileImgUrl = request.ProfileImgUrl ?? "default-url",
                AnnualLeaveDays = request.AnnualLeaveDays,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(user);

            return new RegisterResponse
            {
                UserId = createdUser.Id,
                Username = createdUser.Username,
                Password = plainPassword, // Return plain password for admin
                FullName = createdUser.FullName,
                Role = createdUser.Role.ToString(),
                Message = "User created successfully"
            };
        }

        // Generate unique username by adding incremental number
        private async Task<string> GenerateUniqueUsernameAsync(string baseUsername)
        {
            var username = baseUsername + "1";
            var counter = 1;

            while (await _userRepository.UsernameExistsAsync(username))
            {
                counter++;
                username = baseUsername + counter;
            }

            return username;
        }
    }
}

