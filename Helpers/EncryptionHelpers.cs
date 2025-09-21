using System.Security.Cryptography;
using System.Text;

namespace NGO_Web_Demo.Helpers
{
    public static class EncryptionHelper
    {
        // You must store your encryption key and IV securely.
        // **DO NOT hardcode them like this in a production application.**
        // This is for demonstration purposes only.
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("YourSecretKey12345"); // Must be 16, 24, or 32 bytes
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("YourSecretIV678901"); // Must be 16 bytes

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;

            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
    }
}