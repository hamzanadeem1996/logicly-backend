using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Apex.DataAccess
{
    public static class Encryption
    {
        public const string Key = "LpxenIGdqA0jMgrvFzpBdeFKtXUNNKMx";

        private static byte[] mkey(string skey)
        {
            byte[] key = Encoding.UTF8.GetBytes(skey);
            byte[] k = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < key.Length; i++)
            {
                k[i % 16] = (byte)(k[i % 16] ^ key[i]);
            }

            return k;
        }

        public static string Encrypt(string input)
        {
            if (String.IsNullOrEmpty(input)) return "";
            var aes = new RijndaelManaged
            {
                KeySize = 128,
                BlockSize = 128,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
                Key = mkey(Encryption.Key),
                IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };

            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Encoding.UTF8.GetBytes(input);
                    cs.Write(xXml, 0, xXml.Length);
                    cs.FlushFinalBlock();
                }

                xBuff = ms.ToArray();
            }

            return xBuff.ToHexString();
        }

        public static string Decrypt(string input)
        {
            if (String.IsNullOrEmpty(input)) return "";
            RijndaelManaged aes = new RijndaelManaged
            {
                KeySize = 128,
                BlockSize = 128,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
                Key = mkey(Encryption.Key),
                IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };

            var decrypt = aes.CreateDecryptor();
            byte[] encryptedStr = input.FromHex2ByteArray();

            string Plain_Text;

            using (var ms = new MemoryStream(encryptedStr))
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                    {
                        Plain_Text = reader.ReadToEnd();
                    }
                }
            }
            return Plain_Text;
        }
    }

    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public static byte[] FromHex2ByteArray(this string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        private static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}