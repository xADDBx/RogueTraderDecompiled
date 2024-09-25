using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.Collections;

[BurstCompile]
public readonly struct UnsafeSpan<T> where T : unmanaged
{
	[BurstCompile]
	public struct Enumerator
	{
		private unsafe T* m_Current;

		private unsafe readonly T* m_End;

		public unsafe T* Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Current;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Enumerator(in UnsafeSpan<T> span)
		{
			m_Current = span.m_Ptr - 1;
			m_End = span.m_Ptr + span.m_Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe bool MoveNext()
		{
			return ++m_Current < m_End;
		}
	}

	private unsafe readonly T* m_Ptr;

	private readonly int m_Length;

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe UnsafeSpan(T* ptr, int length)
	{
		m_Ptr = ptr;
		m_Length = length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Enumerator GetEnumerator()
	{
		return new Enumerator(in this);
	}
}
