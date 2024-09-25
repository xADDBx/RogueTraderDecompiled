using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Owlcat.Runtime.Visual.Collections;

[BurstCompile]
public static class UnsafeCollectionExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static UnsafeSpan<T> AsUnsafeSpan<T>(this in NativeArray<T> array) where T : unmanaged
	{
		return new UnsafeSpan<T>((T*)array.GetUnsafePtr(), array.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static UnsafeSpan<T> AsUnsafeSpan<T>(this in NativeList<T> list) where T : unmanaged
	{
		return new UnsafeSpan<T>(list.GetUnsafePtr(), list.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T AsRef<T>(this in NativeReference<T> reference) where T : unmanaged
	{
		return ref *reference.GetUnsafePtr();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref readonly T AsReadOnlyRef<T>(this in NativeReference<T> reference) where T : unmanaged
	{
		return ref *reference.GetUnsafeReadOnlyPtr();
	}
}
