using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class ImageDebugHeaderEntry
{
	private ImageDebugDirectory directory;

	private readonly byte[] data;

	public ImageDebugDirectory Directory
	{
		get
		{
			return directory;
		}
		internal set
		{
			directory = value;
		}
	}

	public byte[] Data => data;

	public ImageDebugHeaderEntry(ImageDebugDirectory directory, byte[] data)
	{
		this.directory = directory;
		this.data = data ?? Empty<byte>.Array;
	}
}
