using System.IO;

namespace G42.AesEncryption.Core
{
    public interface IEncryptionHelper
    {
        Stream Encrypt(Stream plainTextStream);
        Stream Decrypt(Stream encryptedStream);
        string EncryptAsBase64(string plainText);
        string DecryptAsBase64(string base64Input);
    }
}
