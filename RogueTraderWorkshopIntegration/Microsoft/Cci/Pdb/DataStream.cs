namespace Microsoft.Cci.Pdb;

internal class DataStream
{
	internal int contentSize;

	internal int[] pages;

	internal int Length => contentSize;

	internal DataStream()
	{
	}

	internal DataStream(int contentSize, BitAccess bits, int count)
	{
		this.contentSize = contentSize;
		if (count > 0)
		{
			pages = new int[count];
			bits.ReadInt32(pages);
		}
	}

	internal void Read(PdbReader reader, BitAccess bits)
	{
		bits.MinCapacity(contentSize);
		Read(reader, 0, bits.Buffer, 0, contentSize);
	}

	internal void Read(PdbReader reader, int position, byte[] bytes, int offset, int data)
	{
		if (position + data > contentSize)
		{
			throw new PdbException("DataStream can't read off end of stream. (pos={0},siz={1})", position, data);
		}
		if (position == contentSize)
		{
			return;
		}
		int num = data;
		int num2 = position / reader.pageSize;
		int num3 = position % reader.pageSize;
		if (num3 != 0)
		{
			int num4 = reader.pageSize - num3;
			if (num4 > num)
			{
				num4 = num;
			}
			reader.Seek(pages[num2], num3);
			reader.Read(bytes, offset, num4);
			offset += num4;
			num -= num4;
			num2++;
		}
		while (num > 0)
		{
			int num5 = reader.pageSize;
			if (num5 > num)
			{
				num5 = num;
			}
			reader.Seek(pages[num2], 0);
			reader.Read(bytes, offset, num5);
			offset += num5;
			num -= num5;
			num2++;
		}
	}
}
