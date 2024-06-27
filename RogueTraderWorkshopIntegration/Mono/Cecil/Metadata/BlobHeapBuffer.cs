using System.Collections.Generic;
using Mono.Cecil.PE;

namespace Mono.Cecil.Metadata;

internal sealed class BlobHeapBuffer : HeapBuffer
{
	private readonly Dictionary<ByteBuffer, uint> blobs = new Dictionary<ByteBuffer, uint>(new ByteBufferEqualityComparer());

	public override bool IsEmpty => length <= 1;

	public BlobHeapBuffer()
		: base(1)
	{
		WriteByte(0);
	}

	public uint GetBlobIndex(ByteBuffer blob)
	{
		if (blobs.TryGetValue(blob, out var value))
		{
			return value;
		}
		value = (uint)position;
		WriteBlob(blob);
		blobs.Add(blob, value);
		return value;
	}

	private void WriteBlob(ByteBuffer blob)
	{
		WriteCompressedUInt32((uint)blob.length);
		WriteBytes(blob);
	}
}
