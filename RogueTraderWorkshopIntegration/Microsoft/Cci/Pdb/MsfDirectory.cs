namespace Microsoft.Cci.Pdb;

internal class MsfDirectory
{
	internal DataStream[] streams;

	internal MsfDirectory(PdbReader reader, PdbFileHeader head, BitAccess bits)
	{
		int num = reader.PagesFromSize(head.directorySize);
		bits.MinCapacity(head.directorySize);
		int num2 = head.directoryRoot.Length;
		int num3 = head.pageSize / 4;
		int num4 = num;
		for (int i = 0; i < num2; i++)
		{
			int num5 = ((num4 <= num3) ? num4 : num3);
			reader.Seek(head.directoryRoot[i], 0);
			bits.Append(reader.reader, num5 * 4);
			num4 -= num5;
		}
		bits.Position = 0;
		DataStream dataStream = new DataStream(head.directorySize, bits, num);
		bits.MinCapacity(head.directorySize);
		dataStream.Read(reader, bits);
		bits.ReadInt32(out var value);
		int[] array = new int[value];
		bits.ReadInt32(array);
		streams = new DataStream[value];
		for (int j = 0; j < value; j++)
		{
			if (array[j] <= 0)
			{
				streams[j] = new DataStream();
			}
			else
			{
				streams[j] = new DataStream(array[j], bits, reader.PagesFromSize(array[j]));
			}
		}
	}
}
