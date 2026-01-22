using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.Utilities;
using Xunit;

namespace UserTracker.Tests.Utilities;

public class ScreepsAPITests
{
    [Fact]
    public async Task CloneHttpRequestMessageAsync_CopiesHeadersAndContent()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api")
        {
            Content = new StringContent("{\"payload\": true}", Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Test-Header", "value");

        var method = typeof(ScreepsAPI).GetMethod("CloneHttpRequestMessageAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var task = (Task<HttpRequestMessage>)method!.Invoke(null, new object?[] { request })!;
        var clone = await task;

        Assert.NotSame(request, clone);
        Assert.Equal(request.Method, clone.Method);
        Assert.Equal(request.RequestUri, clone.RequestUri);
        Assert.True(clone.Headers.Contains("X-Test-Header"));
        Assert.NotNull(clone.Content);
        var originalBody = await request.Content!.ReadAsStringAsync();
        var cloneBody = await clone.Content!.ReadAsStringAsync();
        Assert.Equal(originalBody, cloneBody);
    }
}
