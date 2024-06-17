using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.Collections;

[BurstCompile]
internal struct FixedArray4<T> where T : unmanaged
{
	public T item0;

	public T item1;

	public T item2;

	public T item3;

	internal unsafe ref T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			fixed (T* ptr = &item0)
			{
				return ref ptr[index];
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FixedArray4(in T splat)
	{
		item0 = (item1 = (item2 = (item3 = splat)));
	}
}
