using DatabaseReportingSystem.Shared.Models;

namespace DatabaseReportingSystem.Shared;

public interface IEncryptor
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string hashedPassword);

    string Encrypt(string text);

    string Decrypt(string text);

    Result<ConnectionCredentialsDto> DecryptConnectionCredentials(User user);
}
