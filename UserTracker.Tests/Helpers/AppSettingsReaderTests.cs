using System.Collections.Specialized;
using UserTrackerShared.Helpers;
using Xunit;

namespace UserTracker.Tests.Helpers;

public class AppSettingsReaderTests
{
    [Fact]
    public void Constructor_ThrowsForNullSettings()
    {
        Assert.Throws<System.ArgumentNullException>(() => new AppSettingsReader(null!));
    }

    [Fact]
    public void GetString_UsesDefaultWhenMissing()
    {
        var reader = new AppSettingsReader(new NameValueCollection());
        Assert.Equal("fallback", reader.GetString("missing", "fallback"));
    }

    [Fact]
    public void GetString_ReturnsValue()
    {
        var reader = new AppSettingsReader(new NameValueCollection { ["key"] = "value" });
        Assert.Equal("value", reader.GetString("key"));
    }

    [Fact]
    public void GetRequiredString_ReturnsValue_WhenPresent()
    {
        var reader = new AppSettingsReader(new NameValueCollection { ["required"] = "value" });
        Assert.Equal("value", reader.GetRequiredString("required"));
    }

    [Fact]
    public void GetRequiredString_Throws_WhenMissing()
    {
        var reader = new AppSettingsReader(new NameValueCollection());
        Assert.Throws<System.ArgumentException>(() => reader.GetRequiredString("key"));
    }

    [Fact]
    public void GetRequiredInt_ReturnsValue_WhenValid()
    {
        var reader = new AppSettingsReader(new NameValueCollection { ["number"] = "42" });
        Assert.Equal(42, reader.GetRequiredInt("number"));
    }

    [Fact]
    public void GetRequiredInt_Throws_WhenInvalid()
    {
        var reader = new AppSettingsReader(new NameValueCollection { ["number"] = "notanint" });
        Assert.Throws<System.ArgumentException>(() => reader.GetRequiredInt("number"));
    }

    [Fact]
    public void GetRequiredBool_ReturnsTrue_WhenOne()
    {
        var reader = new AppSettingsReader(new NameValueCollection { ["flag"] = "1" });
        Assert.True(reader.GetRequiredBool("flag"));
    }

    [Fact]
    public void GetRequiredBool_ReturnsFalse_WhenZero()
    {
        var reader = new AppSettingsReader(new NameValueCollection { ["flag"] = "0" });
        Assert.False(reader.GetRequiredBool("flag"));
    }

    [Fact]
    public void GetRequiredBool_ReturnsBool_WhenText()
    {
        var reader = new AppSettingsReader(new NameValueCollection { ["flag"] = "true" });
        Assert.True(reader.GetRequiredBool("flag"));
    }

    [Fact]
    public void GetRequiredBool_Throws_WhenInvalid()
    {
        var reader = new AppSettingsReader(new NameValueCollection { ["flag"] = "maybe" });
        Assert.Throws<System.ArgumentException>(() => reader.GetRequiredBool("flag"));
    }
}
