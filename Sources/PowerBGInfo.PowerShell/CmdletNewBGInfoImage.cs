using System.Management.Automation;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a BGInfo image overlay definition.</summary>
[Cmdlet(VerbsCommon.New, "BGInfoImage")]
[OutputType(typeof(BgInfoImage))]
public sealed class CmdletNewBGInfoImage : PSCmdlet {
    /// <para>Path to the image file.</para>
    [Parameter(Mandatory = true, Position = 0)]
    [Alias("FullName")]
    public string Path { get; set; } = string.Empty;

    /// <para>Target image width in pixels. Omit with Height to preserve aspect ratio.</para>
    [Parameter]
    public int Width { get; set; }

    /// <para>Target image height in pixels. Omit with Width to preserve aspect ratio.</para>
    [Parameter]
    public int Height { get; set; }

    /// <para>Anchor position for placement.</para>
    [Parameter]
    public BgInfoTextPosition Anchor { get; set; } = BgInfoTextPosition.BottomRight;

    /// <para>Horizontal offset from the anchor.</para>
    [Parameter]
    public int OffsetX { get; set; } = 32;

    /// <para>Vertical offset from the anchor.</para>
    [Parameter]
    public int OffsetY { get; set; } = 32;

    /// <para>Absolute X position for placement.</para>
    [Parameter]
    public int PositionX { get; set; }

    /// <para>Absolute Y position for placement.</para>
    [Parameter]
    public int PositionY { get; set; }

    /// <para>Image opacity from zero to one.</para>
    [Parameter]
    public double Opacity { get; set; } = 1d;

    /// <summary>Emits an image overlay definition.</summary>
    protected override void EndProcessing() {
        if (Width < 0) {
            ThrowTerminatingError(new ErrorRecord(new ArgumentOutOfRangeException(nameof(Width), Width, "Width cannot be negative."), "BGInfoImageInvalidWidth", ErrorCategory.InvalidArgument, Width));
            return;
        }
        if (Height < 0) {
            ThrowTerminatingError(new ErrorRecord(new ArgumentOutOfRangeException(nameof(Height), Height, "Height cannot be negative."), "BGInfoImageInvalidHeight", ErrorCategory.InvalidArgument, Height));
            return;
        }
        if (Opacity < 0d || Opacity > 1d) {
            ThrowTerminatingError(new ErrorRecord(new ArgumentOutOfRangeException(nameof(Opacity), Opacity, "Opacity must be between 0 and 1."), "BGInfoImageInvalidOpacity", ErrorCategory.InvalidArgument, Opacity));
            return;
        }

        var image = new BgInfoImage {
            Path = SessionState.Path.GetUnresolvedProviderPathFromPSPath(Path),
            Width = Width,
            Height = Height,
            Anchor = Anchor,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            Opacity = Opacity
        };

        if (MyInvocation.BoundParameters.ContainsKey(nameof(PositionX)) &&
            MyInvocation.BoundParameters.ContainsKey(nameof(PositionY))) {
            image.PositionX = PositionX;
            image.PositionY = PositionY;
        }

        WriteObject(image);
    }
}
