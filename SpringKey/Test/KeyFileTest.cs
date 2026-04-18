using SpringKey.Core;
using SpringKey.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringKey.Test
{
    internal static class KeyFileTest
    {
        public static void Test()
        {
            string keyValue = CreateKeyStruct();
            string keyLoad = "";
            // 读取失败
            if (!LoadKeyStruct(keyValue, ref keyLoad))
                return;

            if (keyValue != keyLoad)
            {
                Log.warning("Key struct test failed!");
                Log.print("keyValue: \n" + keyValue);
                Log.print("keyLoad: \n" + keyLoad);
            }
            else
            {
                Log.print("[0]KeyStructTest is ok!!!");
            }

        }

        public static string CreateKeyStruct()
        {
            var userData = new TestData();
            KeyFile key = new KeyFile(userData.Key1.Title, userData.Key1.Account, userData.Key1.Password);
            key.Description = userData.Key1.Description;
            foreach (string tag in userData.Key1.Tags)
                key.AddTag(tag);
            key.AddTag("life");
            key.RemoveTag("life");
            key.AddTag("账号");
            key.RenameTag("账号", "文件");
            string value = key.GetStringKey();

            return value;
        }

        private static bool LoadKeyStruct(string _data, ref string _loadKey)
        {
            KeyFile key = KeyFile.LoadKey(_data);
            if (!key.IsKey())
                return false;
            _loadKey = key.GetStringKey();
            return true;
        }
    }
}
