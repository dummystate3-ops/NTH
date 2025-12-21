using System.Security.Cryptography;
using System.Text;

namespace NovaToolsHub.Services;

/// <summary>
/// AES-GCM encryption service with PBKDF2 key derivation.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private const int SaltSize = 16;
    private const int IvSize = 12;
    private const int KeySizeBits = 256;
    private const int Iterations = 100_000;

    public Task<string> EncryptAsync(string plainText, string password)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            throw new ArgumentException("Plain text is required.", nameof(plainText));
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters.", nameof(password));

        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var iv = RandomNumberGenerator.GetBytes(IvSize);
        var key = DeriveKey(password, salt);

        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aesGcm = new AesGcm(key, 16);
        aesGcm.Encrypt(iv, plainBytes, cipherBytes, tag);

        var combined = new byte[SaltSize + IvSize + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, SaltSize);
        Buffer.BlockCopy(iv, 0, combined, SaltSize, IvSize);
        Buffer.BlockCopy(tag, 0, combined, SaltSize + IvSize, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, SaltSize + IvSize + tag.Length, cipherBytes.Length);

        return Task.FromResult(Convert.ToBase64String(combined));
    }

    public Task<string> DecryptAsync(string cipherText, string password)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            throw new ArgumentException("Cipher text is required.", nameof(cipherText));
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters.", nameof(password));

        var combined = Convert.FromBase64String(cipherText);
        if (combined.Length < SaltSize + IvSize + 16 + 1)
            throw new InvalidOperationException("Cipher text is invalid or corrupted.");

        var salt = new byte[SaltSize];
        var iv = new byte[IvSize];
        var tag = new byte[16];
        var cipherBytes = new byte[combined.Length - SaltSize - IvSize - tag.Length];

        Buffer.BlockCopy(combined, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(combined, SaltSize, iv, 0, IvSize);
        Buffer.BlockCopy(combined, SaltSize + IvSize, tag, 0, tag.Length);
        Buffer.BlockCopy(combined, SaltSize + IvSize + tag.Length, cipherBytes, 0, cipherBytes.Length);

        var key = DeriveKey(password, salt);
        var plainBytes = new byte[cipherBytes.Length];

        using var aesGcm = new AesGcm(key, 16);
        aesGcm.Decrypt(iv, cipherBytes, tag, plainBytes);

        return Task.FromResult(Encoding.UTF8.GetString(plainBytes));
    }

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(KeySizeBits / 8);
    }
}
