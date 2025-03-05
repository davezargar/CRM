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