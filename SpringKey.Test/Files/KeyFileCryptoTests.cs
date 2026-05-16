using SpringKey.Core;
using SpringKey.Files;

namespace SpringKey.Test.Files;

public class KeyFileCryptoTests
{
    private static KeySpring CreateKeySpring() => new();

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
    public void EncryptDecrypt_WithDescription()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P") { Description = "单行描述" };
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal("单行描述", reloaded.Description);
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
    public void EncryptDecrypt_WithPasswordPrev()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        key.RecordPassword("old_password");
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Single(reloaded.PasswordPrev);
        Assert.Equal("old_password", reloaded.PasswordPrev[0]);
    }

    [Fact]
    public void EncryptDecrypt_WithMultiplePasswordPrev()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        key.AddPasswordPrevEntries(new[] { "p1", "p2", "p3" });
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal(3, reloaded.PasswordPrev.Count);
        Assert.Equal("p1", reloaded.PasswordPrev[0]);
        Assert.Equal("p2", reloaded.PasswordPrev[1]);
        Assert.Equal("p3", reloaded.PasswordPrev[2]);
    }

    [Fact]
    public void EncryptDecrypt_EmptyPasswordPrev()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Empty(reloaded.PasswordPrev);
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
    public void EncryptDecrypt_EmptyBinding()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Empty(reloaded.Binding);
    }

    [Fact]
    public void EncryptDecrypt_AllFields()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("完整标题", "full@test.com", "p@ssw0rd")
        {
            Place = "https://example.com",
            Description = "这是描述\n多行内容"
        };
        key.AddPasswordPrevEntries(new[] { "old1", "old2" });
        key.AddBinding("phone", "13900001111");
        key.AddBinding("email", "bind@test.com");

        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal("完整标题", reloaded.Title);
        Assert.Equal("full@test.com", reloaded.Account);
        Assert.Equal("p@ssw0rd", reloaded.Password);
        Assert.Equal("https://example.com", reloaded.Place);
        Assert.Equal("这是描述\n多行内容", reloaded.Description);
        Assert.Equal(2, reloaded.PasswordPrev.Count);
        Assert.Equal(2, reloaded.Binding.Count);
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
    public void Decrypt_WrongKey_Throws()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        var encrypted = key.Encrypt(ks, "correctKey");
        Assert.ThrowsAny<Exception>(() => KeyFile.Decrypt(ks, encrypted, "wrongKey"));
    }

    [Fact]
    public void Decrypt_OldVersionFormat_ReturnsInvalid()
    {
        var ks = CreateKeySpring();
        var oldFormat = "skkey_ver0.1\n[title]\n\tT\n[account]\n\tA\n[password]\n\tP\n";
        var encrypted = ks.EncryptString(oldFormat, "key");
        var result = KeyFile.Decrypt(ks, encrypted, "key");
        Assert.False(result.IsValid());
    }

    [Fact]
    public void Decrypt_WrongVersion_ReturnsInvalid()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        var tampered = key.Serialize().Replace(KeyFile.KeyVersion, "skkey-ver0.0.0");
        var encrypted = ks.EncryptString(tampered, "key");
        var result = KeyFile.Decrypt(ks, encrypted, "key");
        Assert.False(result.IsValid());
    }

    [Fact]
    public void Decrypt_EmptyPlaintext_ReturnsInvalid()
    {
        var ks = CreateKeySpring();
        var encrypted = ks.EncryptString("", "key");
        var result = KeyFile.Decrypt(ks, encrypted, "key");
        Assert.False(result.IsValid());
    }

    [Fact]
    public void EncryptDecrypt_PasswordPrevOrderPreserved()
    {
        var ks = CreateKeySpring();
        var key = new KeyFile("T", "A", "P");
        key.AddPasswordPrevEntries(new[] { "third", "second", "first" });
        var reloaded = KeyFile.Decrypt(ks, key.Encrypt(ks, "key"), "key");
        Assert.Equal(3, reloaded.PasswordPrev.Count);
        Assert.Equal("third", reloaded.PasswordPrev[0]);
        Assert.Equal("second", reloaded.PasswordPrev[1]);
        Assert.Equal("first", reloaded.PasswordPrev[2]);
    }
}
