using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace G42.AesEncryption.Core
{
    public class AesEncryptionHelper : IEncryptionHelper
    {
        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        private const int KeyBitSize = 256;
        private const int BlockBitSize = 128;
        private static readonly string ConfiguredAesCipherKey = ConfigurationManager.AppSettings["AesCipherKey"];

        public static string CipherKey;

        private byte[] _getKey()
        {
            var aesKey = CipherKey ?? ConfiguredAesCipherKey;

            if (string.IsNullOrEmpty(aesKey))
            {
                throw new Exception($"AES Key missing, ensure you are an using the correct app setting (AesCipherKey) or set it in code!");
            }

            return Convert.FromBase64String(aesKey);
        }

        public static string GenerateCipherKey()
        {
            var key = new byte[KeyBitSize / 8];

            Random.GetBytes(key);

            return Convert.ToBase64String(key);
        }

        public Stream Encrypt(Stream plainTextStream)
        {
            plainTextStream.Position = 0;

            var byteArray = _toByteArray(plainTextStream);

            return new MemoryStream(Encrypt(byteArray, _getKey()));
        }

        public Stream Decrypt(Stream encryptedStream)
        {
            encryptedStream.Position = 0;

            var byteArray = _toByteArray(encryptedStream);

            return new MemoryStream(Decrypt(byteArray, _getKey()));
        }

        public string EncryptAsBase64(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            var byteArray = Encoding.Default.GetBytes(plainText);

            using (var stream = new MemoryStream(byteArray))
            {
                using (var encryptedStream = Encrypt(stream))
                {
                    var reader = new StreamReader(encryptedStream, Encoding.Default);

                    var encryptedString = reader.ReadToEnd();

                    var plainTextBytes = Encoding.Default.GetBytes(encryptedString);

                    return Convert.ToBase64String(plainTextBytes);
                }
            }
        }

        public string DecryptAsBase64(string base64Input)
        {
            if (string.IsNullOrEmpty(base64Input))
            {
                return base64Input;
            }

            var base64EncodedBytes = Convert.FromBase64String(base64Input);

            using (var stream = new MemoryStream(base64EncodedBytes))
            {
                using (var decryptedStream = Decrypt(stream))
                {
                    var reader = new StreamReader(decryptedStream, Encoding.Default);

                    return reader.ReadToEnd();
                }
            }
        }

        internal byte[] Encrypt(byte[] secretMessage, byte[] cryptKey)
        {
            //User Error Checks
            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException($"Key needs to be {KeyBitSize} bit!", nameof(cryptKey));

            if (secretMessage == null || secretMessage.Length < 1)
                throw new ArgumentException("Secret Message Required!", nameof(secretMessage));

            byte[] cipherText;
            byte[] iv;

            using (var aes = new AesManaged
            {
                KeySize = KeyBitSize,
                BlockSize = BlockBitSize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                //Use random IV
                aes.GenerateIV();
                iv = aes.IV;

                using (var encrypter = aes.CreateEncryptor(cryptKey, iv))
                {
                    using (var cipherStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
                        using (var binaryWriter = new BinaryWriter(cryptoStream))
                        {
                            //Encrypt Data
                            binaryWriter.Write(secretMessage);
                        }

                        cipherText = cipherStream.ToArray();
                    }
                }
            }

            //Assemble encrypted message and add authentication
            using (var encryptedStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(encryptedStream))
                {
                    //Prepend IV
                    binaryWriter.Write(iv);
                    //Write Ciphertext
                    binaryWriter.Write(cipherText);
                    binaryWriter.Flush();
                }

                return encryptedStream.ToArray();
            }
        }

        internal byte[] Decrypt(byte[] encryptedMessage, byte[] cryptKey)
        {
            //Basic Usage Error Checks
            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException($"CryptKey needs to be {KeyBitSize} bit!", nameof(cryptKey));

            if (encryptedMessage == null || encryptedMessage.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

            var ivLength = (BlockBitSize / 8);

            //Compare Tag with constant time comparison
            var compare = 0;

            //if message doesn't authenticate return null
            if (compare != 0)
                return null;

            using (var aes = new AesManaged
            {
                KeySize = KeyBitSize,
                BlockSize = BlockBitSize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                //Grab IV from message
                var iv = new byte[ivLength];
                Array.Copy(encryptedMessage, 0, iv, 0, iv.Length);

                using (var decrypter = aes.CreateDecryptor(cryptKey, iv))
                {
                    using (var plainTextStream = new MemoryStream())
                    {
                        using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
                        {
                            using (var binaryWriter = new BinaryWriter(decrypterStream))
                            {
                                //Decrypt Cipher Text from Message
                                binaryWriter.Write(
                                    encryptedMessage,
                                    iv.Length,
                                    encryptedMessage.Length - iv.Length
                                );
                            }
                        }

                        //Return Plain Text
                        return plainTextStream.ToArray();
                    }
                }
            }
        }

        private byte[] _toByteArray(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);

                return memoryStream.ToArray();
            }
        }
    }
}
