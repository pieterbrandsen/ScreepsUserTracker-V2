using UserTrackerShared.Helpers.Tests;
using Xunit;

namespace UserTracker.Tests.Helpers;

public class HistoryDictionaryHelperTests
{
    [Fact]
    public void MapPropertyIfFound_RewritesKnownSegments()
    {
        var input = "_id._updated.effect";
        var result = HistoryDictionaryHelper.MapPropertyIfFound(input);
        Assert.Equal("Id.Updated.EffectType", result);
    }

    [Fact]
    public void MapPropertyIfFound_LeavesUnknownSegments()
    {
        var input = "foo.bar";
        var result = HistoryDictionaryHelper.MapPropertyIfFound(input);
        Assert.Equal("foo.bar", result);
    }

    [Fact]
    public void CapitalizeLetters_CapitalizesEverySegment()
    {
        var input = "alpha.beta.gamma";
        var result = HistoryDictionaryHelper.CapitalizeLetters(input);
        Assert.Equal("Alpha.Beta.Gamma", result);
    }

    [Fact]
    public void CapitalizeLettersExceptLast_LeavesLastSegment()
    {
        var input = "alpha.beta.gamma";
        var result = HistoryDictionaryHelper.CapitalizeLettersExceptLast(input);
        Assert.Equal("Alpha.Beta.gamma", result);
    }

    [Fact]
    public void CapitalizeLetters_ReturnsEmptyWhenInputEmpty()
    {
        Assert.Equal(string.Empty, HistoryDictionaryHelper.CapitalizeLetters(string.Empty));
    }
}
