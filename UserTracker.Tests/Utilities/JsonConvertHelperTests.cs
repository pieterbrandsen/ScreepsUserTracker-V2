using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.Utilities;
using Xunit;

namespace UserTracker.Tests.Utilities;

public class JsonConvertHelperTests
{
    [Fact]
    public async Task ReadAndConvertStream_ReturnsDeserializedObject()
    {
        using var content = new StringContent("{\"value\": 15}", Encoding.UTF8, "application/json");

        var result = await JsonConvertHelper.ReadAndConvertStream<Dictionary<string, long>>(content);

        Assert.NotNull(result);
        Assert.Equal(15L, result["value"]);
    }
}
