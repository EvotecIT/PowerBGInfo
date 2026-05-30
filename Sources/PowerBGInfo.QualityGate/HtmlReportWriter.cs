using System.Globalization;
using System.Net;
using System.Text;

namespace PowerBGInfo.QualityGate;

internal static class HtmlReportWriter {
    public static void Write(ImageComparisonReport report, string path) {
        var builder = new StringBuilder(8192);
        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        builder.AppendLine("<title>PowerBGInfo Image Quality Report</title>");
        builder.AppendLine("<style>");
        builder.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;margin:24px;background:#f6f7f9;color:#18202a}h1{font-size:24px;margin:0 0 12px}.summary{display:flex;gap:12px;flex-wrap:wrap;margin:16px 0}.metric{background:white;border:1px solid #d8dee8;border-radius:6px;padding:10px 12px;min-width:130px}.metric strong{display:block;font-size:22px}.case{background:white;border:1px solid #d8dee8;border-radius:6px;margin:18px 0;padding:14px}.case.fail{border-color:#d34b4b}.case.pass{border-color:#4b9b62}.grid{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:12px}.grid img{width:100%;height:auto;border:1px solid #ccd3df;background:#fff}.label{font-weight:700;margin:6px 0}.small{color:#526172;font-size:12px}.bad{color:#b42318}.good{color:#16703a}code{background:#edf1f6;padding:2px 4px;border-radius:4px}@media(max-width:900px){.grid{grid-template-columns:1fr}}");
        builder.AppendLine("</style></head><body>");
        builder.AppendLine("<h1>PowerBGInfo Image Quality Report</h1>");
        builder.Append("<div class=\"small\">Created ")
            .Append(WebUtility.HtmlEncode(report.CreatedAt.ToString("u", CultureInfo.InvariantCulture)))
            .Append(" from <code>")
            .Append(WebUtility.HtmlEncode(report.BaselineDirectory))
            .Append("</code> to <code>")
            .Append(WebUtility.HtmlEncode(report.CandidateDirectory))
            .AppendLine("</code></div>");
        builder.AppendLine("<div class=\"summary\">");
        Metric(builder, "Compared", report.Compared.ToString(CultureInfo.InvariantCulture));
        Metric(builder, "Passed", report.Passed.ToString(CultureInfo.InvariantCulture), "good");
        Metric(builder, "Failed", report.Failed.ToString(CultureInfo.InvariantCulture), report.Failed == 0 ? "good" : "bad");
        Metric(builder, "Missing", report.Missing.ToString(CultureInfo.InvariantCulture), report.Missing == 0 ? "good" : "bad");
        builder.AppendLine("</div>");
        builder.Append("<p class=\"small\">Thresholds: mean <= ")
            .Append(report.MeanThreshold.ToString("0.###", CultureInfo.InvariantCulture))
            .Append(", RMSE <= ")
            .Append(report.RmseThreshold.ToString("0.###", CultureInfo.InvariantCulture))
            .Append(", max channel <= ")
            .Append(report.MaxChannelThreshold.ToString(CultureInfo.InvariantCulture))
            .Append(", changed pixels <= ")
            .Append(report.ChangedPixelPercentThreshold.ToString("0.###", CultureInfo.InvariantCulture))
            .AppendLine("%.</p>");

        foreach (var result in report.Results.OrderBy(r => r.Passed).ThenBy(r => r.RelativePath, StringComparer.OrdinalIgnoreCase)) {
            WriteCase(builder, result);
        }

        builder.AppendLine("</body></html>");
        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    private static void WriteCase(StringBuilder builder, ImageComparisonResult result) {
        builder.Append("<section class=\"case ").Append(result.Passed ? "pass" : "fail").AppendLine("\">");
        builder.Append("<h2>").Append(WebUtility.HtmlEncode(result.RelativePath)).Append("</h2>");
        builder.Append("<div class=\"small ").Append(result.Passed ? "good" : "bad").Append("\">")
            .Append(WebUtility.HtmlEncode(result.Message)).AppendLine("</div>");
        builder.Append("<p class=\"small\">");
        builder.Append("Mean ").Append(result.MeanAbsoluteChannelError.ToString("0.###", CultureInfo.InvariantCulture)).Append(" | ");
        builder.Append("RMSE ").Append(result.RmseChannelError.ToString("0.###", CultureInfo.InvariantCulture)).Append(" | ");
        builder.Append("Max ").Append(result.MaxChannelError.ToString(CultureInfo.InvariantCulture)).Append(" | ");
        builder.Append("Changed ").Append(result.ChangedPixelPercent.ToString("0.###", CultureInfo.InvariantCulture)).Append("%");
        builder.AppendLine("</p>");
        if (!result.MissingCandidate && result.DimensionsMatch) {
            builder.AppendLine("<div class=\"grid\">");
            Image(builder, "Baseline", result.BaselinePath);
            Image(builder, "Candidate", result.CandidatePath);
            Image(builder, "Diff heatmap", result.DiffPath);
            builder.AppendLine("</div>");
        }
        builder.AppendLine("</section>");
    }

    private static void Metric(StringBuilder builder, string label, string value, string css = "") {
        builder.Append("<div class=\"metric ").Append(css).Append("\"><span>")
            .Append(WebUtility.HtmlEncode(label)).Append("</span><strong>")
            .Append(WebUtility.HtmlEncode(value)).AppendLine("</strong></div>");
    }

    private static void Image(StringBuilder builder, string label, string path) {
        builder.Append("<div><div class=\"label\">").Append(WebUtility.HtmlEncode(label)).Append("</div><img src=\"")
            .Append(DataUri(path)).Append("\" alt=\"").Append(WebUtility.HtmlEncode(label)).AppendLine("\"></div>");
    }

    private static string DataUri(string path) {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        var mime = extension is ".jpg" or ".jpeg" ? "image/jpeg" : "image/png";
        return "data:" + mime + ";base64," + Convert.ToBase64String(File.ReadAllBytes(path));
    }
}
