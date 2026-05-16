using System.Security.Cryptography;
using System.Text;

namespace NovaStaff.Helpers;

public static class TokenHasher
{
    public static string Hash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}
