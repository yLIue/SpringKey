using SpringKey.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using SpringKey.Struct;
using SpringKey.ViewModel;

namespace SpringKey.Files
{
    // 构造:接受user路径，如果没有索引文件则创建一个新的索引文件，否则加载索引文件
    // 索引内容:版本号,各个分类,和总文件列表
    // 考虑功能
    // Init.创建索引文件的时候查看当前目录下有没有KeyFile文件，如果有则将它们加入索引文件中
    public class IndexFile
    {
        private const string Version = "skindex_ver0.1";
        private const string FileName = ".skindex";
        private string RootPath;
        private string IndexPath;
        private string UserKey;
        private Dictionary<string,List<string>> Groups = new Dictionary<string, List<string>>();
        private List<string> groupIndex = new List<string>();
        private string ALLGorup = "全部";
        private readonly Dictionary<string, KeyFile> _keyCache = new();
        private readonly KeySpring _keySpring = new();
        public IReadOnlyList<String> GroupIndex => groupIndex.AsReadOnly();

        public IndexFile()
        {
            
        }
        public IndexFile(string _userPath, string _userKey)
        {
            RootPath = _userPath;
            UserKey = _userKey;
            IndexPath = Path.Combine(RootPath, FileName);
            if (!File.Exists(IndexPath))
                Init();
            else
                Load();

        }

        private void Init()
        {
            CreateGroup(ALLGorup);
            CreateGroup("未分类");
        }

        #region Load
        private void Load()
        {
            string[] data = File.ReadAllLines(IndexPath, Encoding.UTF8);
            switch (data[0])
            {
                case "skindex_ver0.1": LoadIndexVer01(data.Skip(1).ToArray()); break;
            }
        }

        private void LoadIndexVer01(string[] lines)
        {
            string group = "";
            foreach (string line in lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    group = line.Trim('[', ']');
                    CreateGroup(group);
                }
                else
                {
                    Groups[group].Add(line);
                }
            }
        }
        #endregion

        #region api
        #region 关于key的api
        public void AddKey(KeyFile _key, string _class = "未分类")
        {
            
            string hash = KeySave(_key);
            _keyCache[hash] = _key;
            if (Groups["全部"].Contains(hash))
                return;
            if(_class == "全部")
                Groups["未分类"].Add(hash);
            else
                Groups["全部"].Add(hash);
            if (!Groups.ContainsKey(_class)) CreateGroup(_class);
            Groups[_class].Add(hash);
            Updata();
        }

