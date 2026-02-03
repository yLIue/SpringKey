using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringKey.Models;

namespace SpringKey.Struct
{
    class Key
    {
        public const string KeyVersion = "skkey_ver0.1";
        public string Title { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string Description { get; set; } = "";

        private List<String> tags = new List<string>();
        public IReadOnlyList<String> Tags => tags.AsReadOnly();
        public Key(string _title, string _account, string _password) {
            Title = _title;
            Account = _account;
            Password = _password;
        }

        #region api

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




        #region 输出字符串
        public string GetStringKey()
        {
            var strBud = new StringBuilder();
            void StrBudAppend(string _title, string _values)
            {
                strBud.AppendLine($"[{_title}]");
                foreach(var value in _values.Split('\n'))
                    strBud.AppendLine('\t' + value);

            }
            StrBudAppend("keyVersion", KeyVersion);
            StrBudAppend("title", Title);
            StrBudAppend("account", Account);
            StrBudAppend("password", Password);
            StrBudAppend("description", Description);
            StrBudAppend("tags", string.Join('\n', tags));
            return strBud.ToString();
        }
        #endregion

        #endregion

    }
}
