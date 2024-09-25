using System;
using Unity.Collections.LowLevel.Unsafe;

namespace StbDxtSharp;

internal static class CRuntime
{
	public unsafe static void memcpy(void* a, void* b, long size)
	{
		UnsafeUtility.MemCpy(a, b, size);
	}

	public unsafe static void memcpy(void* a, void* b, ulong size)
	{
		memcpy(a, b, (long)size);
	}

	public unsafe static void memset(void* ptr, int value, long size)
	{
		UnsafeUtility.MemSet(ptr, (byte)value, size);
	}

	public unsafe static void memset(void* ptr, int value, ulong size)
	{
		memset(ptr, value, (long)size);
	}

	public static int abs(int v)
	{
		return Math.Abs(v);
	}

	public static float fabs(double a)
	{
		return (float)Math.Abs(a);
	}
}