        public void UpdataKey(KeyInfo _info, KeyFile _newKey)
        {
            string newHash = KeySave(_newKey);
            if (_info.Hash == newHash) return;
            string oldHash = _info.Hash;
            foreach (var kv in Groups.ToList())
            {
                var list = kv.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == _info.Hash)
                    {
                        // 如果已经存在 newHash，则删除旧条目；否则替换为 newHash
                        if (!list.Contains(newHash))
                            list[i] = newHash;
                        else
                        {
                            list.RemoveAt(i);
                            i--; // 调整索引以继续正确遍历
                        }
                    }
                }
            }
            string oldDir = Path.Combine(RootPath, _info.Hash.Substring(0, 2));
            string oldFilePath = Path.Combine(oldDir, _info.Hash.Substring(2) + ".skkey");
            if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
            if (Directory.Exists(oldDir) && Directory.GetFiles(oldDir).Length == 0 && Directory.GetDirectories(oldDir).Length == 0)
            {
                Directory.Delete(oldDir);
            }
            Updata();
            _info.Hash = newHash;
            _keyCache.Remove(oldHash);
            _keyCache[newHash] = _newKey;
        }
        #endregion

        #region 关于group的api
        public List<KeyItemViewModel> GetGroupInfo(string _group)
        {
            List<KeyItemViewModel> infoVms = new List<KeyItemViewModel>();
            foreach (string keyHash in Groups[_group])
            {
                var keyFile = LoadKeyFile(keyHash);
                infoVms.Add(new KeyItemViewModel(keyFile, keyHash, _group));
            }
            return infoVms;
        }

        public void MoveGroup(KeyInfo _info, string _aimGroup)
        {
            if (_info.Group == ALLGorup) return;
            if (!Groups.ContainsKey(_aimGroup)) CreateGroup(_aimGroup);
            Groups[_info.Group].Remove(_info.Hash);
            Groups[_aimGroup].Add(_info.Hash);
            _info.Group = _aimGroup;
            Updata();
        }

        public void AddGroup(KeyInfo _info, string _aimGroup)
        {
            if (!Groups.ContainsKey(_aimGroup)) CreateGroup(_aimGroup);
            if (!Groups[_aimGroup].Contains(_info.Hash))
            {
                Groups[_aimGroup].Add(_info.Hash);
                Updata();
            }
        }

        public void RemoveGroup(KeyInfo _info)
        {
            if (!Groups.ContainsKey(_info.Group)) return;
            if (Groups[_info.Group].Contains(_info.Hash))
            {
                Groups[_info.Group].Remove(_info.Hash);
                _info.Group = ALLGorup;
                Updata();
            }
        }

        public bool RenameGroup(string _oldName, string _newName)
        {
            if (string.IsNullOrWhiteSpace(_newName)) return false;
            if (_oldName == _newName) return false;
            if (!Groups.ContainsKey(_oldName)) return false;
            if (Groups.ContainsKey(_newName)) return false;
            Groups[_newName] = Groups[_oldName];
            Groups.Remove(_oldName);
            int idx = groupIndex.IndexOf(_oldName);
            groupIndex[idx] = _newName;
            Updata();
            return true;
        }

        public void DeleteGroup(string _groupName)
        {
            if (_groupName == ALLGorup || _groupName == "未分类") return;
            if (!Groups.ContainsKey(_groupName)) return;
            var hashes = Groups[_groupName].ToList();
            Groups.Remove(_groupName);
            groupIndex.Remove(_groupName);
            foreach (var hash in hashes)
            {
                bool inOtherGroup = false;
                foreach (var kv in Groups)
                {
                    if (kv.Key != ALLGorup && kv.Value.Contains(hash))
                    {
                        inOtherGroup = true;
                        break;
                    }
                }
                if (!inOtherGroup)
                    Groups["未分类"].Add(hash);
            }
            Updata();
        }
        #endregion
        #endregion

        #region utils
        public void CreateGroup(string _groupName)
        {
            if (Groups.ContainsKey(_groupName)) return;
            Groups.Add(_groupName, new List<string>());
            groupIndex.Add(_groupName);
        }

        public void AddNewGroup(string _groupName)
        {
            if (Groups.ContainsKey(_groupName)) return;
            CreateGroup(_groupName);
            Updata();
        }

        public void ClearCache() => _keyCache.Clear();

        private string KeySave(KeyFile _key)
        {
            string hash = SkHash.GetFileHash(_key.GetStringKey());
            string fileDirPath = Path.Combine(RootPath, hash.Substring(0, 2));
            if (!Directory.Exists(fileDirPath)) Directory.CreateDirectory(fileDirPath);

            string filePath = Path.Combine(fileDirPath, hash.Substring(2) + ".skkey");

            File.WriteAllText(filePath, _key.Save(UserKey), Encoding.UTF8);
            return hash;
        }

        private void Updata()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Version);
            foreach (string group in groupIndex)
            {
                sb.AppendLine($"[{group}]");
                foreach (string hash in Groups[group])
                {
                    sb.AppendLine(hash);
                }
            }
            File.WriteAllText(IndexPath, sb.ToString(), Encoding.UTF8);
        }

        private KeyFile LoadKeyFile(string _hash)
        {
            if (_keyCache.TryGetValue(_hash, out var cached))
                return cached;

            string filePath = Path.Combine(RootPath, _hash[..2], _hash[2..] + ".skkey");
            string data = _keySpring.DecryptToString(File.ReadAllText(filePath, Encoding.UTF8), UserKey);
            var keyFile = KeyFile.LoadKey(data);
            _keyCache[_hash] = keyFile;
            return keyFile;
        }
        #endregion
    }
}
