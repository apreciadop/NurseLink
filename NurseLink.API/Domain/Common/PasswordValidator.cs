using System.Text.RegularExpressions;

namespace NurseLink.API.Domain.Common
{
    public static class PasswordValidator
    {
        private static readonly Regex PasswordRegex = new(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$",
            RegexOptions.Compiled
        );

        public static string? Validate(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password is required.";

            if (password.Length < 6 || password.Length > 255)
                return "Password must be between 6 and 255 characters.";

            if (!PasswordRegex.IsMatch(password))
                return "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character.";

            return null;
        }
    }
}