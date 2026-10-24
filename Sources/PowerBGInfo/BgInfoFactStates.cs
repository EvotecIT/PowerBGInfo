using System.Collections.ObjectModel;

namespace PowerBGInfo;

/// <summary>Maps snapshot values to explicit availability without inventing equivalent security products on another OS.</summary>
internal static class BgInfoFactStates {
    internal static IReadOnlyDictionary<string, BgInfoFactState> FromSnapshot(BgInfoSystemInfo snapshot, bool windowsFacts) {
        var states = new Dictionary<string, BgInfoFactState>(StringComparer.OrdinalIgnoreCase) {
            [nameof(snapshot.ComputerName)] = Text(snapshot.ComputerName),
            [nameof(snapshot.UserName)] = Text(snapshot.UserName),
            [nameof(snapshot.LoggedOnUser)] = Text(snapshot.LoggedOnUser),
            [nameof(snapshot.Domain)] = Text(snapshot.Domain),
            [nameof(snapshot.OSName)] = Text(snapshot.OSName),
            [nameof(snapshot.OSVersion)] = Text(snapshot.OSVersion),
            [nameof(snapshot.OSBuild)] = Text(snapshot.OSBuild),
            [nameof(snapshot.LastBootTime)] = Value(snapshot.LastBootTime.HasValue),
            [nameof(snapshot.Uptime)] = Value(snapshot.Uptime.HasValue),
            [nameof(snapshot.UptimeText)] = Text(snapshot.UptimeText),
            [nameof(snapshot.IPv4Address)] = Text(snapshot.IPv4Address),
            [nameof(snapshot.Disks)] = Value(snapshot.Disks.Length > 0),
            [nameof(snapshot.LastPatchId)] = Windows(Text(snapshot.LastPatchId), windowsFacts),
            [nameof(snapshot.LastPatchDate)] = Windows(Value(snapshot.LastPatchDate.HasValue), windowsFacts),
            [nameof(snapshot.DefenderStatus)] = Windows(Text(snapshot.DefenderStatus), windowsFacts),
            [nameof(snapshot.DefenderSignatureAge)] = Windows(Value(snapshot.DefenderSignatureAge.HasValue), windowsFacts),
            [nameof(snapshot.BitLockerStatus)] = Windows(Text(snapshot.BitLockerStatus), windowsFacts)
        };
        return new ReadOnlyDictionary<string, BgInfoFactState>(states);
    }

    private static BgInfoFactState Text(string? value) => Value(!string.IsNullOrWhiteSpace(value) && value != SystemInfoProvider.Unknown);
    private static BgInfoFactState Value(bool available) => available ? BgInfoFactState.Available : BgInfoFactState.Unavailable;
    private static BgInfoFactState Windows(BgInfoFactState state, bool supported) => supported ? state : BgInfoFactState.Unsupported;
}
