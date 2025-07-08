using Xunit;

namespace PowerBGInfo.Tests;

public class SystemInfoProviderTests
{
    [Fact]
    public void ReturnsUserName()
    {
        var value = SystemInfoProvider.GetValue("UserName");
        Assert.Equal(Environment.UserName, value);
    }
}
