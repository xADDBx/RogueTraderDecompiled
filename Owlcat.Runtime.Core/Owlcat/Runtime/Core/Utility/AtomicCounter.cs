using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;

namespace Owlcat.Runtime.Core.Utility;

public struct AtomicCounter
{
	[NativeDisableUnsafePtrRestriction]
	private unsafe int* m_Counter;

	public unsafe int Count => *m_Counter;

	public unsafe AtomicCounter(void* ptr)
	{
		m_Counter = (int*)ptr;
		*m_Counter = 0;
	}

	public unsafe int Increment()
	{
		return Interlocked.Increment(ref *m_Counter);
	}

	public unsafe void AddFloat(float value)
	{
		int comparand = Asint(value);
		int num = 0;
		while (true)
		{
			int num2 = Interlocked.CompareExchange(ref *m_Counter, num, comparand);
			if (num2 != num)
			{
				num = num2;
				comparand = Asint(value + Asfloat(num2));
				continue;
			}
			break;
		}
	}

	public unsafe void Reset(int value = 0)
	{
		*m_Counter = value;
	}

	public unsafe void Dispose()
	{
		Marshal.FreeHGlobal((IntPtr)m_Counter);
	}

	public unsafe static AtomicCounter Create()
	{
		return new AtomicCounter(Marshal.AllocHGlobal(Marshal.SizeOf<int>()).ToPointer());
	}

	public unsafe static int Asint(float val)
	{
		return *(int*)(&val);
	}

	public unsafe static float Asfloat(int val)
	{
		return *(float*)(&val);
	}
}
