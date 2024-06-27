namespace Microsoft.Cci.Pdb;

internal struct BitSet
{
	private int size;

	private uint[] words;

	internal bool IsEmpty => size == 0;

	internal BitSet(BitAccess bits)
	{
		bits.ReadInt32(out size);
		words = new uint[size];
		bits.ReadUInt32(words);
	}

	internal bool IsSet(int index)
	{
		int num = index / 32;
		if (num >= size)
		{
			return false;
		}
		return (words[num] & GetBit(index)) != 0;
	}

	private static uint GetBit(int index)
	{
		return (uint)(1 << index % 32);
	}
}
