using SpringKey.Core;

namespace SpringKey.Test.Core;

public class KeySpringTests
{
    private static KeySpring CreateKeySpring() => new();

    [Fact]
    public void EncryptDecrypt_SimpleString()
    {
        var ks = CreateKeySpring();
        var encrypted = ks.EncryptString("hello world", "key");
        var decrypted = ks.DecryptToString(encrypted, "key");
        Assert.Equal("hello world", decrypted);
    }

    [Fact]
    public void EncryptDecrypt_EmptyString()
    {
        var ks = CreateKeySpring();
        var encrypted = ks.EncryptString("", "key");
        var decrypted = ks.DecryptToString(encrypted, "key");
        Assert.Equal("", decrypted);
    }

    [Fact]
    public void EncryptDecrypt_ChineseCharacters()
    {
        var ks = CreateKeySpring();
        var input = "你好世界测试中文";
        var encrypted = ks.EncryptString(input, "key");
        var decrypted = ks.DecryptToString(encrypted, "key");
        Assert.Equal(input, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_SpecialCharacters()
    {
        var ks = CreateKeySpring();
        var input = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~\n\t\r";
        var encrypted = ks.EncryptString(input, "key");
        var decrypted = ks.DecryptToString(encrypted, "key");
        Assert.Equal(input, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_LongString()
    {
        var ks = CreateKeySpring();
        var input = new string('A', 10000);
        var encrypted = ks.EncryptString(input, "key");
        var decrypted = ks.DecryptToString(encrypted, "key");
        Assert.Equal(input, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_MultilineString()
    {
        var ks = CreateKeySpring();
        var input = "line1\nline2\nline3\n\nline5";
        var encrypted = ks.EncryptString(input, "key");
        var decrypted = ks.DecryptToString(encrypted, "key");
        Assert.Equal(input, decrypted);
    }

    [Fact]
    public void EncryptString_ReturnsValidBase64()
    {
        var ks = CreateKeySpring();
        var encrypted = ks.EncryptString("test", "key");
        var bytes = Convert.FromBase64String(encrypted);
        Assert.True(bytes.Length > 29); // header(29) + cipher + tag(16)
    }

    [Fact]
    public void EncryptString_DifferentCalls_ProduceDifferentOutput()
    {
        var ks = CreateKeySpring();
        var enc1 = ks.EncryptString("test", "key");
        var enc2 = ks.EncryptString("test", "key");
        Assert.NotEqual(enc1, enc2); // different nonce/salt each time
    }

    [Fact]
    public void Decrypt_WrongKey_Throws()
    {
        var ks = CreateKeySpring();
        var encrypted = ks.EncryptString("test", "correctKey");
        Assert.ThrowsAny<Exception>(() => ks.DecryptToString(encrypted, "wrongKey"));
    }

    [Fact]
    public void Decrypt_TamperedCiphertext_Throws()
    {
        var ks = CreateKeySpring();
        var encrypted = ks.EncryptString("test", "key");
        var bytes = Convert.FromBase64String(encrypted);
        bytes[30] ^= 0xFF;
        var tampered = Convert.ToBase64String(bytes);
        Assert.ThrowsAny<Exception>(() => ks.DecryptToString(tampered, "key"));
    }

    [Fact]
    public void Decrypt_InvalidBase64_Throws()
    {
        var ks = CreateKeySpring();
        Assert.ThrowsAny<Exception>(() => ks.DecryptToString("not-valid-base64!!!", "key"));
    }

    [Fact]
    public void Decrypt_TooShortBlob_Throws()
    {
        var ks = CreateKeySpring();
        var shortBase64 = Convert.ToBase64String(new byte[5]);
        Assert.ThrowsAny<Exception>(() => ks.DecryptToString(shortBase64, "key"));
    }

    [Fact]
    public void EncryptString_NullKey_Throws()
    {
        var ks = CreateKeySpring();
        Assert.Throws<ArgumentNullException>(() => ks.EncryptString("test", null!));
    }

    [Fact]
    public void Decrypt_DifferentKeys_ProduceDifferentPlaintext()
    {
        var ks = CreateKeySpring();
        var encrypted = ks.EncryptString("test", "keyA");
        Assert.ThrowsAny<Exception>(() => ks.DecryptToString(encrypted, "keyB"));
    }
}
