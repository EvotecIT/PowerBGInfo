using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace PowerBGInfo;

public static partial class SystemInfoProvider {
    private static readonly SystemInfoQueryExecutor QueryExecutor = new();

    /// <summary>Bounds connection, enumeration and disposal without accumulating stalled WMI workers.</summary>
    private static List<Dictionary<string, object?>> Query(string scope, string query) =>
        QueryExecutor.Run(() => CollectQueryRows(scope, query), QueryTimeout)
        ?? new List<Dictionary<string, object?>>();

    private static List<Dictionary<string, object?>> CollectQueryRows(string scope, string query) {
        var timer = Stopwatch.StartNew();
        var rows = new List<Dictionary<string, object?>>();
        var options = new System.Management.EnumerationOptions {
            Timeout = QueryTimeout, Rewindable = false, ReturnImmediately = true
        };
        using var searcher = new ManagementObjectSearcher(new ManagementScope(scope), new ObjectQuery(query), options);
        using var results = searcher.Get();
        foreach (ManagementObject item in results) {
            using (item) {
                // A timed-out caller no longer needs rows, even if connection subsequently recovers.
                if (timer.Elapsed >= QueryTimeout) break;
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (PropertyData property in item.Properties) row[property.Name] = property.Value;
                rows.Add(row);
            }
        }
        return rows;
    }
}
