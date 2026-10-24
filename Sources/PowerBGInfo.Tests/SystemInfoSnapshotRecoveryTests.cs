using System.Linq.Expressions;
using System.Reflection;

namespace PowerBGInfo.Tests;

[CollectionDefinition("OS metadata boundary", DisableParallelization = true)]
public sealed class OsMetadataBoundaryCollection { }

[Collection("OS metadata boundary")]
public sealed class SystemInfoSnapshotRecoveryTests {
    [Fact]
    public void SnapshotUsesOneUnavailableOsReadAndTheNextSnapshotRetries() {
        if (SystemInfoProvider.DefaultProvider.GetType().Name != "WindowsSystemInfoProvider") return;
        // Substitute only the existing external-fact loader, without changing cache policy
        // or adding a production hook. Other snapshot facts retain their read-only providers.
        var cacheField = SystemInfoProvider.DefaultProvider.GetType().GetField("OsInfoCache", BindingFlags.Static | BindingFlags.NonPublic)!;
        var cache = cacheField.GetValue(null)!;
        var cacheType = cache.GetType();
        var loadField = cacheType.GetField("_load", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var valueField = cacheType.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var originalLoad = loadField.GetValue(cache);
        var originalValue = valueField.GetValue(cache);
        var infoType = cacheType.GetGenericArguments()[0];
        var reads = 0;
        Func<object> unavailable = () => { reads++; return Activator.CreateInstance(infoType, nonPublic: true)!; };
        var replacement = Expression.Lambda(loadField.FieldType,
            Expression.Convert(Expression.Invoke(Expression.Constant(unavailable)), infoType)).Compile();
        try {
            loadField.SetValue(cache, replacement);
            valueField.SetValue(cache, null);
            Assert.Equal(SystemInfoProvider.Unknown, SystemInfoProvider.GetSnapshot().OSName);
            Assert.Equal(1, reads);
            Assert.Equal(SystemInfoProvider.Unknown, SystemInfoProvider.GetSnapshot().OSName);
            Assert.Equal(2, reads);
        } finally {
            loadField.SetValue(cache, originalLoad);
            valueField.SetValue(cache, originalValue);
        }
    }
}
