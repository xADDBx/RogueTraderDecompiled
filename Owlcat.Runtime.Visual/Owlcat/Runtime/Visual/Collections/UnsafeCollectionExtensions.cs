using System;
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
		return new UnsafeSpan<T>((T*)list.GetUnsafePtr(), list.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T AsRef<T>(this in NativeReference<T> reference) where T : unmanaged
	{
		return ref *(T*)reference.GetUnsafePtr();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref readonly T AsReadOnlyRef<T>(this in NativeReference<T> reference) where T : unmanaged
	{
		return ref *(T*)reference.GetUnsafeReadOnlyPtr();
	}

	public unsafe static bool TryGetValueUnsafePointer<TKey, TValue>(this in NativeHashMap<TKey, TValue> map, in TKey key, out TValue* valuePtr) where TKey : struct, IEquatable<TKey> where TValue : unmanaged
	{
		if (map.IsEmpty)
		{
			valuePtr = null;
			return false;
		}
		UnsafeHashMapBucketData unsafeBucketData = map.GetUnsafeBucketData();
		int capacity = map.Capacity;
		int* buckets = (int*)unsafeBucketData.buckets;
		int num = key.GetHashCode() & unsafeBucketData.bucketCapacityMask;
		int num2 = buckets[num];
		if (num2 < 0 || num2 >= capacity)
		{
			valuePtr = null;
			return false;
		}
		int* next = (int*)unsafeBucketData.next;
		while (!UnsafeUtility.ReadArrayElement<TKey>(unsafeBucketData.keys, num2).Equals(key))
		{
			num2 = next[num2];
			if (num2 < 0 || num2 >= capacity)
			{
				valuePtr = null;
				return false;
			}
		}
		valuePtr = (TValue*)(void*)((IntPtr)unsafeBucketData.values + num2 * sizeof(TValue));
		return true;
	}
}
