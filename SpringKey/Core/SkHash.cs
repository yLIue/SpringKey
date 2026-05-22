using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SpringKey.Core
{
    public struct SkHash
    {
        public static string GetFileHash(string _data)
        {
            byte[] _bytes = Encoding.UTF8.GetBytes(_data);
            using var sha = SHA256.Create();
            String hash = BitConverter.ToString(sha.ComputeHash(_bytes))
                .Replace("-", "").ToLower()
                .Substring(0, 40);
            return hash;
        }
    }
}
