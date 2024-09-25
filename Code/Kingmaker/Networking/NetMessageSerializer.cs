using System;
using System.Buffers;
using ExitGames.Client.Photon;
using MemoryPack;

namespace Kingmaker.Networking;

public static class NetMessageSerializer
{
	private static CustomArrayBufferWriter<byte> MemoryPackBuffer => MessageNetManager.SendBytes;

	public static ByteArraySlice SerializeToSlice<T>(T value)
	{
		MemoryPackBuffer.Clear();
		IBufferWriter<byte> bufferWriter = MemoryPackBuffer;
		MemoryPackSerializer.Serialize(in bufferWriter, in value);
		return PhotonManager.Instance.ByteArraySlicePool.Acquire(MemoryPackBuffer.GetArray(), 0, MemoryPackBuffer.WrittenCount);
	}

	public static T DeserializeFromSlice<T>(ByteArraySlice slice)
	{
		return DeserializeFromSpan<T>(new ReadOnlySpan<byte>(slice.Buffer, slice.Offset, slice.Count));
	}

	public static T DeserializeFromSpan<T>(ReadOnlySpan<byte> span)
	{
		return MemoryPackSerializer.Deserialize<T>(span);
	}
}
