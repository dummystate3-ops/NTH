using System.Threading.Tasks;

namespace NovaToolsHub.Services;

public interface IEncryptionService
{
    Task<string> EncryptAsync(string plainText, string password);
    Task<string> DecryptAsync(string cipherText, string password);
}
