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
        Assert.Contains("-Target File", warning);
        Assert.Null(LockScreenPolicy.GetEditionWarning("Enterprise"));
    }

    [Theory]
    [InlineData(DesktopManager.LogonWallpaperMechanism.WindowsRuntime)]
    [InlineData(DesktopManager.LogonWallpaperMechanism.None)]
    public void DoesNotReadOrWarnAboutEditionsWithoutAPolicyWrite(DesktopManager.LogonWallpaperMechanism mechanism)
    {
        Assert.Null(LockScreenPolicy.GetFallbackWarning(mechanism,
            () => throw new InvalidOperationException("No policy was written, so edition information is irrelevant.")));
    }

    [Fact]
    public void ReportsEditionLimitsForTheMechanismDesktopManagerActuallyUsed()
    {
        var mechanism = DesktopManager.LogonWallpaperMechanism.PersonalizationPolicy;
        Assert.NotNull(LockScreenPolicy.GetFallbackWarning(mechanism, () => "Professional"));
        Assert.Null(LockScreenPolicy.GetFallbackWarning(mechanism, () => "Enterprise"));
    }

    [Fact]
    public void ReadsTheEditionOfThisMachine()
    {
        Assert.False(string.IsNullOrWhiteSpace(LockScreenPolicy.ReadEditionId()));
    }
}
