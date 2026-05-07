using SpringKey.Core;
using SpringKey.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpringKey.Files
{
    // 关于这个文件的的介绍
    // 采用userKey构造,将userKey的哈希和userName和user的index文件的路径进行关联
    // 提供更改userName的功能
    // 可返回index文件
    public class LinkFile
    {
        private string RootPath;
        private string DataPath;
        private string UserHash;
        private string FilePath;
        public string UserName;
        public string UserPath = "";
        public string Hash => UserHash;
        private const string Default = "default";

        public LinkFile(string _appPath, string _userKey, string _userName = Default) 
        {
            RootPath = Path.Combine(_appPath, "link");
            if (!Directory.Exists(RootPath)) Directory.CreateDirectory(RootPath);
            DataPath = Path.Combine(_appPath, "data");
            if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);

            UserHash = SkHash.GetFileHash($"<SpringKey>{_userKey}</SpringKey>");
            FilePath = GetFilePath(UserHash);
            UserName = _userName;

            if (!File.Exists(FilePath))
                Init();
            else
                Load();
        }

        private void Init()
        {
            if(UserName == Default)
                UserName = AutoUserName();
            UserPath = Path.Combine(DataPath, UserName);
            if (!Directory.Exists(UserPath)) Directory.CreateDirectory(UserPath);
            Updata();
        }

        private void Load()
        {
            string[] data = File.ReadAllLines(FilePath, Encoding.UTF8)[0].Split(' ');
            UserName = data[0];
            UserPath = data[1];
        }

        public void Rename(string newUserName)
        {
            if (UserName == newUserName) return;
            string newPath = Path.Combine(DataPath, newUserName);
            if (Directory.Exists(UserPath))
                Directory.Move(UserPath, newPath);
            else if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            UserName = newUserName;
            UserPath = newPath;
            Updata();
        }

        #region utils
        private string GetFilePath(string _userHash)
        {
            return Path.Combine(RootPath, _userHash + ".sklink");
        }

        private string AutoUserName()
        {
            string[] names =
            {
                "杨间",
                "姜小白",
                "瑾旦",
                "杨月",
                "沐雪"
            };
            Random random = new Random();
            int index = random.Next(names.Length);
            return names[index];
        }

        private void Updata()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(UserName);
            sb.Append(" ");
            sb.Append(UserPath);
            File.WriteAllText(FilePath, sb.ToString(), Encoding.UTF8);
        }
        #endregion
    }
}
