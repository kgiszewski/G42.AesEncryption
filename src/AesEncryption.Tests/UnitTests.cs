using System;
using System.IO;
using System.Reflection;
using G42.AesEncryption.Core;
using NUnit.Framework;

namespace G42.AesEncryption.Tests
{
    [TestFixture]
    public class UnitTests
    {
        private IEncryptionHelper _encryptionHelper;

        [SetUp]
        public void Init()
        {
            _encryptionHelper = new AesEncryptionHelper();

            //for test purposes, we're generating a new key each time
            AesEncryptionHelper.CipherKey = AesEncryptionHelper.GenerateCipherKey();
        }

        [Ignore("Used to generate a key")]
        [Test]
        public void Generate_Key()
        {
            var key = AesEncryptionHelper.GenerateCipherKey();
        }

        [Test]
        public void Can_Encrypt_And_Decrypt_String()
        {
            var plainText = "https://www.nsa.gov/";

            var cipherText = _encryptionHelper.EncryptAsBase64(plainText);

            Assert.AreNotEqual(plainText, cipherText);

            var decryptedText = _encryptionHelper.DecryptAsBase64(cipherText);

            Assert.AreEqual(plainText, decryptedText);
        }

        [Test]
        public void Can_Encrypt_And_Decrypt_String_With_ConfiguredKey()
        {
            var plainText = "https://www.nsa.gov/";

            AesEncryptionHelper.CipherKey = null;

            var cipherText = _encryptionHelper.EncryptAsBase64(plainText);

            Assert.AreNotEqual(plainText, cipherText);

            var decryptedText = _encryptionHelper.DecryptAsBase64(cipherText);

            Assert.AreEqual(plainText, decryptedText);
        }

        [Test]
        public void Can_Encrypt_And_Decrypt_Streams()
        {
            using (var stream = GetResourceStream("G42.AesEncryption.Tests.Images.nsa.png"))
            {
                var base64PlainText = Convert.ToBase64String(_toByteArray(stream));

                stream.Position = 0;

                using (var encryptedStream = _encryptionHelper.Encrypt(stream))
                {
                    var base64CipherText = Convert.ToBase64String(_toByteArray(encryptedStream));

                    Assert.AreNotEqual(base64CipherText, base64PlainText);

                    encryptedStream.Position = 0;

                    var decryptedStream = _encryptionHelper.Decrypt(encryptedStream);

                    var base64DecryptedPlainText = Convert.ToBase64String(_toByteArray(decryptedStream));

                    Assert.AreEqual(base64PlainText, base64DecryptedPlainText);
                }
            }
        }

        private Stream GetResourceStream(string resourceLocation, Type type = null)
        {
            var assembly = type == null ? Assembly.GetExecutingAssembly() : Assembly.GetAssembly(type);

            return assembly.GetManifestResourceStream(resourceLocation);
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
