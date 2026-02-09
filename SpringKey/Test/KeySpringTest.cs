using SpringKey.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringKey.Test
{
    internal static class KeySpringTest
    {
        public static void Test()
        {
            var userData = new TestData();
            string data = KeyStructTest.CreateKeyStruct();
            var keySpring = new KeySpring();
            string keyData = keySpring.EncryptString(data, userData.UserKey);
            string loadData = keySpring.DecryptToString(keyData, userData.UserKey);
            if (data != loadData)
            {
                Log.warning("Cryptography test failed!");
                Log.print("data: \n" + data);
                Log.print("loadData: \n" + loadData);
            }
            else
            {
                Log.print("[1]KeySpringTest is ok!!!");
            }
        }
    }
}
