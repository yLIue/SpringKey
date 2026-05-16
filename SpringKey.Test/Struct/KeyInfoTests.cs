using SpringKey.Files;
using SpringKey.Struct;

namespace SpringKey.Test.Struct;

public class KeyInfoTests
{
    [Fact]
    public void Constructor_FromKeyFile_CopiesBasicFields()
    {
        var key = new KeyFile("标题", "account", "password");
        var info = new KeyInfo(key, "hash123", "默认分组");
        Assert.Equal("标题", info.Title);
        Assert.Equal("account", info.Account);
        Assert.Equal("password", info.Password);
        Assert.Equal("hash123", info.Hash);
        Assert.Equal("默认分组", info.Group);
    }

    [Fact]
    public void Constructor_FromKeyFile_CopiesOptionalFields()
    {
        var key = new KeyFile("T", "A", "P")
        {
            Place = "https://example.com",
            Description = "描述内容"
        };
        key.RecordPassword("old");
        key.AddBinding("phone", "123");

        var info = new KeyInfo(key, "h", "g");
        Assert.Equal("https://example.com", info.Place);
        Assert.Equal("描述内容", info.Description);
        Assert.Single(info.PasswordPrev);
        Assert.Equal("old", info.PasswordPrev[0]);
        Assert.Single(info.Binding);
        Assert.Equal("123", info.Binding["phone"]);
    }

    [Fact]
    public void Constructor_FromKeyFile_PasswordPrevIsCopy()
    {
        var key = new KeyFile("T", "A", "P");
        key.RecordPassword("p");
        var info = new KeyInfo(key, "h", "g");
        key.RecordPassword("q");
        Assert.Single(info.PasswordPrev);
    }

    [Fact]
    public void Constructor_FromKeyFile_BindingIsCopy()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("k", "v");
        var info = new KeyInfo(key, "h", "g");
        key.AddBinding("k2", "v2");
        Assert.Single(info.Binding);
    }

    [Fact]
    public void DefaultConstructor_SetsNullPlaceholder()
    {
        var info = new KeyInfo();
        // 无参构造用于占位，所有字符串字段设为 "Null"
        Assert.Equal("Null", info.Title);
        Assert.Equal("Null", info.Account);
        Assert.Equal("Null", info.Password);
        Assert.Equal("Null", info.Place);
        Assert.Equal("Null", info.Hash);
        Assert.Equal("Null", info.Group);
    }

    [Fact]
    public void DefaultConstructor_HasEmptyCollections()
    {
        var info = new KeyInfo();
        Assert.NotNull(info.PasswordPrev);
        Assert.Empty(info.PasswordPrev);
        Assert.NotNull(info.Binding);
        Assert.Empty(info.Binding);
    }
}
