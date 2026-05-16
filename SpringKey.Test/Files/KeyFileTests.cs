using SpringKey.Files;

namespace SpringKey.Test.Files;

public class KeyFileTests
{
    // ==================== Serialize ====================

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
        var key = new KeyFile("MyTitle", "user@email.com", "secret");
        var output = key.Serialize();
        Assert.Contains("[title]", output);
        Assert.Contains("\tMyTitle", output);
        Assert.Contains("[account]", output);
        Assert.Contains("\tuser@email.com", output);
        Assert.Contains("[password]", output);
        Assert.Contains("\tsecret", output);
    }

    [Fact]
    public void Serialize_SkipsEmptyOptionals()
    {
        var key = new KeyFile("T", "A", "P");
        var output = key.Serialize();
        Assert.DoesNotContain("[place]", output);
        Assert.DoesNotContain("[description]", output);
        Assert.DoesNotContain("[passwordPrev]", output);
        Assert.DoesNotContain("[binding]", output);
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
    public void Serialize_WithSingleLineDescription()
    {
        var key = new KeyFile("T", "A", "P") { Description = "some description" };
        var output = key.Serialize();
        Assert.Contains("[description]", output);
        Assert.Contains("\tsome description", output);
    }

    [Fact]
    public void Serialize_WithMultilineDescription()
    {
        var key = new KeyFile("T", "A", "P") { Description = "line1\nline2\nline3" };
        var output = key.Serialize();
        Assert.Contains("[description]", output);
        Assert.Contains("\tline1", output);
        Assert.Contains("\tline2", output);
        Assert.Contains("\tline3", output);
    }

    [Fact]
    public void Serialize_WithPasswordPrev()
    {
        var key = new KeyFile("T", "A", "P");
        key.RecordPassword("old_pass");
        var output = key.Serialize();
        Assert.Contains("[passwordPrev]", output);
        Assert.Contains("\told_pass", output);
    }

    [Fact]
    public void Serialize_WithMultiplePasswordPrev_PreservesOrder()
    {
        var key = new KeyFile("T", "A", "P");
        key.RecordPassword("first");
        key.RecordPassword("second");
        key.RecordPassword("third");
        var output = key.Serialize();
        var prevIdx = output.IndexOf("[passwordPrev]");
        var thirdIdx = output.IndexOf("\tthird", prevIdx);
        var secondIdx = output.IndexOf("\tsecond", prevIdx);
        var firstIdx = output.IndexOf("\tfirst", prevIdx);
        Assert.True(thirdIdx < secondIdx);
        Assert.True(secondIdx < firstIdx);
    }

    [Fact]
    public void Serialize_WithSingleBinding()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("phone", "13800138000");
        var output = key.Serialize();
        Assert.Contains("[binding][phone]", output);
        Assert.Contains("\t13800138000", output);
    }

    [Fact]
    public void Serialize_WithMultipleBindings()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("phone", "111");
        key.AddBinding("email", "test@test.com");
        var output = key.Serialize();
        Assert.Contains("[binding][phone]", output);
        Assert.Contains("\t111", output);
        Assert.Contains("[binding][email]", output);
        Assert.Contains("\ttest@test.com", output);
    }

    [Fact]
    public void Serialize_AllFields()
    {
        var key = new KeyFile("FullTitle", "full@test.com", "p@ssw0rd")
        {
            Place = "https://example.com",
            Description = "desc line1\ndesc line2"
        };
        key.RecordPassword("prev1");
        key.AddBinding("qq", "123456");

        var output = key.Serialize();
        Assert.StartsWith(KeyFile.KeyVersion, output);
        Assert.Contains("[title]", output);
        Assert.Contains("[account]", output);
        Assert.Contains("[password]", output);
        Assert.Contains("[place]", output);
        Assert.Contains("[description]", output);
        Assert.Contains("[passwordPrev]", output);
        Assert.Contains("[binding][qq]", output);
    }

    // ==================== IsValid ====================

    [Fact]
    public void IsValid_AllFieldsPresent_ReturnsTrue()
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
    public void IsValid_WhitespaceAccount_ReturnsFalse()
    {
        Assert.False(new KeyFile("T", "   ", "P").IsValid());
    }

    [Fact]
    public void IsValid_WhitespacePassword_ReturnsFalse()
    {
        Assert.False(new KeyFile("T", "A", "\t").IsValid());
    }

    [Fact]
    public void IsValid_WithOptionals_ReturnsTrue()
    {
        var key = new KeyFile("T", "A", "P")
        {
            Place = "https://example.com",
            Description = "desc"
        };
        key.RecordPassword("old");
        key.AddBinding("k", "v");
        Assert.True(key.IsValid());
    }

    // ==================== Binding ====================

    [Fact]
    public void AddBinding_ReturnsTrue()
    {
        var key = new KeyFile("T", "A", "P");
        Assert.True(key.AddBinding("qq", "123456"));
    }

    [Fact]
    public void AddBinding_StoresValue()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("qq", "123456");
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
    public void AddBinding_TrimsWhitespace()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding(" phone ", " 138 ");
        Assert.Equal("138", key.Binding["phone"]);
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
    public void RemoveBinding_Exists_ReturnsTrue()
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
        Assert.False(key.RemoveBinding("nonexistent"));
    }

    [Fact]
    public void Binding_IsReadOnly()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddBinding("k", "v");
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(key.Binding);
    }

    // ==================== PasswordPrev ====================

    [Fact]
    public void PasswordPrev_StartsEmpty()
    {
        var key = new KeyFile("T", "A", "P");
        Assert.Empty(key.PasswordPrev);
    }

    [Fact]
    public void RecordPassword_AddsToFront()
    {
        var key = new KeyFile("T", "A", "P");
        key.RecordPassword("first");
        key.RecordPassword("second");
        Assert.Equal(2, key.PasswordPrev.Count);
        Assert.Equal("second", key.PasswordPrev[0]);
        Assert.Equal("first", key.PasswordPrev[1]);
    }

    [Fact]
    public void RecordPassword_Duplicate_MovesToFront()
    {
        var key = new KeyFile("T", "A", "P");
        key.RecordPassword("a");
        key.RecordPassword("b");
        key.RecordPassword("a");
        Assert.Equal(2, key.PasswordPrev.Count);
        Assert.Equal("a", key.PasswordPrev[0]);
        Assert.Equal("b", key.PasswordPrev[1]);
    }

    [Fact]
    public void RecordPassword_EmptyString_Ignored()
    {
        var key = new KeyFile("T", "A", "P");
        key.RecordPassword("");
        Assert.Empty(key.PasswordPrev);
    }

    [Fact]
    public void RecordPassword_Null_Ignored()
    {
        var key = new KeyFile("T", "A", "P");
        key.RecordPassword(null!);
        Assert.Empty(key.PasswordPrev);
    }

    [Fact]
    public void AddPasswordPrevEntries_AddsAll()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddPasswordPrevEntries(new[] { "a", "b", "c" });
        Assert.Equal(3, key.PasswordPrev.Count);
    }

    [Fact]
    public void AddPasswordPrevEntries_FiltersEmptyStrings()
    {
        var key = new KeyFile("T", "A", "P");
        key.AddPasswordPrevEntries(new[] { "a", "", "b", "" });
        Assert.Equal(2, key.PasswordPrev.Count);
        Assert.Equal("a", key.PasswordPrev[0]);
        Assert.Equal("b", key.PasswordPrev[1]);
    }

    [Fact]
    public void RemovePasswordPrev_Exists_ReturnsTrue()
    {
        var key = new KeyFile("T", "A", "P");
        key.RecordPassword("pwd");
        Assert.True(key.RemovePasswordPrev("pwd"));
        Assert.Empty(key.PasswordPrev);
    }

    [Fact]
    public void RemovePasswordPrev_NotExists_ReturnsFalse()
    {
        var key = new KeyFile("T", "A", "P");
        Assert.False(key.RemovePasswordPrev("nonexistent"));
    }

    [Fact]
    public void PasswordPrev_IsReadOnly()
    {
        var key = new KeyFile("T", "A", "P");
        key.RecordPassword("p");
        Assert.IsAssignableFrom<IReadOnlyList<string>>(key.PasswordPrev);
    }
}
