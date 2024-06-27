using Mono.Cecil.PE;

namespace Mono.Cecil.Metadata;

internal sealed class DataBuffer : ByteBuffer
{
	private int buffer_align = 4;

	public int BufferAlign => buffer_align;

	public DataBuffer()
		: base(0)
	{
	}

	private void Align(int align)
	{
		align--;
		WriteBytes(((position + align) & ~align) - position);
	}

	public uint AddData(byte[] data, int align)
	{
		if (buffer_align < align)
		{
			buffer_align = align;
		}
		Align(align);
		int result = position;
		WriteBytes(data);
		return (uint)result;
	}
}
