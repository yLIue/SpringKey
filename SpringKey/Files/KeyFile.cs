using SpringKey.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SpringKey.Files
{
    public class KeyFile
    {
        public const string KeyVersion = "skkey_ver0.1";
        public string Title { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string Description { get; set; } = "";

        private List<String> tags = new List<string>();
        public IReadOnlyList<String> Tags => tags.AsReadOnly();
        public KeyFile(string _title, string _account, string _password) {
            Title = _title;
            Account = _account;
            Password = _password;
        }

        #region api
        // 判断是否是有效的Key
        public bool IsKey()
        {
            if (Title == "" || Password == "" || Account == "")
                return false;
            return true;
        }

        // 加密并输出字符串
        public string Save(string _userKey)
        {
            var keySpring = new KeySpring();
            return keySpring.EncryptString(GetStringKey(), _userKey);
        }

        #region tag
        public bool AddTag(string _tag)
        {
            if (string.IsNullOrWhiteSpace(_tag)) return false;
            _tag = _tag.Trim();
            if (tags.Contains(_tag)) return false;
            tags.Add(_tag);
            return true;
        }

        public bool RemoveTag(string _tag) => tags.Remove(_tag);

        public bool RenameTag(string _oldTag, string _newTag)
        {
            if (string.IsNullOrWhiteSpace(_newTag)) return false;
            int indexFind = tags.IndexOf(_oldTag);
            if (indexFind < 0) return false;
            tags[indexFind] = _newTag.Trim();
            return true;
        }
        #endregion

        // 输出字符串
        public string GetStringKey()
        {
            var strBud = new StringBuilder();
            void StrBudAppend(string _title, string _values)
            {
                strBud.AppendLine($"[{_title}]");
                foreach(var value in _values.Split('\n'))
                    strBud.AppendLine('\t' + value);

            }
            strBud.AppendLine(KeyVersion);
            StrBudAppend("title", Title);
            StrBudAppend("account", Account);
            StrBudAppend("password", Password);
            StrBudAppend("description", Description);
            StrBudAppend("tags", string.Join('\n', tags));
            return strBud.ToString();
        }

        #region LoadKey
        public static KeyFile LoadKey(string _data)
        {
            KeyFile key = new KeyFile("", "", "");
            switch (_data.Split('\n')[0].TrimEnd('\r'))
            {
                case "skkey_ver0.1": LoadKeyVer01(key, _data); break;
            }
            return key;
        }

        private static bool LoadKeyVer01(KeyFile _key, string _data)
        {
            StringBuilder description = new StringBuilder();
            string sec = "";
            void Push(string _value)
            {
                if (sec == "") return;
                switch (sec)
                {
                    case "title": _key.Title = _value; break;
                    case "account": _key.Account = _value; break;
                    case "password": _key.Password = _value; break;
                    case "description": description.Append($"{_value}\n"); break;
                    case "tags": _key.AddTag(_value); break;
                }
            }

            foreach (var stepLine in _data.Split('\n'))
            {
                var line = stepLine.TrimEnd('\r');
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    sec = line.Trim('[', ']');
                }
                else if (line.StartsWith("\t"))
                {
                    Push(line.Substring(1));
                }
            }
            _key.Description = description.ToString().TrimEnd('\n');
            return true;
        }
        #endregion

        #endregion //api
    }
}
