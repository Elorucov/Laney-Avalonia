using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ELOR.Laney.Core {
    public class Encryption {
        public static bool IsChaCha20Poly1305Supported { get { return ChaCha20Poly1305.IsSupported; } }
        // public static bool IsChaCha20Poly1305Supported { get { return false; } }

        public static Tuple<string, string, string> Encrypt(byte[] key, string plainText) {
            if (IsChaCha20Poly1305Supported) {
                using (ChaCha20Poly1305 cha = new ChaCha20Poly1305(key)) {
                    byte[] p = Encoding.UTF8.GetBytes(plainText);
                    byte[] e = new byte[p.Length];
                    byte[] n = p.TakeLast(12).ToArray();
                    byte[] t = new byte[16];
                    cha.Encrypt(n, p, e, t);
                    return new Tuple<string, string, string>(Convert.ToBase64String(e), Convert.ToBase64String(n), Convert.ToBase64String(t));
                }
            } else {
                using (var aes = Aes.Create()) {
                    aes.Key = key;
                    aes.GenerateIV();
                    string vector = Convert.ToBase64String(aes.IV);

                    ICryptoTransform encryptor = aes.CreateEncryptor();
                    byte[] encryptedData;

                    using (MemoryStream ms = new MemoryStream()) {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                            using (StreamWriter sw = new StreamWriter(cs)) sw.Write(plainText);
                            encryptedData = ms.ToArray();
                        }
                    }

                    return new Tuple<string, string, string>(Convert.ToBase64String(encryptedData), vector, String.Empty);
                }
            }
        }

        public static string Decrypt(byte[] key, string cipherText, string nonce, string tag) {
            if (IsChaCha20Poly1305Supported) {
                using (ChaCha20Poly1305 cha = new ChaCha20Poly1305(key)) {
                    byte[] e = Convert.FromBase64String(cipherText);
                    byte[] p = new byte[e.Length];
                    byte[] n = Convert.FromBase64String(nonce);
                    byte[] t = Convert.FromBase64String(tag);
                    cha.Decrypt(n, e, t, p);
                    return Encoding.UTF8.GetString(p);
                }
            } else {
                using (var aes = Aes.Create()) {
                    aes.Key = key;
                    aes.IV = Convert.FromBase64String(nonce);

                    ICryptoTransform decryptor = aes.CreateDecryptor();
                    string decrypted = String.Empty;

                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText))) {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {
                            using (StreamReader sr = new StreamReader(cs)) decrypted = sr.ReadToEnd();
                        }
                    }

                    return decrypted;
                }
            }
        }
    }
}