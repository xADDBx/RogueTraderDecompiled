using System;
using System.Collections.Generic;

namespace Mono.Cecil.Metadata;

internal sealed class GuidHeapBuffer : HeapBuffer
{
	private readonly Dictionary<Guid, uint> guids = new Dictionary<Guid, uint>();

	public override bool IsEmpty => length == 0;

	public GuidHeapBuffer()
		: base(16)
	{
	}

	public uint GetGuidIndex(Guid guid)
	{
		if (guids.TryGetValue(guid, out var value))
		{
			return value;
		}
		value = (uint)(guids.Count + 1);
		WriteGuid(guid);
		guids.Add(guid, value);
		return value;
	}

	private void WriteGuid(Guid guid)
	{
		WriteBytes(guid.ToByteArray());
	}
}
