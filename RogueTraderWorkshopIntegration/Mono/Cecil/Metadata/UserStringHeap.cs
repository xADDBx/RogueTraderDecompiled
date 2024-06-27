namespace Mono.Cecil.Metadata;

internal sealed class UserStringHeap : StringHeap
{
	public UserStringHeap(byte[] data)
		: base(data)
	{
	}

	protected override string ReadStringAt(uint index)
	{
		int position = (int)index;
		uint num = (uint)(data.ReadCompressedUInt32(ref position) & -2);
		if (num < 1)
		{
			return string.Empty;
		}
		char[] array = new char[num / 2];
		int i = position;
		int num2 = 0;
		for (; i < position + num; i += 2)
		{
			array[num2++] = (char)(data[i] | (data[i + 1] << 8));
		}
		return new string(array);
	}
}
