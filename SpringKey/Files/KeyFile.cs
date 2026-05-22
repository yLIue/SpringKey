using SpringKey.Core;
using System.Text;

namespace SpringKey.Files
{
    public class KeyFile
    {
        public const string KeyVersion = "skkey-ver1.0.0";
        public string Title { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        /// <summary>描述</summary>
        public string Description { get; set; } = "";
        /// <summary>用途(一般是网站或app名字)</summary>
        public string Place { get; set; } = "";
        private readonly List<string> _passwordPrev = new();
        /// <summary>曾用密码，最新在前</summary>
        public IReadOnlyList<string> PasswordPrev => _passwordPrev;
        
        private readonly Dictionary<string, string> _binding = new();
        /// <summary>绑定信息</summary>
        public IReadOnlyDictionary<string, string> Binding => _binding;

        /// <summary>创建密码项</summary>
        /// <param name="title">标题</param>
        /// <param name="account">账号</param>
        /// <param name="password">密码</param>
        public KeyFile(string title, string account, string password)
        {
            Title = title;
            Account = account;
            Password = password;
        }

        private KeyFile()
        {
            Title = "";
            Account = "";
            Password = "";
        }

        /// <summary>判断是否为有效密码项</summary>
        /// <returns>标题、账号、密码均非空时返回 true</returns>
        public bool IsValid() =>
            !string.IsNullOrWhiteSpace(Title)
            && !string.IsNullOrWhiteSpace(Account)
            && !string.IsNullOrWhiteSpace(Password);

        /// <summary>添加绑定信息</summary>
        /// <param name="type">绑定类型（如 email、phone）</param>
        /// <param name="value">绑定值</param>
        /// <returns>添加成功返回 true；type 或 value 为空则返回 false</returns>
        public bool AddBinding(string type, string value)
        {
            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value)) return false;
            _binding[type.Trim()] = value.Trim();
            return true;
        }

        /// <summary>移除绑定信息</summary>
        /// <param name="type">绑定类型</param>
        /// <returns>存在并移除成功返回 true</returns>
        public bool RemoveBinding(string type) => _binding.Remove(type);

        /// <summary>批量添加曾用密码</summary>
        public void AddPasswordPrevEntries(IEnumerable<string> entries)
        {
            foreach (var e in entries)
                if (!string.IsNullOrEmpty(e))
                    _passwordPrev.Add(e);
        }

        /// <summary>记录旧密码到曾用密码列表（去重后推到最前）</summary>
        public void RecordPassword(string oldPassword)
        {
            if (string.IsNullOrEmpty(oldPassword)) return;
            _passwordPrev.Remove(oldPassword);
            _passwordPrev.Insert(0, oldPassword);
        }

        /// <summary>删除指定曾用密码</summary>
        public bool RemovePasswordPrev(string password) => _passwordPrev.Remove(password);

        /// <summary>获取明文</summary>
        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.AppendLine(KeyVersion);
            WriteSection(sb, "title", Title);
            WriteSection(sb, "account", Account);
            WriteSection(sb, "password", Password);
            WriteSection(sb, "place", Place);
            WriteSection(sb, "description", Description);
            if (_passwordPrev.Count > 0)
            {
                sb.AppendLine("[passwordPrev]");
                foreach (var p in _passwordPrev)
                    sb.AppendLine($"\t{p}");
            }
            foreach (var kv in _binding)
                WriteSection(sb, $"binding][{kv.Key}", kv.Value);
            return sb.ToString();
        }

        /// <summary>获取密文</summary>
        /// <param name="keySpring">密钥加密器</param>
        /// <param name="userKey">用户密钥</param>
        public string Encrypt(KeySpring keySpring, string userKey)
        {
            return keySpring.EncryptString(Serialize(), userKey);
        }

        private static void WriteSection(StringBuilder sb, string name, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            sb.AppendLine($"[{name}]");
            foreach (var line in value.Split('\n'))
                sb.AppendLine($"\t{line}");
        }
        
        /// <summary>解密</summary>
        /// <param name="keySpring">密钥加密器</param>
        /// <param name="encryptedData">密文</param>
        /// <param name="userKey">用户密钥</param>
        /// <returns>返回一个可能为空的 KeyFile</returns>
        public static KeyFile Decrypt(KeySpring keySpring, string encryptedData, string userKey)
        {
            var plain = keySpring.DecryptToString(encryptedData, userKey);
            return Deserialize(plain);
        }

        /// <summary>构建key</summary>
        /// <param name="data">明文数据</param>
        /// <returns>版本不匹配时返回空 KeyFile</returns>
        private static KeyFile Deserialize(string data)
        {
            var lines = data.Split('\n');
            if (lines.Length == 0) return new KeyFile();

            var version = lines[0].TrimEnd('\r');
            if (version != KeyVersion) return new KeyFile();

            var key = new KeyFile();
            var descSb = new StringBuilder();
            string section = "";
            string bindingType = "";

            foreach (var rawLine in lines.Skip(1))
            {
                var line = rawLine.TrimEnd('\r');
                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    var name = line[1..^1];
                    if (name.StartsWith("binding]["))
                    {
                        section = "binding";
                        bindingType = name["binding][".Length..];
                    }
                    else
                    {
                        section = name;
                        bindingType = "";
                    }
                }
                else if (line.StartsWith('\t'))
                {
                    ApplyValue(key, descSb, section, bindingType, line[1..]);
                }
            }

            key.Description = descSb.ToString().TrimEnd('\n');
            return key;
        }

        private static void ApplyValue(KeyFile key, StringBuilder descSb, string section, string bindingType,
            string value)
        {
            switch (section)
            {
                case "title": key.Title = value; break;
                case "account": key.Account = value; break;
                case "password": key.Password = value; break;
                case "place": key.Place = value; break;
                case "description": descSb.Append(value).Append('\n'); break;
                case "passwordPrev": key._passwordPrev.Add(value); break;
                case "binding": key.AddBinding(bindingType, value); break;
            }
        }
    }
}
