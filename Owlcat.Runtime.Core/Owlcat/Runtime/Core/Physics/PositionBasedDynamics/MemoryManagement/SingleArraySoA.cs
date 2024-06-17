using System.Runtime.InteropServices;
using Unity.Collections;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;

public class SingleArraySoA<T> : StructureOfArrayBase where T : struct
{
	public NativeArray<T> Array;

	public T this[int index]
	{
		get
		{
			return Array[index];
		}
		set
		{
			Array[index] = value;
		}
	}

	public SingleArraySoA()
		: this(64)
	{
	}

	public SingleArraySoA(int size)
		: base(size)
	{
		m_Allocator.Stride = Marshal.SizeOf<T>();
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Array = new NativeArray<T>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (Array.IsCreated)
		{
			Array.Dispose();
		}
	}
}
