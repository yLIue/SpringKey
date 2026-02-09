using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringKey.Test
{
     internal class TestData
     {
        public class KeyData 
        { 
            public string Title { get; set; }
            public string Account { get; set; }
            public string Password { get; set; }
            public string Description { get; set; } = "";

            public List<String> Tags = new List<string>();

            public KeyData(string _title, string _account, string _password, string _description, string[] _tags) 
            { 
                Title = _title;
                Account = _account;
                Password = _password;
                Description = _description;
                Tags = _tags.ToList();
            }
        }

        public string UserKey = "userkey";
        public string UserName = "uesr";
        public KeyData Key1 = new KeyData(
            "mira",
            "110119120",
            "this_password",
            "我的mira账号\nuser",
            new string[] { "音乐", "娱乐" }
        );
     }
}
