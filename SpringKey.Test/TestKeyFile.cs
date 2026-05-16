using SpringKey.Core;
using SpringKey.Files;

namespace SpringKey.Test;

public class TestKeyFile
{
    private static KeySpring CreateKeySpring() => new();

    [Fact]
    public void Serialize_StartsWithVersion()
    {
        var key = new KeyFile("T", "A", "P");
        var output = key.Serialize();
        Assert.StartsWith(KeyFile.KeyVersion, output);
    }

    [Fact]
    public void Serialize_ContainsRequiredSections()
    {
        var key = new KeyFile("T", "A", "P");
        var output = key.Serialize();
        Assert.Contains("[title]", output);
        Assert.Contains("[account]", output);
        Assert.Contains("[password]", output);
    }

    [Fact]
    public void Serialize_SkipsEmptyOptionals()
    {
        var key = new KeyFile("T", "A", "P");
        var output = key.Serialize();
        Assert.DoesNotContain("[place]", output);
        Assert.DoesNotContain("[passwordPrev]", output);
        Assert.DoesNotContain("[description]", output);
    }

    [Fact]
    public void Serialize_WithPlace()
    {
        var key = new KeyFile("T", "A", "P") { Place = "https://example.com" };
        var output = key.Serialize();
        Assert.Contains("[place]", output);
        Assert.Contains("\thttps://example.com", output);
    }

