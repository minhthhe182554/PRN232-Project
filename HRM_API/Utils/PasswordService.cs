using System;

namespace HRM_API.Utils;

public class PasswordService
{
    public static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool VerifyPassword(string password, string hashed)
        => BCrypt.Net.BCrypt.Verify(password, hashed);
}
