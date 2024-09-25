using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct NativeStack<T> where T : unmanaged
{
	private int m_Head;

	public NativeList<T> m_List;

	public bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_List.IsCreated;
		}
	}

	public int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_List.Length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeStack(int initialCapacity, Allocator allocator)
	{
		m_Head = -1;
		m_List = new NativeList<T>(initialCapacity, allocator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Push(T item)
	{
		m_List.Add(in item);
		m_Head++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Pop()
	{
		T result = m_List[m_Head];
		m_List.RemoveAt(m_Head);
		m_Head--;
		return result;
	}

	public void Dispose()
	{
		m_List.Dispose();
	}
}
