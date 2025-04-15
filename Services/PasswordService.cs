using BCrypt.Net;
using worksystem.DTOs;

namespace worksystem.Services
{
    public class PasswordService
    {
        // Jelszó hash-olása
        public string HashPassword(string password)
        {
            ValidatePassword(password);
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Jelszó ellenőrzése
        public bool VerifyPassword(string password, string PasswordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(PasswordHash))
                return false;
                
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
            }
            catch (SaltParseException ex)
            {
                throw new ArgumentException("Invalid password hash formátum!", ex);
            }
        }
        // Jelszó validációja
        public void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("A jelszó nem lehet üres.");

            if (password.Length < 8)
                throw new ArgumentException("A jelszónak legalább 8 karakter hosszúnak kell lennie.");

            if (!password.Any(char.IsLetter))
                throw new ArgumentException("A jelszónak tartalmaznia kell legalább egy betűt.");

            if (!password.Any(char.IsDigit))
                throw new ArgumentException("A jelszónak tartalmaznia kell legalább egy számot.");
        }
    }
}