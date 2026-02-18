using SpringKey.Files;
using SpringKey.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringKey.Struct;

namespace SpringKey.Test
{
    internal class LinkFileTest
    {
        public static void Test()
        {
            var userData = new TestData();
            string testPath = GetTestPath();

            Init(testPath, userData);
            Load(testPath, userData);

        }

        private static void Load(string _testPath, TestData _userData)
        {
            LinkFile link = new LinkFile(_testPath, _userData.UserKey);
            IndexFile index = new IndexFile(link.UserPath, _userData.UserKey);
            List<KeyInfo> Keys = index.GetGroupInfo(index.GroupIndex[0]);
            foreach (var item in Keys)
            {
                Log.print($"group: {item.Group}\ntitle: {item.Title}");
            }
        }

        private static void Init(string _testPath, TestData _userData)
        {
            LinkFile link = new LinkFile(_testPath, _userData.UserKey);
            IndexFile index = new IndexFile(link.UserPath, _userData.UserKey);

            // index的测试

            KeyFile testKey = new KeyFile(
                _title: _userData.Key1.Title,
                _account: _userData.Key1.Account,
                _password: _userData.Key1.Password
                );
            index.AddKey(testKey);
        }

        private static string GetTestPath()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string testPath = Path.Combine(appPath, "test");
            if (!Directory.Exists(testPath)) Directory.CreateDirectory(testPath);
            return testPath;
        }
    }
}
