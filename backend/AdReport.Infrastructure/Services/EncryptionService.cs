using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using AdReport.Application.Interfaces;

namespace AdReport.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var secret = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key is not configured");

        // Derive a 256-bit key from the configured secret
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
    }

    /// <summary>
    /// Encrypts plain text using AES-256-CBC and returns a Base64 string of IV+ciphertext.
    /// </summary>
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts a Base64-encoded AES-256-CBC cipher text (IV prepended).
    /// </summary>
    public string Decrypt(string cipherText)
    {
        var fullBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullBytes.Length - iv.Length];

        Buffer.BlockCopy(fullBytes, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullBytes, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
