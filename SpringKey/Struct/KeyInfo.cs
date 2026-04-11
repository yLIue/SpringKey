using SpringKey.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringKey.Struct
{
    public class KeyInfo
    {
        public string Title { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string Hash { get; set; }
        public string Group { get; set; }

        private readonly string _null = "Null";
        public KeyInfo(KeyFile key, string hash, string group)
        {
            Title = key.Title;
            Account = key.Account;
            Password = key.Password;
            Hash = hash;
            Group = group;
        }

        public KeyInfo()
        {
            Title = _null;
            Account = _null;
            Password = _null;
            Hash = _null;
            Group = _null;
        }
    }
}
