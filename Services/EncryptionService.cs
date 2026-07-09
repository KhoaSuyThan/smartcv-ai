using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace DoAnCS.Services
{
    public interface IEncryptionService
    {
        // Mã hóa chuỗi văn bản sang định dạng Base64 kèm tiền tố ENC:
        string Encrypt(string plainText);
        // Giải mã chuỗi văn bản nếu có tiền tố ENC:, ngược lại trả về chuỗi gốc
        string Decrypt(string cipherText);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private const string Prefix = "ENC:";

        public EncryptionService(IConfiguration configuration)
        {
            // Lấy khóa cấu hình từ appsettings.json, nếu không có sẽ tự động dùng khóa mặc định an toàn
            string configKey = configuration["Encryption:Key"] ?? "SmartCV_AI_SecretKey_2026_Secure";
            string configIv = configuration["Encryption:IV"] ?? "SmartCV_Init_Vect";

            // Đảm bảo độ dài Key là 32 bytes (AES-256) và IV là 16 bytes (AES-128 block size)
            _key = GetBytes(configKey, 32);
            _iv = GetBytes(configIv, 16);
        }

        private byte[] GetBytes(string input, int length)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] result = new byte[length];
            Array.Copy(bytes, result, Math.Min(bytes.Length, length));
            return result;
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return plainText;
            if (plainText.StartsWith(Prefix)) return plainText; // Đã được mã hóa từ trước

            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Prefix + Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText)) return cipherText;

            // Nếu không có tiền tố ENC:, coi như đây là Plain Text cũ chưa được mã hóa (hỗ trợ chuyển đổi tự động)
            if (!cipherText.StartsWith(Prefix)) return cipherText;

            string encryptedData = cipherText.Substring(Prefix.Length);

            try
            {
                byte[] buffer = Convert.FromBase64String(encryptedData);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EncryptionError] Không thể giải mã API Key: {ex.Message}");
                return cipherText; // Fallback trả về nguyên bản để hệ thống không bị crash
            }
        }
    }
}
