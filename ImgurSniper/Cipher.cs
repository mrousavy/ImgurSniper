using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ImgurSniper {
    //Cipher En/Decryption - Thanks to http://stackoverflow.com/users/57477/craigtp !
    public static class Cipher {
        private const int Keysize = 256;
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase) {
            try {
                byte[] saltStringBytes = Generate256BitsOfRandomEntropy();
                byte[] ivStringBytes = Generate256BitsOfRandomEntropy();
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                using (
                    Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes,
                        DerivationIterations)) {
                    byte[] keyBytes = password.GetBytes(Keysize / 8);
                    using (RijndaelManaged symmetricKey = new RijndaelManaged()) {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes)) {
                            using (MemoryStream memoryStream = new MemoryStream()) {
                                using (
                                    CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor,
                                        CryptoStreamMode.Write)) {
                                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                    cryptoStream.FlushFinalBlock();
                                    byte[] cipherTextBytes = saltStringBytes;
                                    cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                    cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Convert.ToBase64String(cipherTextBytes);
                                }
                            }
                        }
                    }
                }
            }
            catch {
                return null;
            }
        }

        public static string Decrypt(string cipherText, string passPhrase) {
            try {
                byte[] cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
                byte[] saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
                byte[] ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
                byte[] cipherTextBytes =
                    cipherTextBytesWithSaltAndIv.Skip(Keysize / 8 * 2)
                        .Take(cipherTextBytesWithSaltAndIv.Length - Keysize / 8 * 2)
                        .ToArray();

                using (
                    Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes,
                        DerivationIterations)) {
                    byte[] keyBytes = password.GetBytes(Keysize / 8);
                    using (RijndaelManaged symmetricKey = new RijndaelManaged()) {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes)) {
                            using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes)) {
                                using (
                                    CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor,
                                        CryptoStreamMode.Read)) {
                                    byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                    int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                }
                            }
                        }
                    }
                }
            }
            catch {
                return null;
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy() {
            byte[] randomBytes = new byte[32];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider()) {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}