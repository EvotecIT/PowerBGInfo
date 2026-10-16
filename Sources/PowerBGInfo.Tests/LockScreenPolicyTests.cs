namespace PowerBGInfo.Tests;

public class LockScreenPolicyTests
{
    [Theory]
    [InlineData("Enterprise", true)]
    [InlineData("EnterpriseS", true)]
    [InlineData("Education", true)]
    [InlineData("EducationN", true)]
    [InlineData("ServerStandard", true)]
    [InlineData("ServerRdsh", true)]
    [InlineData("IoTEnterprise", true)]
    [InlineData("Professional", false)]
    [InlineData("ProfessionalEducation", false)]
    [InlineData("Core", false)]
    [InlineData(null, false)]
    public void KnowsWhichEditionsApplyTheLockScreenPolicy(string? edition, bool expected)
    {
        Assert.Equal(expected, LockScreenPolicy.IsPolicyEdition(edition));
    }

    [Fact]
    public void WarnsOnEditionsThatIgnoreThePolicy()
    {
        var warning = LockScreenPolicy.GetEditionWarning("Professional");

        Assert.NotNull(warning);
        Assert.Contains("'Professional'", warning);
        Assert.Contains(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization\LockScreenImage", warning);
        Assert.Contains("Windows PowerShell 5.1", warning);
        Assert.Null(LockScreenPolicy.GetEditionWarning("Enterprise"));
    }

    [Fact]
    public void LockScreenApiIsNotReachableOnDotNet8()
    {
        // The test host runs on .NET 8, like PowerShell 7: the lock screen always goes through the policy value there.
        Assert.False(LockScreenPolicy.IsLockScreenApiAvailable());
    }

    [Fact]
    public void ReadsTheEditionOfThisMachine()
    {
        Assert.False(string.IsNullOrWhiteSpace(LockScreenPolicy.ReadEditionId()));
    }
}
