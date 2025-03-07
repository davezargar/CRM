using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

public class PasswordHasher
{
    public static (string hashedPassword, string salt) HashPassword(string password)
    {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(128 / 8);
        string salt = Convert.ToBase64String(saltBytes);

        string hashedPassword = PBKDF2Hash(password, saltBytes);

        return (hashedPassword, salt);
    }

    public static bool VerifyHashedPassword(string password, string salt, string hashedPassword)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] computedHashBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8
        );

        string computedHash = Convert.ToBase64String(computedHashBytes);

        return computedHash == hashedPassword;
    }

    private static string PBKDF2Hash(string password, byte[] saltBytes)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8
        ));
    }
}