using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpringKey.Struct;

namespace SpringKey.Models
{
    internal class Test
    {
        public string UserKey = "userkey";
        public string UserName = "uesr";
        // link -> .skindex(hash) -> hash.skkey
        // 构建 -> info... -> string
        // 读取 -> string -> key

        public Key KeyStructTest()
        {
            string testFile = "KeyStruct.struct";
            string userPath = GetUserPath(UserName);
            string testFilePath = Path.Combine(userPath, testFile);

            Key key = new Key("mira", "110119120", "this_password");
            key.Description = "我的mira账号\nuser";
            key.AddTag("音乐");
            key.AddTag("账号");
            key.AddTag("life");
            key.RemoveTag("life");
            key.RenameTag("账号", "文件");
            string value = key.GetStringKey();

            SaveFile(testFilePath, value);

            return key;
        }
        private bool SaveFile(string _path, string _value)
        {
            if (File.Exists(_path)) File.Delete(_path);
            File.WriteAllText(_path, _value, Encoding.UTF8);
            return true;
        }
        private string GetUserPath(string _userName)
        {
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string testPath = Path.Combine(rootPath, ".test");
            InitDir(testPath);
            string dataPath = Path.Combine(testPath, "data");
            InitDir(dataPath);
            string userPath = Path.Combine(dataPath, _userName);
            InitDir(userPath);
            return userPath;
        }

        private void InitDir(string _paht)
        {
            if (!Directory.Exists(_paht)) Directory.CreateDirectory(_paht);
        }
    }
}
