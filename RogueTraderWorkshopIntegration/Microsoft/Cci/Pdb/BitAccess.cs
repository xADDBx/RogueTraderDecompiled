using System;
using System.IO;
using System.Text;

namespace Microsoft.Cci.Pdb;

internal class BitAccess
{
	private byte[] buffer;

	private int offset;

	internal byte[] Buffer => buffer;

	internal int Position
	{
		get
		{
			return offset;
		}
		set
		{
			offset = value;
		}
	}

	internal BitAccess(int capacity)
	{
		buffer = new byte[capacity];
	}

	internal BitAccess(byte[] buffer)
	{
		this.buffer = buffer;
		offset = 0;
	}

	internal void FillBuffer(Stream stream, int capacity)
	{
		MinCapacity(capacity);
		stream.Read(buffer, 0, capacity);
		offset = 0;
	}

	internal void Append(Stream stream, int count)
	{
		int num = offset + count;
		if (buffer.Length < num)
		{
			byte[] destinationArray = new byte[num];
			Array.Copy(buffer, destinationArray, buffer.Length);
			buffer = destinationArray;
		}
		stream.Read(buffer, offset, count);
		offset += count;
	}

	internal void MinCapacity(int capacity)
	{
		if (buffer.Length < capacity)
		{
			buffer = new byte[capacity];
		}
		offset = 0;
	}

	internal void Align(int alignment)
	{
		while (offset % alignment != 0)
		{
			offset++;
		}
	}

	internal void ReadInt16(out short value)
	{
		value = (short)((buffer[offset] & 0xFF) | (buffer[offset + 1] << 8));
		offset += 2;
	}

	internal void ReadInt8(out sbyte value)
	{
		value = (sbyte)buffer[offset];
		offset++;
	}

	internal void ReadInt32(out int value)
	{
		value = (buffer[offset] & 0xFF) | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
		offset += 4;
	}

	internal void ReadInt64(out long value)
	{
		value = ((long)buffer[offset] & 0xFFL) | (long)((ulong)buffer[offset + 1] << 8) | (long)((ulong)buffer[offset + 2] << 16) | (long)((ulong)buffer[offset + 3] << 24) | (long)((ulong)buffer[offset + 4] << 32) | (long)((ulong)buffer[offset + 5] << 40) | (long)((ulong)buffer[offset + 6] << 48) | (long)((ulong)buffer[offset + 7] << 56);
		offset += 8;
	}

	internal void ReadUInt16(out ushort value)
	{
		value = (ushort)((buffer[offset] & 0xFFu) | (uint)(buffer[offset + 1] << 8));
		offset += 2;
	}

	internal void ReadUInt8(out byte value)
	{
		value = (byte)(buffer[offset] & 0xFFu);
		offset++;
	}

	internal void ReadUInt32(out uint value)
	{
		value = (buffer[offset] & 0xFFu) | (uint)(buffer[offset + 1] << 8) | (uint)(buffer[offset + 2] << 16) | (uint)(buffer[offset + 3] << 24);
		offset += 4;
	}

	internal void ReadUInt64(out ulong value)
	{
		value = ((ulong)buffer[offset] & 0xFFuL) | ((ulong)buffer[offset + 1] << 8) | ((ulong)buffer[offset + 2] << 16) | ((ulong)buffer[offset + 3] << 24) | ((ulong)buffer[offset + 4] << 32) | ((ulong)buffer[offset + 5] << 40) | ((ulong)buffer[offset + 6] << 48) | ((ulong)buffer[offset + 7] << 56);
		offset += 8;
	}

	internal void ReadInt32(int[] values)
	{
		for (int i = 0; i < values.Length; i++)
		{
			ReadInt32(out values[i]);
		}
	}

	internal void ReadUInt32(uint[] values)
	{
		for (int i = 0; i < values.Length; i++)
		{
			ReadUInt32(out values[i]);
		}
	}

	internal void ReadBytes(byte[] bytes)
	{
		for (int i = 0; i < bytes.Length; i++)
		{
			bytes[i] = buffer[offset++];
		}
	}

	internal float ReadFloat()
	{
		float result = BitConverter.ToSingle(buffer, offset);
		offset += 4;
		return result;
	}

	internal double ReadDouble()
	{
		double result = BitConverter.ToDouble(buffer, offset);
		offset += 8;
		return result;
	}

	internal decimal ReadDecimal()
	{
		int[] array = new int[4];
		ReadInt32(array);
		return new decimal(array[2], array[3], array[1], array[0] < 0, (byte)((array[0] & 0xFF0000) >> 16));
	}

	internal void ReadBString(out string value)
	{
		ReadUInt16(out var value2);
		value = Encoding.UTF8.GetString(buffer, offset, value2);
		offset += value2;
	}

	internal string ReadBString(int len)
	{
		string @string = Encoding.UTF8.GetString(buffer, offset, len);
		offset += len;
		return @string;
	}

	internal void ReadCString(out string value)
	{
		int i;
		for (i = 0; offset + i < buffer.Length && buffer[offset + i] != 0; i++)
		{
		}
		value = Encoding.UTF8.GetString(buffer, offset, i);
		offset += i + 1;
	}

	internal void SkipCString(out string value)
	{
		int i;
		for (i = 0; offset + i < buffer.Length && buffer[offset + i] != 0; i++)
		{
		}
		offset += i + 1;
		value = null;
	}

	internal void ReadGuid(out Guid guid)
	{
		ReadUInt32(out var value);
		ReadUInt16(out var value2);
		ReadUInt16(out var value3);
		ReadUInt8(out var value4);
		ReadUInt8(out var value5);
		ReadUInt8(out var value6);
		ReadUInt8(out var value7);
		ReadUInt8(out var value8);
		ReadUInt8(out var value9);
		ReadUInt8(out var value10);
		ReadUInt8(out var value11);
		guid = new Guid(value, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11);
	}

	internal string ReadString()
	{
		int i;
		for (i = 0; offset + i < buffer.Length && buffer[offset + i] != 0; i += 2)
		{
		}
		string @string = Encoding.Unicode.GetString(buffer, offset, i);
		offset += i + 2;
		return @string;
	}
}
