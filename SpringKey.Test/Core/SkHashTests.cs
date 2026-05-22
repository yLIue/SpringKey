using SpringKey.Core;

namespace SpringKey.Test.Core;

public class SkHashTests
{
    [Fact]
    public void GetFileHash_Returns40CharHexString()
    {
        var hash = SkHash.GetFileHash("test");
        Assert.Equal(40, hash.Length);
        Assert.All(hash, c => Assert.True(char.IsDigit(c) || (c is >= 'a' and <= 'f')));
    }

    [Fact]
    public void GetFileHash_SameInput_ReturnsSameHash()
    {
        var hash1 = SkHash.GetFileHash("consistent");
        var hash2 = SkHash.GetFileHash("consistent");
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetFileHash_DifferentInput_ReturnsDifferentHash()
    {
        var hash1 = SkHash.GetFileHash("alpha");
        var hash2 = SkHash.GetFileHash("beta");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GetFileHash_CaseSensitive()
    {
        var hash1 = SkHash.GetFileHash("Key");
        var hash2 = SkHash.GetFileHash("key");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GetFileHash_EmptyString_ReturnsValidHash()
    {
        var hash = SkHash.GetFileHash("");
        Assert.Equal(40, hash.Length);
    }

    [Fact]
    public void GetFileHash_UnicodeCharacters()
    {
        var hash = SkHash.GetFileHash("中文测试");
        Assert.Equal(40, hash.Length);
        Assert.All(hash, c => Assert.True(char.IsDigit(c) || (c is >= 'a' and <= 'f')));
    }
}
