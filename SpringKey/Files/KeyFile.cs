using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpringKey.Struct;

namespace SpringKey.Files
{
    class KeyFile
    {
        private const string secureVersion = "sksecure_ver0.1";

        public const string SecureVersion = secureVersion;

        public string UserPath;

        public Key ken;
        public KeyFile(string _userPath)
        {
            UserPath = _userPath;
        }

        public void LoadFile(string _userPath, string _fileHash) {
            
        }

    }

}
