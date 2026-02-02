using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SpringKey.Models
{
    public struct SkHash
    {
        public static string GetFileHash(byte[] _bytes)
        {
            using var sha = SHA256.Create();
            String hash = BitConverter.ToString(sha.ComputeHash(_bytes))
                .Replace("-", " ").ToLower();
            return hash;
        }
    }
}
