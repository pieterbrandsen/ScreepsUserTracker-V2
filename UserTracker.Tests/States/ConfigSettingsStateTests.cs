using System.Configuration;
using UserTrackerShared.States;

namespace UserTracker.Tests.States;

public class ConfigSettingsStateTests
{
    [Theory]
    [InlineData(100, 10)]
    [InlineData(100, 100)]
    [InlineData(100, 1000)]
    [InlineData(20, 5)]
    [InlineData(1, 1)]
    public void InitTest_AcceptsAlignedWindows(int file, int window)
    {
        WithSettings(file, window, settings => ConfigSettingsState.InitTest(settings));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(100, 0)]
    [InlineData(-100, 10)]
    [InlineData(100, -10)]
    [InlineData(100, 30)]
    [InlineData(100, 150)]
    public void InitTest_RejectsNonPositiveOrMisalignedWindows(int file, int window)
    {
        WithSettings(file, window, settings =>
        {
            var error = Assert.Throws<ArgumentException>(() => ConfigSettingsState.InitTest(settings));
            Assert.Contains("TICKS_IN_FILE", error.Message);
            Assert.Contains("TICKS_IN_OBJECT", error.Message);
        });
    }

    private static void WithSettings(int file, int window, Action<AppSettingsSection> test)
    {
        var previousFile = ConfigSettingsState.TicksInFile;
        var previousWindow = ConfigSettingsState.TicksInObject;
        try
        {
            var settings = new AppSettingsSection();
            settings.Settings.Add("TICKS_IN_FILE", file.ToString());
            settings.Settings.Add("TICKS_IN_OBJECT", window.ToString());
            test(settings);
        }
        finally
        {
            ConfigSettingsState.TicksInFile = previousFile;
            ConfigSettingsState.TicksInObject = previousWindow;
        }
    }
}
