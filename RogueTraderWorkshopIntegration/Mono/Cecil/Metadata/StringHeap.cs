using System.Collections.Generic;
using System.Text;

namespace Mono.Cecil.Metadata;

internal class StringHeap : Heap
{
	private readonly Dictionary<uint, string> strings = new Dictionary<uint, string>();

	public StringHeap(byte[] data)
		: base(data)
	{
	}

	public string Read(uint index)
	{
		if (index == 0)
		{
			return string.Empty;
		}
		if (strings.TryGetValue(index, out var value))
		{
			return value;
		}
		if (index > data.Length - 1)
		{
			return string.Empty;
		}
		value = ReadStringAt(index);
		if (value.Length != 0)
		{
			strings.Add(index, value);
		}
		return value;
	}

	protected virtual string ReadStringAt(uint index)
	{
		int num = 0;
		for (int i = (int)index; data[i] != 0; i++)
		{
			num++;
		}
		return Encoding.UTF8.GetString(data, (int)index, num);
	}
}
