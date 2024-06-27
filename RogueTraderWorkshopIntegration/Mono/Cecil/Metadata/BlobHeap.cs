using System;

namespace Mono.Cecil.Metadata;

internal sealed class BlobHeap : Heap
{
	public BlobHeap(byte[] data)
		: base(data)
	{
	}

	public byte[] Read(uint index)
	{
		if (index == 0 || index > data.Length - 1)
		{
			return Empty<byte>.Array;
		}
		int position = (int)index;
		int num = (int)data.ReadCompressedUInt32(ref position);
		if (num > data.Length - position)
		{
			return Empty<byte>.Array;
		}
		byte[] array = new byte[num];
		Buffer.BlockCopy(data, position, array, 0, num);
		return array;
	}

	public void GetView(uint signature, out byte[] buffer, out int index, out int length)
	{
		if (signature == 0 || signature > data.Length - 1)
		{
			buffer = null;
			index = (length = 0);
		}
		else
		{
			buffer = data;
			index = (int)signature;
			length = (int)buffer.ReadCompressedUInt32(ref index);
		}
	}
}
