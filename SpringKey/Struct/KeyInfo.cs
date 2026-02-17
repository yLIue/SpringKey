using SpringKey.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringKey.Struct
{
    internal class KeyInfo
    {
        public string Title { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string Hash { get; set; }
        public string Group { get; set; }

        private string Null = "Null";
        public KeyInfo(KeyFile _key, string _hash, string _group)
        {
            Title = _key.Title;
            Account = _key.Account;
            Password = _key.Password;
            Hash = _hash;
            Group = _group;
        }

        public KeyInfo()
        {
            Title = Null;
            Account = Null;
            Password = Null;
            Hash = Null;
            Group = Null;
        }
    }
}