    [Fact]
    public void Serialize_WithBinding()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("phone", "13800138000");
        var output = key.Serialize();
        Assert.Contains("[binding][phone]", output);
        Assert.Contains("\t13800138000", output);
    }

    [Fact]
    public void Serialize_MultilineDescription()
    {
        var key = new KeyFile("T", "A", "P") { Description = "第一行\n第二行" };
        var output = key.Serialize();
        Assert.Contains("[description]", output);
        Assert.Contains("\t第一行", output);
        Assert.Contains("\t第二行", output);
    }

    // ===== Encrypt/Decrypt 往返 =====

    [Fact]
    public void EncryptDecrypt_BasicFields()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("测试标题", "test@email.com", "mypassword");
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "myKey"), "myKey");
        Assert.Equal("测试标题", reloaded.Title);
        Assert.Equal("test@email.com", reloaded.Account);
        Assert.Equal("mypassword", reloaded.Password);
    }

    [Fact]
    public void EncryptDecrypt_WithPlace()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P") { Place = "https://example.com" };
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal("https://example.com", reloaded.Place);
    }

    [Fact]
    public void EncryptDecrypt_WithPasswordPrev()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P") { PasswordPrev = "old_password" };
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal("old_password", reloaded.PasswordPrev);
    }

    [Fact]
    public void EncryptDecrypt_MultilineDescription()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P") { Description = "第一行\n第二行\n第三行" };
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal("第一行\n第二行\n第三行", reloaded.Description);
    }

    [Fact]
    public void EncryptDecrypt_EmptyDescription()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P") { Description = "" };
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal("", reloaded.Description);
    }

    [Fact]
    public void EncryptDecrypt_SingleBinding()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("phone", "13800138000");
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Single(reloaded.Binding);
        Assert.Equal("13800138000", reloaded.Binding["phone"]);
    }

    [Fact]
    public void EncryptDecrypt_MultipleBindings()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("phone", "13800138000");
        key.AddBinding("email", "test@test.com");
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal(2, reloaded.Binding.Count);
        Assert.Equal("13800138000", reloaded.Binding["phone"]);
        Assert.Equal("test@test.com", reloaded.Binding["email"]);
    }

    [Fact]
    public void EncryptDecrypt_AllFields()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("完整标题", "full@test.com", "p@ssw0rd")
        {
            Place = "https://example.com",
            Description = "这是一段\n多行描述",
            PasswordPrev = "previous_password"
        };
        key.AddBinding("phone", "13900001111");
        key.AddBinding("email", "bind@test.com");

        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal("完整标题", reloaded.Title);
        Assert.Equal("full@test.com", reloaded.Account);
        Assert.Equal("p@ssw0rd", reloaded.Password);
        Assert.Equal("https://example.com", reloaded.Place);
        Assert.Equal("这是一段\n多行描述", reloaded.Description);
        Assert.Equal("previous_password", reloaded.PasswordPrev);
        Assert.Equal(2, reloaded.Binding.Count);
        Assert.Equal("13900001111", reloaded.Binding["phone"]);
        Assert.Equal("bind@test.com", reloaded.Binding["email"]);
    }

    [Fact]
    public void Decrypt_WrongKey_Throws()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        var encrypted = key.Encrypt(ks, "correctKey");
        Assert.ThrowsAny<Exception>(() => KeyFile.Decrypt(ks, encrypted, "wrongKey"));
    }

    [Fact]
    public void Decrypt_OldVersion_ReturnsEmptyKey()
    {
        var ks = CreateKeySpring();
        var oldFormat = "skkey_ver0.1\n[title]\n\tT\n[account]\n\tA\n[password]\n\tP\n";
        var encrypted = ks.EncryptString(oldFormat, "key");
        var result = KeyFile.Decrypt(ks, encrypted, "key");
        Assert.Equal("", result.Title);
        Assert.False(result.IsValid());
    }

    // ===== IsValid =====

    [Fact]
    public void IsValid_AllFields_ReturnsTrue()
    {
        Assert.True(new KeyFile("T", "A", "P").IsValid());
    }

    [Fact]
    public void IsValid_EmptyTitle_ReturnsFalse()
    {
        Assert.False(new KeyFile("", "A", "P").IsValid());
    }

    [Fact]
    public void IsValid_EmptyAccount_ReturnsFalse()
    {
        Assert.False(new KeyFile("T", "", "P").IsValid());
    }

    [Fact]
    public void IsValid_EmptyPassword_ReturnsFalse()
    {
        Assert.False(new KeyFile("T", "A", "").IsValid());
    }

    [Fact]
    public void IsValid_WhitespaceTitle_ReturnsFalse()
    {
        Assert.False(new KeyFile("  ", "A", "P").IsValid());
    }

    [Fact]
    public void IsValid_WhitespacePassword_ReturnsFalse()
    {
        Assert.False(new KeyFile("T", "A", "  ").IsValid());
    }

    // ===== Binding =====

    [Fact]
    public void AddBinding_Valid()
    {
        var key = new KeyFile("T", "A", "P");
        Assert.True(key.AddBinding("qq", "123456"));
        Assert.Equal("123456", key.Binding["qq"]);
    }

    [Fact]
    public void AddBinding_EmptyType_ReturnsFalse()
    {
        var key = new KeyFile("T", "A", "P");
        Assert.False(key.AddBinding("", "val"));
        Assert.Empty(key.Binding);
    }

    [Fact]
    public void AddBinding_EmptyValue_ReturnsFalse()
    {
        var key = new KeyFile("T", "A", "P");
        Assert.False(key.AddBinding("key", ""));
        Assert.Empty(key.Binding);
    }

    [Fact]
    public void AddBinding_WhitespaceType_ReturnsFalse()
    {
        var key = new KeyFile("T", "A", "P");
        Assert.False(key.AddBinding("  ", "val"));
        Assert.Empty(key.Binding);
    }

    [Fact]
    public void AddBinding_WhitespaceValue_ReturnsFalse()
    {
        var key = new KeyFile("T", "A", "P");
        Assert.False(key.AddBinding("key", "  "));
        Assert.Empty(key.Binding);
    }

    [Fact]
    public void AddBinding_OverwriteExisting()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("phone", "111");
        key.AddBinding("phone", "222");
        Assert.Single(key.Binding);
        Assert.Equal("222", key.Binding["phone"]);
    }

    [Fact]
    public void AddBinding_TrimsWhitespace()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding(" phone ", " 138 ");
        Assert.Equal("138", key.Binding["phone"]);
    }

    [Fact]
    public void RemoveBinding_Exists()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("qq", "123");
        Assert.True(key.RemoveBinding("qq"));
        Assert.False(key.Binding.ContainsKey("qq"));
    }

    [Fact]
    public void RemoveBinding_NotExists_ReturnsFalse()
    {
        var key = new KeyFile("T", "A", "P");
        Assert.False(key.RemoveBinding("nope"));
    }
    
    [Fact]
    public void EncryptDecrypt_EmptyBinding()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Empty(reloaded.Binding);
    }

    [Fact]
    public void EncryptDecrypt_ChineseCharacters()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("中文标题测试", "账号@测试.com", "密码123")
        {
            Place = "https://测试.中国",
            Description = "这是一段中文描述\n第二行中文"
        };
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal("中文标题测试", reloaded.Title);
        Assert.Equal("账号@测试.com", reloaded.Account);
        Assert.Equal("密码123", reloaded.Password);
        Assert.Equal("https://测试.中国", reloaded.Place);
        Assert.Equal("这是一段中文描述\n第二行中文", reloaded.Description);
    }

    [Fact]
    public void EncryptDecrypt_SpecialCharacters()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T<>&\"'", "A", "P@ss!@#$%^&*()");
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal("T<>&\"'", reloaded.Title);
        Assert.Equal("P@ss!@#$%^&*()", reloaded.Password);
    }

    [Fact]
    public void Binding_IsReadOnly()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("k", "v");
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(key.Binding);
    }
}
