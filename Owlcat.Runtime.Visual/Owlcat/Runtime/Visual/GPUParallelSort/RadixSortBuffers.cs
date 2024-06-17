using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUParallelSort;

public class RadixSortBuffers
{
	public ComputeBuffer[] KeysBuffers { get; private set; }

	public ComputeBuffer[] ValuesBuffers { get; private set; }

	public string Name { get; private set; }

	public RadixSortBuffers(string name, int size)
	{
		KeysBuffers = new ComputeBuffer[2];
		ValuesBuffers = new ComputeBuffer[2];
		Name = name;
		Resize(size);
	}

	public void Resize(int newSize)
	{
		for (int i = 0; i < 2; i++)
		{
			if (KeysBuffers[i] != null)
			{
				KeysBuffers[i].Release();
			}
			KeysBuffers[i] = new ComputeBuffer(newSize, 4, ComputeBufferType.Structured);
			KeysBuffers[i].name = $"{Name}_RadixSort.KeysBuffer_{i}";
			if (ValuesBuffers[i] != null)
			{
				ValuesBuffers[i].Release();
			}
			ValuesBuffers[i] = new ComputeBuffer(newSize, 4, ComputeBufferType.Structured);
			ValuesBuffers[i].name = $"{Name}_RadixSort.ValuesBuffer_{i}";
		}
	}

	public void Dispose()
	{
		for (int i = 0; i < 2; i++)
		{
			if (KeysBuffers[i] != null)
			{
				KeysBuffers[i].Release();
			}
			if (ValuesBuffers[i] != null)
			{
				ValuesBuffers[i].Release();
			}
		}
	}
}
