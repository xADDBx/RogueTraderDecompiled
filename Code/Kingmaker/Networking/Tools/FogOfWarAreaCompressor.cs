using System;
using System.Buffers.Binary;
using System.IO;

namespace Kingmaker.Networking.Tools;

public class FogOfWarAreaCompressor
{
	private const int OnePixelSize = 3;

	private const int GreenChannelOffset = 1;

	private readonly byte[] m_InBuffer;

	private readonly byte[] m_OutBuffer;

	public FogOfWarAreaCompressor(long capacity)
	{
		m_InBuffer = new byte[capacity];
		m_OutBuffer = new byte[capacity];
	}

	public void Compress(Stream inEntryStream, Stream outEntryStream)
	{
		int num = inEntryStream.Read(m_InBuffer, 0, m_InBuffer.Length);
		BinaryPrimitives.WriteInt32BigEndian(m_OutBuffer.AsSpan(0, 4), num);
		int num2 = num / 3 / 2;
		Span<byte> span = m_OutBuffer.AsSpan(4, num2);
		for (int i = 0; i < span.Length; i++)
		{
			int num3 = i * 2 * 3 + 1;
			int num4 = m_InBuffer[num3];
			num4 /= 16;
			if (16 <= num4)
			{
				PFLog.Net.Error($"[TEST] pixelDataA {m_InBuffer[num3]} {num4}");
			}
			num3 = (i * 2 + 1) * 3 + 1;
			int num5 = m_InBuffer[num3];
			num5 /= 16;
			if (16 <= num5)
			{
				PFLog.Net.Error($"[TEST] pixelDataB {m_InBuffer[num3]} {num5}");
			}
			int num6 = (num4 << 4) | num5;
			if (256 <= num6)
			{
				PFLog.Net.Error($"[TEST] b {num4} {num5} {num6}");
			}
			span[i] = (byte)num6;
		}
		outEntryStream.Write(m_OutBuffer, 0, 4 + num2);
	}

	public void Uncompress(Stream inEntryStream, Stream outEntryStream)
	{
		int num = inEntryStream.Read(m_InBuffer, 0, m_InBuffer.Length);
		int count = BinaryPrimitives.ReadInt32BigEndian(m_InBuffer.AsSpan(0, 4));
		Span<byte> span = m_InBuffer.AsSpan(4, num - 4);
		for (int i = 0; i < span.Length; i++)
		{
			byte num2 = span[i];
			int num3 = num2 >> 4;
			int num4 = num3;
			num4 = num4 * 255 / 15;
			if (256 <= num4)
			{
				PFLog.Net.Error($"[TEST] pixelDataA {i} {num3} {num4}");
			}
			num3 = num2 % 16;
			int num5 = num3;
			num5 = num5 * 255 / 15;
			if (256 <= num5)
			{
				PFLog.Net.Error($"[TEST] pixelDataB {i} {num3} {num5}");
			}
			int num6 = i * 2 * 3 + 1;
			m_OutBuffer[num6] = (byte)num4;
			num6 = (i * 2 + 1) * 3 + 1;
			m_OutBuffer[num6] = (byte)num5;
		}
		outEntryStream.Write(m_OutBuffer, 0, count);
	}
}
