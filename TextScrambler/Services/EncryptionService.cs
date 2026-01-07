using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TextScrambler.Services
{
    public class EncryptionService
    {
        private const int KeySize = 256;
        private const int BlockSize = 128;
        private const int DerivationIterations = 10000;

        // In a real app, we might want to let the user choose a salt or random salt per message.
        // For simplicity and "reproducibility" with just a PIN, we use a fixed salt or derive it.
        // However, secure practice implies random salt.
        // If we use random salt, we must prepend it to the ciphertext.
        // Format: [Salt(16)][IV(16)][Ciphertext] -> Base64

        public string Encrypt(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password cannot be empty");

            // Generate random salt
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // Derive key
            using var kdf = new Rfc2898DeriveBytes(password, salt, DerivationIterations, HashAlgorithmName.SHA256);
            byte[] key = kdf.GetBytes(KeySize / 8);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();

            // Write Salt and IV first
            ms.Write(salt, 0, salt.Length);
            ms.Write(iv, 0, iv.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            // Use InsertLineBreaks to prevent Notepad freezing on very long single lines
            return Convert.ToBase64String(ms.ToArray(), Base64FormattingOptions.InsertLineBreaks);
        }

        public string Decrypt(string cipherText, string password)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password cannot be empty");

            try
            {
                byte[] fullCipher = Convert.FromBase64String(cipherText);

                // Min length check: Salt(16) + IV(16) = 32
                if (fullCipher.Length < 32) return null; // Invalid

                using var ms = new MemoryStream(fullCipher);

                byte[] salt = new byte[16];
                ms.Read(salt, 0, salt.Length);

                byte[] iv = new byte[16];
                ms.Read(iv, 0, iv.Length);

                using var kdf = new Rfc2898DeriveBytes(password, salt, DerivationIterations, HashAlgorithmName.SHA256);
                byte[] key = kdf.GetBytes(KeySize / 8);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
            catch
            {
                // Decryption failed (wrong password or corrupted text)
                return null;
            }
        }
    }
}
