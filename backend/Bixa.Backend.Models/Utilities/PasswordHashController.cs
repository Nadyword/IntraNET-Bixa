using System.Security.Cryptography;

namespace Bixa.Backend.Models.Utilities;

/// <summary>
/// Provides utility methods for hashing and verifying passwords securely.
/// </summary>
public static class Hasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Hashes a plain-text password using PBKDF2 (Password-Based Key Derivation Function 2)
    /// with a random salt and stores the salt and hash as a Base64 string.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A Base64 string containing the salt and the derived hash.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the password is null.</exception>
    public static string HashPassword(string password)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));


        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);


        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithm))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize); // Get desired hash size

            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            string savedPasswordHash = Convert.ToBase64String(hashBytes);
            return savedPasswordHash;
        }
    }

    /// <summary>
    /// Verifies a plain-text password against a stored hashed password.
    /// </summary>
    /// <param name="enteredPassword">The plain-text password entered by the user.</param>
    /// <param name="storedHash">The stored Base64 hashed password (containing salt and hash).</param>
    /// <returns>True if the entered password matches the stored hash, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if either password or storedHash is null.</exception>
    /// <exception cref="FormatException">Thrown if storedHash is not a valid Base64 string or has an invalid length.</exception>
    public static bool VerifyPassword(string enteredPassword, string storedHash)
    {
        if (enteredPassword == null) throw new ArgumentNullException(nameof(enteredPassword));
        if (storedHash == null) throw new ArgumentNullException(nameof(storedHash));

        byte[] hashBytes;
        try
        {
            hashBytes = Convert.FromBase64String(storedHash);
        }
        catch (FormatException ex)
        {
            throw new FormatException("Stored hash is not a valid Base64 string.", ex);
        }

        if (hashBytes.Length != (SaltSize + HashSize))
            return false;

        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);


        using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, Iterations, HashAlgorithm))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize);

            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    return false; // Mismatch found
                }
            }
        }

        return true; // Passwords match
    }
}

// using System.Security.Cryptography;

// namespace Bixa.Backend.Models.Utilities;

// public static class Hasher
// {
//     public static string HashPassword(string password)
//     {
//         byte[] salt;
//         new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
//         var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
//         byte[] hash = pbkdf2.GetBytes(20);

//         byte[] hashBytes = new byte[36];
//         Array.Copy(salt, 0, hashBytes, 0, 16);
//         Array.Copy(hash, 0, hashBytes, 16, 20);

//         string savedPasswordHash = Convert.ToBase64String(hashBytes);
//         return savedPasswordHash;
//     }
//     public static bool VerifyPassword(string enteredPassword, string storedHash)
//     {
//         byte[] hashBytes = Convert.FromBase64String(storedHash);
//         byte[] salt = new byte[16];
//         Array.Copy(hashBytes, 0, salt, 0, 16);

//         var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 1000);
//         byte[] hash = pbkdf2.GetBytes(20);

//         for (int i = 0; i < 20; i++)
//         {
//             if (hashBytes[i + 16] != hash[i])
//             {
//                 return false;
//             }
//         }

//         return true;
//     }
// }