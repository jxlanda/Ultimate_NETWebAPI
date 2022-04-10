using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NET.WebAPI.Extensions
{
    public class EncryptionService
    {
        private readonly string _key;
        private readonly IConfiguration _configuration;
        public EncryptionService(IConfiguration configuration)
        {
            _configuration = configuration;
            _key = _configuration["Encryption:EncryptKey"];
        }

        public string Decrypt(string cipherText)
        {
            string password = _key;
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using Aes encryptor = Aes.Create();
            var salt = cipherBytes.Take(16).ToArray();
            var iv = cipherBytes.Skip(16).Take(16).ToArray();
            var encrypted = cipherBytes.Skip(32).ToArray();
            Rfc2898DeriveBytes pdb = new(password, salt, 100);
            encryptor.Key = pdb.GetBytes(32);
            encryptor.Padding = PaddingMode.PKCS7;
            encryptor.Mode = CipherMode.CBC;
            encryptor.IV = iv;
            using MemoryStream ms = new(encrypted);
            using CryptoStream cs = new(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cs, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        public string EncryptString(string plainText, string key = null)
        {
            if (key == null) key = _key;

            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new((Stream)cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        }
    }
}
