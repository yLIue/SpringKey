using SpringKey.Files;
using SpringKey.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using SpringKey.Struct;

namespace SpringKey.Test
{
    // 其实我觉得这个可以写前端的在前端测试，这样更直观
    // 这个测试只是测试头一次的，多次运行可能会出现问题
    internal class IndexFileTest
    {
        public static void Test()
        {
            var userData = new TestData();
            string userPath = Init(userData.UserName);
            
            InitIndex(userPath, userData);
            LoadIndex(userPath, userData);
        }

        

        private static void LoadIndex(string _userPath, TestData userData)
        {
            var index = new IndexFile(_userPath, userData.UserKey);
            // 1.获取分组
            string group = index.GroupIndex[1];
            // 2.index获取分组下的key
            List<KeyInfo> keys = index.GetGroupInfo(group);
            // 3.修改key
            //  3.1.修改内容
            KeyFile newKey = new KeyFile(
                userData.Key1.Title + "_new",
                userData.Key1.Account + "_new",
                userData.Key1.Password + "_new"
                );
            index.UpdataKey(keys[0], newKey);
            //  3.2 修改分组
            //      3.2.1移动分组
            index.MoveGroup(keys[0], "新分组");
            //      3.2.2.添加到分组
            index.AddGroup(keys[0], "二次备份");
            //      3.2.3.从分组移除
            index.RemoveGroup(keys[0]);

        }

        private static void InitIndex(string _userPath,TestData userData)
        {
            var dataKey = userData.Key1;
            // 用户层面点击确认后构造
            var key = new KeyFile(
                dataKey.Title,
                dataKey.Account,
                dataKey.Password
                );

            // 用户保存
            var index = new IndexFile(_userPath, userData.UserKey);
            index.AddKey(key);
        }

        #region utils
        private static string Init(string _userName)
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string testPath = Path.Combine(appPath, "test");
            if (!Directory.Exists(testPath)) Directory.CreateDirectory(testPath);

            string dataPath = Path.Combine(testPath, "data");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);

            string userPath = Path.Combine(dataPath, _userName);
            if (!Directory.Exists(userPath)) Directory.CreateDirectory(userPath);
            return userPath;
        }
        #endregion
    }
}
