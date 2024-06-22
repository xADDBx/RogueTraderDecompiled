using System;
using System.Buffers.Binary;

namespace Kingmaker.Networking;

public static class AckPacketHelper
{
	private const int AckPacketSize = 8;

	private static readonly byte[] SendBytesBuffer = new byte[8];

	public static (byte[] SendBytesBuffer, int Length) Make(int senderUniqueNumber, int currentLength)
	{
		BinaryPrimitives.WriteInt32BigEndian(SendBytesBuffer.AsSpan(0, 4), senderUniqueNumber);
		BinaryPrimitives.WriteInt32BigEndian(SendBytesBuffer.AsSpan(4, 4), currentLength);
		return (SendBytesBuffer: SendBytesBuffer, Length: 8);
	}

	public static bool TryGetOffset(PhotonActorNumber player, int uniqueNumber, ReadOnlySpan<byte> bytes, out int currentOffset)
	{
		currentOffset = -1;
		if (bytes.Length != 8)
		{
			PFLog.Net.Error($"[TryGetOffset] unexpected packet size! {bytes.Length}/{8}, {player}, N={uniqueNumber}");
			return false;
		}
		int num = BinaryPrimitives.ReadInt32BigEndian(bytes);
		if (num != uniqueNumber)
		{
			PFLog.Net.Error($"[TryGetOffset] unexpected unique number! received={num}, N={uniqueNumber}");
			return false;
		}
		currentOffset = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(4));
		if (currentOffset < 0)
		{
			PFLog.Net.Error($"[TryGetOffset] unexpected offset! Player #{player} offset={currentOffset}, N={uniqueNumber}");
			return false;
		}
		return true;
	}
}
