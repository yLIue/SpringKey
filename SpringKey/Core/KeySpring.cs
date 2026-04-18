using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SpringKey.Core
{
    internal class KeySpring
    {
        private const byte Version = 0x01;
        private const int SaltSize = 16;
        private const int NonceSize = 12;
        private const int SpringKeySize = 16;
        private const int TagSize = 16;
        private const int PBKDF2_Iterations = 200_000;
        private const int HeaderSize = 1 + SaltSize + NonceSize;

        #region 加密
        public string EncryptString(string _data, string _userKey)
        {
            byte[] data = Encoding.UTF8.GetBytes(_data);
            byte[] cipher = new byte[data.Length];

            byte[] salt = new byte[SaltSize];
            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            RandomNumberGenerator.Fill(salt);
            RandomNumberGenerator.Fill(nonce);

            // 头部：版本[1] + salt[16] + nonce[12] _共计 29 字节
            byte[] header = GetHeader(salt, nonce);

            byte[] springKey = GetSpringKey(_userKey, salt);
            var aesGcm = new AesGcm(springKey,TagSize);
            aesGcm.Encrypt(nonce, data, cipher, tag);

            // 拼接 版本[1] + salt[16] + nonce[12] + cipher[...] + tag[16]
            int pos = 0;
            int blobLength = HeaderSize + cipher.Length + TagSize;
            byte[] blob = new byte[blobLength];
            Buffer.BlockCopy(header, 0, blob, pos, HeaderSize); pos += HeaderSize;
            Buffer.BlockCopy(cipher, 0, blob, pos, cipher.Length); pos += cipher.Length;
            Buffer.BlockCopy(tag, 0, blob, pos, TagSize);

            return Convert.ToBase64String(blob);
        }

        private byte[] GetHeader(byte[] _salt, byte[] _nonce)
        {
            int pos = 0;
            byte[] header = new byte[HeaderSize];
            header[pos++] = Version;
            Buffer.BlockCopy(_salt, 0, header, pos, SaltSize); pos += SaltSize;
            Buffer.BlockCopy(_nonce, 0, header, pos, NonceSize);
            return header;
        }

        private byte[] GetSpringKey(string _userKey, byte[] _salt)
        {
            if (_userKey == null) throw new ArgumentNullException(nameof(_userKey));
            byte[] userKeyBytes = Encoding.UTF8.GetBytes(_userKey);
            var rfc = new Rfc2898DeriveBytes(userKeyBytes, _salt, PBKDF2_Iterations, HashAlgorithmName.SHA256);
            return rfc.GetBytes(SpringKeySize);
        }
        #endregion

        #region 解密
        public string DecryptToString(string _data, string _userKey)
        {
            byte[] blob = Convert.FromBase64String(_data);

            if (blob == null || blob.Length < HeaderSize + TagSize)
                throw new ArgumentException("Invalid blob", nameof(blob));

            int pos = 0;
            byte version = blob[pos++];
            if (version != Version) throw new NotSupportedException($"Blob version {version} not supported.");

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(blob, pos, salt, 0, SaltSize); pos += SaltSize;

            var nonce = new byte[NonceSize];
            Buffer.BlockCopy(blob, pos, nonce, 0, NonceSize); pos += NonceSize;

            int cipherLen = blob.Length - pos - TagSize;

            var cipher = new byte[cipherLen];
            Buffer.BlockCopy(blob, pos, cipher, 0, cipherLen); pos += cipherLen;

            var tag = new byte[TagSize];
            Buffer.BlockCopy(blob, pos, tag, 0, TagSize);

            byte[] data = new byte[cipherLen];
            byte[] springKey = GetSpringKey(_userKey, salt);
            var aesGcm = new AesGcm(springKey, TagSize);
            aesGcm.Decrypt(nonce, cipher, tag, data);
            
            return Encoding.UTF8.GetString(data);
        }
        #endregion
    }
}
