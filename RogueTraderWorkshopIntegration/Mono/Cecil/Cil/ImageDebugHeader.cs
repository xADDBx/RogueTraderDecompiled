using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class ImageDebugHeader
{
	private readonly ImageDebugHeaderEntry[] entries;

	public bool HasEntries => !entries.IsNullOrEmpty();

	public ImageDebugHeaderEntry[] Entries => entries;

	public ImageDebugHeader(ImageDebugHeaderEntry[] entries)
	{
		this.entries = entries ?? Empty<ImageDebugHeaderEntry>.Array;
	}

	public ImageDebugHeader()
		: this(Empty<ImageDebugHeaderEntry>.Array)
	{
	}

	public ImageDebugHeader(ImageDebugHeaderEntry entry)
		: this(new ImageDebugHeaderEntry[1] { entry })
	{
	}
}
