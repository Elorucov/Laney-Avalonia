using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ELOR.Laney.Core {
    public class Encryption {
        public static Tuple<string, string, string> Encrypt(byte[] key, string plainText) {
            using (ChaCha20Poly1305 cha = new ChaCha20Poly1305(key)) {
                byte[] p = Encoding.UTF8.GetBytes(plainText);
                byte[] e = new byte[p.Length];
                byte[] n = p.TakeLast(12).ToArray();
                byte[] t = new byte[16];
                cha.Encrypt(n, p, e, t);
                return new Tuple<string, string, string>(Convert.ToBase64String(e), Convert.ToBase64String(n), Convert.ToBase64String(t));
            }
        }

        public static string Decrypt(byte[] key, string cipherText, string nonce, string tag) {
            using (ChaCha20Poly1305 cha = new ChaCha20Poly1305(key)) {
                byte[] e = Convert.FromBase64String(cipherText);
                byte[] p = new byte[e.Length];
                byte[] n = Convert.FromBase64String(nonce);
                byte[] t = Convert.FromBase64String(tag);
                cha.Decrypt(n, e, t, p);
                return Encoding.UTF8.GetString(p);
            }
        }
    }
}