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
        public string Place { get; set; }
        public List<string> PasswordPrev { get; set; } = new();
        public string Description { get; set; }
        public Dictionary<string, string> Binding { get; set; }
        public string Hash { get; set; }
        public string Group { get; set; }

        private readonly string _null = "Null";
        public KeyInfo(KeyFile key, string hash, string group)
        {
            Title = key.Title;
            Account = key.Account;
            Password = key.Password;
            Place = key.Place;
            PasswordPrev = new List<string>(key.PasswordPrev);
            Description = key.Description;
            Binding = new Dictionary<string, string>(key.Binding);
            Hash = hash;
            Group = group;
        }

        public KeyInfo()
        {
            Title = _null;
            Account = _null;
            Password = _null;
            Place = _null;
            PasswordPrev = new List<string>();
            Description = "";
            Binding = new Dictionary<string, string>();
            Hash = _null;
            Group = _null;
        }
    }
}
