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

	public static bool CheckAck(PhotonActorNumber player, int uniqueNumber, int currentOffset, ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length != 8)
		{
			PFLog.Net.Error($"[CheckAck] unexpected packet size! {bytes.Length}/{8}, {player}, N={uniqueNumber}");
			return false;
		}
		if (BinaryPrimitives.ReadInt32BigEndian(bytes) != uniqueNumber)
		{
			PFLog.Net.Error($"[CheckAck] unexpected unique number! {bytes.Length}/{8}, N={uniqueNumber}");
			return false;
		}
		int num = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(4));
		if (currentOffset != num)
		{
			PFLog.Net.Error($"[CheckAck] unexpected offset! Player #{player} offset={num}/{currentOffset}, N={uniqueNumber}");
			return false;
		}
		return true;
	}
}
