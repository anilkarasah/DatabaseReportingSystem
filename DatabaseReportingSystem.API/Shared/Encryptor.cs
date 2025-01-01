using System.Security.Cryptography;
using System.Text;
using DatabaseReportingSystem.Shared.Models;
using Newtonsoft.Json;

namespace DatabaseReportingSystem.Shared;

public sealed class Encryptor(IConfiguration configuration) : IEncryptor
{
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    private readonly string _encryptionKey = configuration["EncryptionKey"]!;

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(Constants.Encryption.PasswordSaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Constants.Encryption.PasswordIterations,
            Algorithm,
            Constants.Encryption.PasswordHashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        string[] parts = hashedPassword.Split('-');

        if (parts.Length != 2)
        {
            return false;
        }

        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);

        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Constants.Encryption.PasswordIterations,
            Algorithm,
            Constants.Encryption.PasswordHashSize);

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }

    public string Encrypt(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
        aes.GenerateIV();

        using var memoryStream = new MemoryStream();
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        using (var streamWriter = new StreamWriter(cryptoStream))
        {
            streamWriter.Write(text);
        }

        byte[] encryptedBytes = memoryStream.ToArray();
        if (encryptedBytes.Length == 0)
        {
            throw new InvalidOperationException("Encryption failed.");
        }

        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string cipherText)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();

        aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);

        byte[] iv = new byte[Constants.Encryption.IvSize];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream(
            fullCipher,
            Constants.Encryption.IvSize,
            fullCipher.Length - Constants.Encryption.IvSize);

        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);

        return streamReader.ReadToEnd();
    }

    public Result<ConnectionCredentialsDto> DecryptConnectionCredentials(User user)
    {
        ConnectionCredentials connectionCredentials = user.ConnectionCredentials;

        string decryptedConnectionHash = Decrypt(connectionCredentials.ConnectionHash);

        try
        {
            var dto = JsonConvert.DeserializeObject<ConnectionCredentialsDto>(decryptedConnectionHash);

            if (dto != null)
            {
                return Result<ConnectionCredentialsDto>.Ok(dto);
            }
        }
        catch (Exception)
        {
            return Result<ConnectionCredentialsDto>.Fail("Could not deserialize current connection credentials.");
        }

        return Result<ConnectionCredentialsDto>.Fail("Could not access connection credentials.");
    }
}
