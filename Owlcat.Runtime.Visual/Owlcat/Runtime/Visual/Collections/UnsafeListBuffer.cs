using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.Collections;

[BurstCompile]
public struct UnsafeListBuffer<T> where T : unmanaged
{
	private unsafe readonly T* m_BufferPtr;

	private unsafe T* m_BackPtr;

	public unsafe int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)(m_BackPtr - m_BufferPtr);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe UnsafeListBuffer(T* bufferPtr, int bufferSize)
	{
		m_BufferPtr = (m_BackPtr = bufferPtr);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Push(in T value)
	{
		*m_BackPtr = value;
		m_BackPtr++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe T Pop()
	{
		m_BackPtr--;
		return *m_BackPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe Span<T> AsSpan()
	{
		return new Span<T>(m_BufferPtr, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe UnsafeSpan<T> AsUnsafeSpan()
	{
		return new UnsafeSpan<T>(m_BufferPtr, Length);
	}
}
