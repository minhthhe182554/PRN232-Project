using System.Text;
using System.Text.RegularExpressions;

namespace HRM_API.Utils
{
    public static class UsernameGenerator
    {
        // Generate base username from FirstName and MiddleName
        public static string GenerateBaseUsername(string firstName, string middleName)
        {
            // Remove Vietnamese diacritics and convert to lowercase
            var cleanFirstName = RemoveVietnameseDiacritics(firstName).ToLower().Trim();
            var cleanMiddleName = RemoveVietnameseDiacritics(middleName).ToLower().Trim();

            // Split middle name into parts
            var middleParts = cleanMiddleName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Get first letter of each middle name part
            var initials = string.Join("", middleParts.Select(p => p[0]));

            // Username = firstName + initials
            return cleanFirstName + initials;
        }

        // Remove Vietnamese diacritics
        private static string RemoveVietnameseDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd').Replace('Đ', 'D');
        }

        // Generate random password
        public static string GenerateRandomPassword(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

